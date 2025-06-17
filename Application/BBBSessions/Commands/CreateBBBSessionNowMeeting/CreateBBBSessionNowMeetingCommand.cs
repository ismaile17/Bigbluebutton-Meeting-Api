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
using System.Data;

namespace Application.BBBSessions.Commands.CreateNowMeeting
{
    public class CreateBBBSessionNowMeetingCommand : IRequest<ResultSingle<CreateBBBSessionDto>>
    {
        public int AppUserId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public GuestPolicy GuestPolicy { get; set; }
        public List<int> MeetingGroupIds { get; set; }
        public List<string> MeetingGuestEmailList { get; set; }

    }

    public class CreateNowMeetingCommandHandler : IRequestHandler<CreateBBBSessionNowMeetingCommand, ResultSingle<CreateBBBSessionDto>>
    {
        private readonly IRepository<Meeting> _meetingRepository;
        private readonly IRepository<Package> _packageRepository;
        private readonly IMapper _mapper;
        private readonly BigBlueButtonAPIClient _client;
        private readonly IRepository<BBBServer> _bbbServer;
        private readonly IRepository<Domain.Entities.CompletedMeeting> _complatedMeeting;
        private readonly UserManager<AppUser> _userManager;
        private readonly IClientInfoService _clientInfoService;

        public CreateNowMeetingCommandHandler(IRepository<Meeting> meetingRepository, IRepository<Package> packageRepository, IMapper mapper, BigBlueButtonAPIClient client, IRepository<BBBServer> bbbServer, IRepository<Domain.Entities.CompletedMeeting> complatedMeeting, UserManager<AppUser> userManager, IClientInfoService clientInfoService)
        {
            _meetingRepository = meetingRepository;
            _packageRepository = packageRepository;
            _mapper = mapper;
            _client = client;
            _bbbServer = bbbServer;
            _complatedMeeting = complatedMeeting;
            _userManager = userManager;
            _clientInfoService = clientInfoService;
        }

        public async Task<ResultSingle<CreateBBBSessionDto>> Handle(CreateBBBSessionNowMeetingCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                                         .Include(u => u.UserDetailSetting)
                                         .FirstOrDefaultAsync(u => u.Id == request.AppUserId);

            var package = _packageRepository.GetById(user.PackageId ?? 1);


            if (user == null)
            {
                return Result.Fail<CreateBBBSessionDto>(null, "Kullanıcı bulunamadı");
            }

            var userDetailSetting = user.UserDetailSetting ?? new UserDetailSetting
            {
                WelcomeMessage = "Toplantı Saati Hoşgeldiniz",
                RecordTF = true,
                Duration = package.MaxDurationMinutes,
                WebcamsOnlyForModerator = true,
                BannerText = "ToplantiSaati.com",
                LockSettingsDisableCam = true,
                LockSettingsDisableMic = true,
                LockSettingsDisablePrivateChat = true,
                LockSettingsDisablePublicChat = true,
                LockSettingsHideUserList = true,
                GuestPolicy = request.GuestPolicy,
                AllowModsToEjectCameras = true,
                AllowRequestsWithoutSession = true,
                MeetingCameraCap = 10,
                Logo = "default_logo.png",
                RecordVisibility = RecordVisibility.MODERATORS                
            };

            // BigBlueButton API ayarlarını al
            var serverUrlAndSecret = _bbbServer.GetMany(m => m.Id == user.BBBServerId).FirstOrDefault();
            if (serverUrlAndSecret == null)
            {
                serverUrlAndSecret = _bbbServer.GetMany(m => m.MainServer == true)
                                                .OrderByDescending(m => m.CreatedTime)
                                                .FirstOrDefault();
            }

            if (serverUrlAndSecret == null)
            {
                return Result.Fail<CreateBBBSessionDto>(null, "Sunucu bilgisi bulunamadı");
            }

            var settings = new BigBlueButtonAPISettings
            {
                ServerAPIUrl = serverUrlAndSecret.ServerApiUrl,
                SharedSecret = serverUrlAndSecret.SharedSecret
            };

            var apiClient = new BigBlueButtonAPIClient(settings, new HttpClient());

            var attendeePassword = Guid.NewGuid();
            var moderatorPassword = Guid.NewGuid();
            var randomMeetingId = Guid.NewGuid();

            var createMeetingRequest = new CreateMeetingRequest
            {
                meetingID = randomMeetingId.ToString(),
                name = request.Name,
                attendeePW = attendeePassword.ToString(),
                moderatorPW = moderatorPassword.ToString(),
                welcome = userDetailSetting.WelcomeMessage,
                maxParticipants = package.MaxParticipants ?? 10,
                logoutURL = "www.toplantisaati.com",
                record = userDetailSetting.RecordTF ?? true,
                duration = package.MaxDurationMinutes,
                webcamsOnlyForModerator = userDetailSetting.WebcamsOnlyForModerator ?? false,
                logo = userDetailSetting.Logo,
                bannerText = userDetailSetting.BannerText,
                copyright = "Toplantı Saati ®",
                lockSettingsDisableCam = userDetailSetting.LockSettingsDisableCam ?? false,
                lockSettingsDisableMic = userDetailSetting.LockSettingsDisableMic ?? false,
                lockSettingsDisablePrivateChat = userDetailSetting.LockSettingsDisablePrivateChat ?? false,
                lockSettingsDisablePublicChat = userDetailSetting.LockSettingsDisablePublicChat ?? false,
                guestPolicy = request.GuestPolicy.ToString() ?? userDetailSetting.GuestPolicy.ToString(),
            };

            var createMeetingResponse = await apiClient.CreateMeetingAsync(createMeetingRequest);

            if (createMeetingResponse.returncode == Returncode.FAILED)
            {
                return Result.Fail<CreateBBBSessionDto>(null, $"CreateMeetingAsync Çalışmadı.");
            }

            var guestJoinURL = apiClient.GetJoinMeetingUrl(new JoinMeetingRequest
            {
                meetingID = randomMeetingId.ToString(),
                fullName = "Misafir",
                password = attendeePassword.ToString(),
                guest = true
            });

            var meeting = new Meeting
            {
                CreatedBy = request.AppUserId,
                Name = request.Name,
                UserId = request.AppUserId,
                AllowModsToEjectCameras = userDetailSetting.AllowModsToEjectCameras ?? false,
                AllowRequestsWithoutSession = userDetailSetting.AllowRequestsWithoutSession ?? false,
                BannerText = userDetailSetting.BannerText,
                Description = request.Description,
                Duration = userDetailSetting.Duration ?? 0,
                GuestPolicy = request.GuestPolicy,
                LockSettingsDisableCam = userDetailSetting.LockSettingsDisableCam ?? false,
                LockSettingsDisableMic = userDetailSetting.LockSettingsDisableMic ?? false,
                LockSettingsDisablePrivateChat = userDetailSetting.LockSettingsDisablePrivateChat ?? false,
                LockSettingsDisablePublicChat = userDetailSetting.LockSettingsDisablePublicChat ?? false,
                LockSettingsHideUserList = userDetailSetting.LockSettingsHideUserList ?? false,
                Logo = userDetailSetting.Logo,
                MeetingCameraCap = userDetailSetting.MeetingCameraCap ?? 0,
                RecordTF = userDetailSetting.RecordTF ?? false,
                StartTime = DateTime.Now.TimeOfDay,
                StartDate = DateTime.Now,
                EndDate = null,
                WebcamsOnlyForModerator = userDetailSetting.WebcamsOnlyForModerator ?? false,
                WelcomeMessage = userDetailSetting.WelcomeMessage,
                SingleOrRepeated = true,
                CreatedTime = DateTime.Now,
                ScheduleOrNowMeeting = ScheduleOrNowMeeting.NOW_MEETING,
                MeetingMeetingGroups = new List<MeetingMeetingGroup>(),
                MeetingGuestEmailLists = new List<MeetingGuestEmailList>(),
                MeetingScheduleDateLists = new List<MeetingScheduleDateList>() // Yeni eklenecek olan schedule listesi

            };

            foreach (var group in request.MeetingGroupIds)
            {
                meeting.MeetingMeetingGroups.Add(new MeetingMeetingGroup { Meeting = meeting, MeetingGroupId = group });
            }

            foreach (var email in request.MeetingGuestEmailList)
            {
                if (!string.IsNullOrEmpty(email))
                {
                    meeting.MeetingGuestEmailLists.Add(new MeetingGuestEmailList { Meeting = meeting, Email = email });
                }
            }


            var meetingResult = await _meetingRepository.CommitAsync(cancellationToken);

            if (meetingResult == -1)
            {
                return Result.Fail<CreateBBBSessionDto>(null, "Meeting kaydedilemedi.");
            }

            var (ipAddress, port) = _clientInfoService.GetClientIpAndPort();


            try
            {
                // Meeting ID'sini aldıktan sonra CompletedMeeting'i kaydet
                var complatedMeeting = new Domain.Entities.CompletedMeeting
                {
                    MeetingId = meeting.Id,
                    BBBMeetingId = createMeetingRequest.meetingID,
                    MeetingType = 2,
                    UserId = request.AppUserId,
                    AttendeePassword = attendeePassword.ToString(),
                    ModeratorPassword = moderatorPassword.ToString(),
                    ServerId = serverUrlAndSecret.Id,
                    Name = meeting.Name,
                    Description = meeting.Description,
                    WelcomeMessage = meeting.WelcomeMessage,
                    RecordTF = meeting.RecordTF ?? false,
                    StartDate = meeting.StartDate,
                    StartTime = meeting.StartDate,
                    EndDate = meeting.EndDate,
                    Duration = meeting.Duration ?? 0,
                    WebcamsOnlyForModerator = meeting.WebcamsOnlyForModerator ?? false,
                    BannerText = meeting.BannerText,
                    LockSettingsDisableCam = meeting.LockSettingsDisableCam ?? false,
                    LockSettingsDisableMic = meeting.LockSettingsDisableMic ?? false,
                    LockSettingsDisablePrivateChat = meeting.LockSettingsDisablePrivateChat ?? false,
                    LockSettingsDisablePublicChat = meeting.LockSettingsDisablePublicChat ?? false,
                    LockSettingsHideUserList = meeting.LockSettingsHideUserList ?? false,
                    GuestPolicy = request.GuestPolicy,
                    AllowModsToEjectCameras = meeting.AllowModsToEjectCameras ?? false,
                    AllowRequestsWithoutSession = meeting.AllowRequestsWithoutSession ?? false,
                    MeetingCameraCap = meeting.MeetingCameraCap ?? 0,
                    Logo = meeting.Logo,
                    SingleOrRepeated = meeting.SingleOrRepeated,
                    CreatedBy = user.Id,
                    CreatedTime = DateTime.Now,
                    GuestLink = guestJoinURL,
                    MeetingExplain = request.Description,
                    InternalMeetingId = createMeetingResponse.internalMeetingID,
                    StartIpAddress = ipAddress,
                    StartPort = port,
                    CreateStartOrFinish = CompletedMeetingCreateStartOrFinish.Start,
                    RecordVisibility = meeting.RecordVisibility ?? RecordVisibility.MODERATORS,
                    ScheduleOrNowMeeting = meeting.ScheduleOrNowMeeting
                };

                _complatedMeeting.InsertWithoutCommit(complatedMeeting);
                var complatedMeetingResult = await _complatedMeeting.CommitAsync(cancellationToken);

                if (complatedMeetingResult == -1)
                {
                    // Eğer CompletedMeeting kaydında bir hata olursa, Meeting kaydını sil
                    _meetingRepository.Delete(meeting);
                    await _meetingRepository.CommitAsync(cancellationToken);
                    return Result.Fail<CreateBBBSessionDto>(null, "complatedMeeting kaydedilemedi ve Meeting silindi.");
                }

                var sessionDto = new CreateBBBSessionDto
                {
                    Id = complatedMeeting.Id.ToString()
                };

                var newScheduleDate = new MeetingScheduleDateList
                {
                    MeetingId = meeting.Id,
                    Date = DateOnly.FromDateTime(DateTime.Now), // Bugünün tarihini atama
                    DidHappen = true,
                    CompletedMeetingId = complatedMeeting.Id,
                };

                // Önce Meeting'i kaydet ve ID'sini al
                meeting.MeetingScheduleDateLists.Add(newScheduleDate);
                _meetingRepository.InsertWithoutCommit(meeting);

                return ResultSingle<CreateBBBSessionDto>.Ok(sessionDto, "Meeting created successfully.");
            }
            catch (Exception ex)
            {
                // Eğer bir istisna fırlatılırsa, Meeting kaydını sil
                _meetingRepository.Delete(meeting);
                await _meetingRepository.CommitAsync(cancellationToken);
                return Result.Fail<CreateBBBSessionDto>(null, $"Bir hata oluştu: {ex.Message} ve Meeting silindi.");
            }
        }

    }
}