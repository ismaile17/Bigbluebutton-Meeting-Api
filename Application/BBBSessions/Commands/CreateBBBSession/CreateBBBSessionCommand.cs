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

namespace Application.BBBSessions.Commands.CreateBBBSession
{
    public class CreateBBBSessionCommand:IRequest<ResultSingle<CreateBBBSessionDto>>
    {
        public int AppUserId { get; set; }
        public int MeetingId { get; set; }
        public int MeetingType { get; set; }
        public GuestPolicy GuestPolicy { get; set; }
        public string? MeetingExplain { get; set; }
    }

    public class CreateBBBSessionCommandHandler:IRequestHandler<CreateBBBSessionCommand, ResultSingle<CreateBBBSessionDto>> 
    {
        private readonly BigBlueButtonAPIClient _client;
        private readonly IRepository<Meeting> _meetingRepository;
        private readonly IRepository<Package> _packageRepository;
        private readonly IMapper _mapper;
        private readonly IRepository<BBBServer> _bbbServer;
        private readonly IRepository<Domain.Entities.CompletedMeeting> _complatedMeeting;
        private readonly UserManager<AppUser> _userManager;
        private readonly IClientInfoService _clientInfoService;

        public CreateBBBSessionCommandHandler(BigBlueButtonAPIClient client, IRepository<Meeting> meetingRepository, IRepository<Package> packageRepository, IMapper mapper, IRepository<BBBServer> bbbServer, IRepository<Domain.Entities.CompletedMeeting> complatedMeeting, UserManager<AppUser> userManager, IClientInfoService clientInfoService)
        {
            _client = client;
            _meetingRepository = meetingRepository;
            _packageRepository = packageRepository;
            _mapper = mapper;
            _bbbServer = bbbServer;
            _complatedMeeting = complatedMeeting;
            _userManager = userManager;
            _clientInfoService = clientInfoService;
        }

        public async Task<ResultSingle<CreateBBBSessionDto>> Handle(CreateBBBSessionCommand request, CancellationToken cancellationToken)
        {
            var appUserId = request.AppUserId;
            var meetingId = request.MeetingId;
            var MeetingType = request.MeetingType;

            //1 == Scheduled yani 1 ise planlanmış, meeting tablosuna önceden işlenmiş veri.
            //2 == Unplanned 2 ise hemen alelacele oluşturulmuş veri.

            var user = await _userManager.FindByIdAsync(request.AppUserId.ToString());

            var package = _packageRepository.GetById(user.PackageId ?? 1);
            

            if (user == null)
            {
                return Result.Fail<CreateBBBSessionDto>(null, $"Kullanıcı bulunamadı");
            }

            var selectedMeeting = _meetingRepository.GetMany(m => m.UserId == appUserId && m.Id == meetingId).FirstOrDefault();

            if (selectedMeeting == null)
            {
                return Result.Fail<CreateBBBSessionDto>(null, $"Toplantı bulunamadı");
            }



            //ÖNCE TOPLANTIYI CREATE EDİP BİGBLUEBUTTON DAN GELEN MEETİNGID Yİ DE COMPLATEDMEETİNG E YAZMAMIZ GEREKTİĞİNİ DÜŞÜNÜYORUM.!

            //BURADA GELEN VE ALINAN BİLGİLER DOĞRULTUSUNDA BIGBLUEBUTTON TOPLANTISI CREATE EDİLECEK VE SONRASINDA TOPLANTI GİRİŞ LİNKİ DÖNDÜRÜLECEK.

            var serverUrlAndSecret = _bbbServer.GetMany(m=> m.Id == user.BBBServerId).FirstOrDefault();

            //eğer kişiye atanmış bir server yoksa onu main server olarak en son eklenmiş olan serverden çıkartacağız.
            if (serverUrlAndSecret == null)
            {
                serverUrlAndSecret = _bbbServer.GetMany(m => m.MainServer == true)
                                                  .OrderByDescending(m => m.CreatedTime)
                                                  .FirstOrDefault();
            }

            // BigBlueButton API ayarlarını al
            var settings = new BigBlueButtonAPISettings
            {
                ServerAPIUrl = serverUrlAndSecret.ServerApiUrl,
                SharedSecret = serverUrlAndSecret.SharedSecret
            };

            // BigBlueButton API istemcisini oluştur
            var apiClient = new BigBlueButtonAPIClient(settings, new HttpClient());


            // Var olan bir tamamlanmış toplantı olup olmadığını kontrol et
            var today2 = DateTime.Now.Date;
            var existingMeeting = _complatedMeeting
                .GetMany(cm => cm.UserId == appUserId && cm.MeetingId == meetingId && cm.CreatedTime >= today2 && cm.CreatedTime < today2.AddDays(1))
                .OrderByDescending(cm => cm.Id)
                .FirstOrDefault();



            if (existingMeeting != null)
            {
                // BBB tarafında toplantı çalışıyor mu kontrol et
                var isMeetingRunningRequest = new IsMeetingRunningRequest { meetingID = existingMeeting.BBBMeetingId };

                var isMeetingRunningResponse = await apiClient.IsMeetingRunningAsync(isMeetingRunningRequest);

                if (isMeetingRunningResponse.returncode == Returncode.SUCCESS && isMeetingRunningResponse.running.HasValue && isMeetingRunningResponse.running.Value)
                {
                    // Mevcut toplantı var ve çalışıyor, kullanıcıyı bu toplantıya yönlendir
                    var sessionDtoExisting = new CreateBBBSessionDto
                    {
                        Id = existingMeeting.CompletedGuid.ToString(),
                        Link = existingMeeting.GuestLink
                    };

                    return ResultSingle<CreateBBBSessionDto>.Ok(sessionDtoExisting, "Existing meeting found, redirecting to it.");
                }
            }


            string GenerateRandomString(int length)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                var random = new Random();
                var randomString = new char[length];

                for (int i = 0; i < length; i++)
                {
                    randomString[i] = chars[random.Next(chars.Length)];
                }

                return new string(randomString);
            }

            // Katılımcı şifresi oluştur
            var attendeePassword = GenerateRandomString(20);

            // Yönetici şifresi oluştur
            var moderatorPassword = GenerateRandomString(20);

            //meetingID oluştur
            var randomMeetingId = Guid.NewGuid();

            // BigBlueButton API üzerinden toplantı oluştur
            var createMeetingRequest = new CreateMeetingRequest
            {
                meetingID = randomMeetingId.ToString(),
                name = selectedMeeting.Name,
                attendeePW = attendeePassword, // Katılımcı şifresi
                moderatorPW = moderatorPassword, // Yönetici şifresi
                welcome = selectedMeeting.WelcomeMessage ?? "Toplantı Saati Uygulamasına Hoşgeldiniz.",
                maxParticipants = package.MaxParticipants,
                logoutURL = "www.toplantisaati.com", //BURASI ALINAN DOMAİNE GÖRE SONRADAN DOLDURULACAK.
                record = selectedMeeting.RecordTF,
                duration = selectedMeeting.Duration,
                webcamsOnlyForModerator = selectedMeeting.WebcamsOnlyForModerator,
                logo = selectedMeeting.Logo,
                bannerText = selectedMeeting.BannerText ?? "Toplantı Saati",
                copyright = "Toplantı Saati ®",
                lockSettingsDisableCam = selectedMeeting.LockSettingsDisableCam,
                lockSettingsDisableMic = selectedMeeting.LockSettingsDisableMic,
                lockSettingsDisablePrivateChat = selectedMeeting.LockSettingsDisablePrivateChat,
                lockSettingsDisablePublicChat = selectedMeeting.LockSettingsDisablePublicChat,
                guestPolicy = request.GuestPolicy.ToString() ?? "ASK_MODERATOR"
                //voiceBridge = 123123
            };

            var createMeetingResponse = await apiClient.CreateMeetingAsync(createMeetingRequest);

            if (createMeetingResponse.returncode == Returncode.FAILED)
            {
                return Result.Fail<CreateBBBSessionDto>(null, $"CreateMeetingAsync Çalışmadı.");
            }


            //eğer kişi public seçtiyse ona link ile misafirlerini çağırması için bu linki göstereceğiz.

            var guestMeetingLink = (new JoinMeetingRequest
            {
                meetingID = randomMeetingId.ToString(),
                fullName = "Guest",
                password = attendeePassword,
                guest = true
            });

            var guestJoinURL = apiClient.GetJoinMeetingUrl(guestMeetingLink);


            //burada gelen verileri gerçekleşen meetingler kısmına kaydetmemiz lazım sonrasında toplantıyı oluşturup başlatmamız lazım.
            //burayı ayrıca kayıt etmemizin sebebi, adamın planladığı toplantıdaki ayarlarını değiştirip bir sonraki toplantıyı başlatabilecek olması. Bu yüzden her ayarı düzgünce kayıt edip kişiye göstermeliyiz.

            var (ipAddress, port) = _clientInfoService.GetClientIpAndPort();


            var complatedMeeting = new Domain.Entities.CompletedMeeting
            {
                MeetingId = meetingId,
                BBBMeetingId = createMeetingRequest.meetingID,
                MeetingType = request.MeetingType,
                UserId = request.AppUserId,
                AttendeePassword = attendeePassword,
                ModeratorPassword = moderatorPassword,
                ServerId = serverUrlAndSecret.Id,
                Name = selectedMeeting.Name,
                Description = selectedMeeting.Description,
                WelcomeMessage = selectedMeeting.WelcomeMessage,
                RecordTF = selectedMeeting.RecordTF,
                StartDate = selectedMeeting.StartDate,
                StartTime = selectedMeeting.StartDate,
                EndDate = selectedMeeting.EndDate,
                Duration = selectedMeeting.Duration,
                WebcamsOnlyForModerator = selectedMeeting.WebcamsOnlyForModerator,
                BannerText = selectedMeeting.BannerText,
                LockSettingsDisableCam = selectedMeeting.LockSettingsDisableCam,
                LockSettingsDisableMic = selectedMeeting.LockSettingsDisableMic,
                LockSettingsDisablePrivateChat = selectedMeeting.LockSettingsDisablePrivateChat,
                LockSettingsDisablePublicChat = selectedMeeting.LockSettingsDisablePublicChat,
                LockSettingsHideUserList = selectedMeeting.LockSettingsHideUserList,
                GuestPolicy = selectedMeeting.GuestPolicy,
                AllowModsToEjectCameras = selectedMeeting.AllowModsToEjectCameras,
                AllowRequestsWithoutSession = selectedMeeting.AllowRequestsWithoutSession,
                MeetingCameraCap = selectedMeeting.MeetingCameraCap,
                Logo = selectedMeeting.Logo,
                CreatedBy = user.Id,
                CreatedTime = DateTime.Now,
                GuestLink = guestJoinURL,
                MeetingExplain = request.MeetingExplain,
                InternalMeetingId = createMeetingResponse.internalMeetingID,
                StartIpAddress = ipAddress,
                StartPort = port,
                CreateStartOrFinish = CompletedMeetingCreateStartOrFinish.Create,
                RecordVisibility = selectedMeeting.RecordVisibility
            };

            //ScheduleDate tablosuna true atacak değer

            var today = DateOnly.FromDateTime(DateTime.Now); // Bugünün tarihi DateOnly formatında alınıyor
            var meetingToUpdate = _meetingRepository.GetMany(m => m.Id == request.MeetingId)
                .Include(m => m.MeetingScheduleDateLists) // ScheduleDates ilişkisi dahil ediliyor
                .FirstOrDefault();

            if (meetingToUpdate != null)
            {
                var scheduleToUpdate = meetingToUpdate.MeetingScheduleDateLists
                    .FirstOrDefault(s => s.Date == today); // Bugünkü tarihe sahip olan schedule kaydı aranıyor

                if (scheduleToUpdate != null)
                {
                    scheduleToUpdate.DidHappen = true;
                    scheduleToUpdate.CompletedMeetingId = complatedMeeting.Id;
                    _meetingRepository.UpdateWithoutCommit(meetingToUpdate); // Meeting tablosu üzerinden güncelleme
                    var updateResult = await _meetingRepository.CommitAsync(cancellationToken);

                    if (updateResult == -1)
                    {
                        return Result.Fail<CreateBBBSessionDto>(null, $"Schedule DidHappen alanı güncellenemedi.");
                    }
                }
                else
                {
                    var newScheduleDate = new MeetingScheduleDateList
                    {
                        MeetingId = meetingId,
                        Date = DateOnly.FromDateTime(DateTime.Now), // Bugünün tarihini atama
                        DidHappen = true,
                        CompletedMeetingId = complatedMeeting.Id
                    };

                    // Yeni scheduleDate'i mevcut meeting kaydına ekleyin
                    meetingToUpdate.MeetingScheduleDateLists.Add(newScheduleDate);

                    // Ardından meeting kaydını güncelleyin
                    _meetingRepository.UpdateWithoutCommit(meetingToUpdate);
                    var updateResult = await _meetingRepository.CommitAsync(cancellationToken);

                    if (updateResult == -1)
                    {
                        return Result.Fail<CreateBBBSessionDto>(null, $"Schedule alanı eklenemedi.");
                    }
                }
            }
            else
            {
                return Result.Fail<CreateBBBSessionDto>(null, $"Toplantı bulunamadı.");
            }


            _complatedMeeting.InsertWithoutCommit(complatedMeeting);
            var complatedMeetingResult = await _complatedMeeting.CommitAsync(cancellationToken);
            if (complatedMeetingResult == -1)
            {
                return Result.Fail<CreateBBBSessionDto>(null, $"complatedMeetingResult atanamadı");
            }

            var sessionDto = new CreateBBBSessionDto
            {
                Id = complatedMeeting.Id.ToString(),  // Burada BigBlueButton tarafından dönen meeting ID kullanılıyor
            };


            return ResultSingle<CreateBBBSessionDto>.Ok(sessionDto, "Meeting created successfully.");

        }

    }

}
