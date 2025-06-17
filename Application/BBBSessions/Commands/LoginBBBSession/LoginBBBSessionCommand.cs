using Application.BBBSessions.Model;
using Application.Services.ClientInfoService;
using Application.Shared.Results;
using AutoMapper;
using BigBlueButtonAPI.Core;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Enum;

namespace Application.BBBSessions.Commands.LoginBBBSession
{
    public class LoginBBBSessionCommand:IRequest<ResultSingle<LoginBBBSessionDto>>
    {
        public int? AppUserId { get; set; }
        public Guid CompletedMeetingGuid { get; set; }
        public int MeetingType { get; set; }
        public int MeetingEntryType { get; set; } // 1 login girişi 2 ise guest girişi
        public string GuestFullName { get; set; }
    }

    public class LoginBBBSessionCommandHandler : IRequestHandler<LoginBBBSessionCommand, ResultSingle<LoginBBBSessionDto>>
    {
        private readonly BigBlueButtonAPIClient _client;
        private readonly IRepository<Meeting> _meetingRepository;
        private readonly IRepository<LoginUserDetailInfo> _loginDetailInfoRepository;
        private readonly IMapper _mapper;
        private readonly IRepository<BBBServer> _bbbServer;
        private readonly IRepository<Domain.Entities.CompletedMeeting> _complatedMeeting;
        private readonly UserManager<AppUser> _userManager;
        private readonly IClientInfoService _clientInfoService;

        public LoginBBBSessionCommandHandler(BigBlueButtonAPIClient client, IRepository<Meeting> meetingRepository, IRepository<LoginUserDetailInfo> loginDetailInfoRepository, IMapper mapper, IRepository<BBBServer> bbbServer, IRepository<Domain.Entities.CompletedMeeting> complatedMeeting, UserManager<AppUser> userManager, IClientInfoService clientInfoService)
        {
            _client = client;
            _meetingRepository = meetingRepository;
            _loginDetailInfoRepository = loginDetailInfoRepository;
            _mapper = mapper;
            _bbbServer = bbbServer;
            _complatedMeeting = complatedMeeting;
            _userManager = userManager;
            _clientInfoService = clientInfoService;
        }

        public async Task<ResultSingle<LoginBBBSessionDto>> Handle(LoginBBBSessionCommand request, CancellationToken cancellationToken)
        {
            var completedMeeting = _complatedMeeting.GetMany(m => m.CompletedGuid == request.CompletedMeetingGuid).FirstOrDefault();

            if (completedMeeting == null)
            {
                return Result.Fail<LoginBBBSessionDto>(null, $"ComplatedMeeting bulunamadı");
            }

            if(completedMeeting.CreatedBy == request.AppUserId && completedMeeting.SingleOrRepeated == true)
            {
                completedMeeting.CreateStartOrFinish = CompletedMeetingCreateStartOrFinish.Start;
                completedMeeting.UpdatedTime = DateTime.Now;
                completedMeeting.UpdatedBy = request.AppUserId;
                try
                {
                    _complatedMeeting.UpdateWithoutCommit(completedMeeting);
                }
                catch (Exception)
                {

                    return Result.Fail<LoginBBBSessionDto>(null, $"completedMeeting update edilemedi.");
                }
            }

            if (completedMeeting.CreateStartOrFinish != CompletedMeetingCreateStartOrFinish.Start && completedMeeting.CreatedBy != request.AppUserId)
            {
                return Result.Fail<LoginBBBSessionDto>(null, $"Bu toplantı henüz başlamadı!");
            }

            AppUser user = null;

            if (request.AppUserId.HasValue || request.MeetingEntryType == 1)
            {
                user = await _userManager.FindByIdAsync(request.AppUserId.Value.ToString());
                if (user == null && request.MeetingEntryType == 1)
                {
                    // Kullanıcı bulunamadıysa hata döndür
                    return Result.Fail<LoginBBBSessionDto>(null, "Kullanıcı bulunamadı. Bu toplantıya giriş yaparak ulaşabilirsiniz.");
                }
            }
            else
            {
                // Misafir kullanıcı için benzersiz bir ID oluştur
                int guestId = GenerateGuestId();

                user = new AppUser
                {
                    Id = guestId,
                    UserName = request.GuestFullName
                    // Diğer gerekli özellikleri burada ayarlayabilirsiniz
                };
            }

            if (user == null && completedMeeting.GuestPolicy == GuestPolicy.ALWAYS_DENY)
            {
                return Result.Fail<LoginBBBSessionDto>(null, $"Bu toplantıya sadece giriş yapılarak erişilebilir.");
            }



            var serverUrl = _bbbServer.GetMany(b => b.Id == completedMeeting.ServerId).FirstOrDefault();


            var settings = new BigBlueButtonAPISettings
            {
                ServerAPIUrl = serverUrl.ServerApiUrl,
                SharedSecret = serverUrl.SharedSecret
            };

            // BigBlueButton API istemcisini oluştur
            var apiClient = new BigBlueButtonAPIClient(settings, new HttpClient());

            var isMeetingRunningResponse = (new IsMeetingRunningRequest
            {
                meetingID = completedMeeting.BBBMeetingId
            });

            var runningControl = await apiClient.IsMeetingRunningAsync(isMeetingRunningResponse);

            if (runningControl.returncode == Returncode.FAILED)
            {
                return Result.Fail<LoginBBBSessionDto>(null, $"IsMeetingRunningAsync Bu Toplantı Yok.");
            }

            var meeting = _meetingRepository.GetMany(m => m.Id == completedMeeting.MeetingId).FirstOrDefault();

            if (meeting == null)
            {
                return Result.Fail<LoginBBBSessionDto>(null, "Meeting bulunamadı");
            }

            //user oluşturan admin mi, katılımcı participant mı yoksa o meetinge admin olarak atanmış bir user mi kontrolü sağlanacak.

            int thisModerator = 0;

            if (request.MeetingEntryType == 1)
            {
                if (user.Id == meeting.UserId)
                {
                    thisModerator = 1;
                }
            }
            else
            {
                thisModerator = 2;
            }


            if (completedMeeting.GuestPolicy != GuestPolicy.ALWAYS_DENY && request.MeetingEntryType == 2)
            {
                thisModerator = 2;
            }


            if (thisModerator != 1 && request.MeetingEntryType == 1)
                {
                        var meetingWithGroupsAndUsersAndModerators = _meetingRepository
                        .GetMany(m => m.Id == completedMeeting.MeetingId)
                        .Include(m => m.MeetingMeetingGroups)
                            .ThenInclude(mm => mm.MeetingGroup)
                                .ThenInclude(mg => mg.MeetingGroupUserLists)
                                    .ThenInclude(mgu => mgu.AppUser)
                        .Include(m => m.MeetingModeratorLists)
                        .FirstOrDefault();

                        if (meetingWithGroupsAndUsersAndModerators == null)
                        {
                            // Hata: Toplantı bulunamadı
                            return Result.Fail<LoginBBBSessionDto>(null, "Katılımcı bulunamadı");
                        }

                        var userIsModerator = meetingWithGroupsAndUsersAndModerators.MeetingModeratorLists.Any(m => m.AppUserId == user.Id);

                        if (userIsModerator)
                        {
                            // Kullanıcı toplantının doğrudan moderatörü
                            thisModerator = 1;
                        }

                        if (thisModerator != 1)
                        {
                            var userIsGroupModerator = meetingWithGroupsAndUsersAndModerators.MeetingMeetingGroups
                                .Any(group => group.MeetingGroup.MeetingGroupUserLists.Any(mgu => mgu.AppUserId == user.Id));

                            if (userIsGroupModerator)
                            {
                                // Kullanıcı grubun düz üyesi
                                thisModerator = 2;
                            }
                            else
                            {
                                return Result.Fail<LoginBBBSessionDto>(null, "Katılımcı bu grupta yetkili değil!");
                            }
                        }
                }

            string meetingModOrAttPassword;

                if(thisModerator == 1)
                {
                    meetingModOrAttPassword = completedMeeting.ModeratorPassword;
                }
                else if(thisModerator == 2)
                {
                meetingModOrAttPassword = completedMeeting.AttendeePassword;
                }
                else
                {
                    return Result.Fail<LoginBBBSessionDto>(null, "Katılımcı bu grupta yetkili değil!");
                }

            //BU AŞAMADA KİŞİ GRUP ÜYESİ Mİ, YARATICI MI YOKSA MODERATOR MÜ KONTROL ETTİK. ONA GÖRE ŞİFRE ATANDI. GÖNDERİRKEN BUNU GÖNDERECEĞİZ.

            var (ipAddress, port) = _clientInfoService.GetClientIpAndPort();

            var loginUserDetailInfo = new LoginUserDetailInfo
            {
                MeetingGuid = completedMeeting.CompletedGuid.ToString(),
                MeetingId = completedMeeting.MeetingId,
                IpAddress = ipAddress,
                Port = port,
                UserId = user.Id,
                UserName = user.UserName,
            };

            _loginDetailInfoRepository.InsertWithoutCommit(loginUserDetailInfo);
            await _loginDetailInfoRepository.CommitAsync(cancellationToken);


            bool guest = false;
            if(request.MeetingEntryType == 2)
            {
                guest = true;
            }
            var meetingLink = (new JoinMeetingRequest
                {
                    meetingID = completedMeeting.BBBMeetingId,
                    fullName = user.UserName,
                    password = meetingModOrAttPassword,
                    userID = user.Id.ToString(),
                    guest = guest
                });

            var JoinURL = apiClient.GetJoinMeetingUrl(meetingLink);


            //var serverUrl2 = _bbbServer.GetMany(b => b.Id == completedMeeting.ServerId).Select(b => b.ServerApiUrl).FirstOrDefault();
            //var fullLink = serverUrl2 + meetingLink;

            if (string.IsNullOrEmpty(JoinURL))
            {
                return Result.Fail<LoginBBBSessionDto>(null, "Toplantıya katılım bağlantısı alınamadı");
            }

            return new ResultSingle<LoginBBBSessionDto>(new LoginBBBSessionDto { Link = JoinURL, Id = completedMeeting.BBBMeetingId }, true, "İşlem başarıyla tamamlandı");

        }

        private int GenerateGuestId()
        {
            const int baseId = 99999;
            Random random = new Random();
            int randomNumber = random.Next(10000, 99999); // 10000 ile 99999 arasında rastgele sayı
            return baseId + randomNumber; // 9999900000 + rastgele sayı
        }
    }
}