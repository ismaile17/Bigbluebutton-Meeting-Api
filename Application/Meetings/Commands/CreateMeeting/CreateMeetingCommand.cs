using Application.Meetings.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Enum;
using System.Globalization;

namespace Application.Meetings.Commands.CreateMeeting
{
    public class CreateMeetingCommand : IRequest<ResultSingle<MeetingDto>>
    {
        public int AppUserId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? WelcomeMessage { get; set; }
        public bool? RecordTF { get; set; }
        public string? StartDate { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? EndDate { get; set; }
        public int? Duration { get; set; }
        public bool? WebcamsOnlyForModerator { get; set; }
        public string? BannerText { get; set; }
        public bool? LockSettingsDisableCam { get; set; }
        public bool? LockSettingsDisableMic { get; set; }
        public bool? LockSettingsDisablePrivateChat { get; set; }
        public bool? LockSettingsDisablePublicChat { get; set; }
        public bool? LockSettingsHideUserList { get; set; }
        public GuestPolicy? GuestPolicy { get; set; }
        public bool? AllowModsToEjectCameras { get; set; }
        public bool? AllowRequestsWithoutSession { get; set; }
        public int? MeetingCameraCap { get; set; }
        public string? Logo { get; set; }
        public bool? SingleOrRepeated { get; set; }
        public RecordVisibility? RecordVisibility { get; set; }

        public List<int>? MeetingGroupIds { get; set; }
        public List<int>? MeetingModeratorListIds { get; set; }
        public List<string>? MeetingGuestEmailLists { get; set; }
        public List<DateOnly>? MeetingScheduleDateListDates { get; set; }
    }

    public class CreateMeetingCommandHandler : IRequestHandler<CreateMeetingCommand, ResultSingle<MeetingDto>>
    {
        private readonly IRepository<Meeting> _meetingRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<Package> _packageRepository;
        private readonly IMapper _mapper;

        public CreateMeetingCommandHandler(
            IRepository<Meeting> meetingRepository,
            UserManager<AppUser> userManager,
            IRepository<Package> packageRepository,
            IMapper mapper)
        {
            _meetingRepository = meetingRepository;
            _userManager = userManager;
            _packageRepository = packageRepository;
            _mapper = mapper;
        }

        public async Task<ResultSingle<MeetingDto>> Handle(CreateMeetingCommand request, CancellationToken cancellationToken)
        {
            // 0) Kullanıcının time zone'unu al
            var user = await _userManager.Users
                .Include(u => u.UserDetailSetting)
                .FirstOrDefaultAsync(u => u.Id == request.AppUserId);

            var timeZoneId = user.TimeZoneId ?? "Turkey Standard Time";
            var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            var package = _packageRepository.GetById(user.PackageId ?? 1);

            // 1) Kullanıcının varsayılan ayarları
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
                GuestPolicy = GuestPolicy.ASK_MODERATOR,
                AllowModsToEjectCameras = true,
                AllowRequestsWithoutSession = true,
                MeetingCameraCap = 10,
                Logo = "default_logo.png",
                RecordVisibility = RecordVisibility.MODERATORS
            };

            // 2) Meeting oluştur
            var meeting = new Meeting
            {
                CreatedBy = request.AppUserId,
                Name = request.Name,
                UserId = request.AppUserId,
                AllowModsToEjectCameras = request.AllowModsToEjectCameras ?? userDetailSetting.AllowModsToEjectCameras,
                AllowRequestsWithoutSession = request.AllowRequestsWithoutSession ?? userDetailSetting.AllowRequestsWithoutSession,
                BannerText = request.BannerText ?? userDetailSetting.BannerText,
                Description = request.Description ?? userDetailSetting.Description,
                Duration = userDetailSetting.Duration ?? 120,
                GuestPolicy = request.GuestPolicy ?? userDetailSetting.GuestPolicy,
                LockSettingsDisableCam = request.LockSettingsDisableCam ?? userDetailSetting.LockSettingsDisableCam,
                LockSettingsDisableMic = request.LockSettingsDisableMic ?? userDetailSetting.LockSettingsDisableMic,
                LockSettingsDisablePrivateChat = request.LockSettingsDisablePrivateChat ?? userDetailSetting.LockSettingsDisablePrivateChat,
                LockSettingsDisablePublicChat = request.LockSettingsDisablePublicChat ?? userDetailSetting.LockSettingsDisablePublicChat,
                LockSettingsHideUserList = request.LockSettingsHideUserList ?? userDetailSetting.LockSettingsHideUserList,
                Logo = request.Logo ?? userDetailSetting.Logo,
                MeetingCameraCap = request.MeetingCameraCap ?? userDetailSetting.MeetingCameraCap,
                RecordTF = request.RecordTF ?? userDetailSetting.RecordTF,
                StartTime = !string.IsNullOrEmpty(request.StartTime)
                                ? DateTime.Parse(request.StartTime).TimeOfDay
                                : (TimeSpan?)null,
                EndTime = !string.IsNullOrEmpty(request.EndTime)
                                ? DateTime.Parse(request.EndTime).TimeOfDay
                                : (TimeSpan?)null,
                StartDate = !string.IsNullOrEmpty(request.StartDate)
                                ? DateTime.Parse(request.StartDate)
                                : DateTime.MinValue,
            EndDate = !string.IsNullOrEmpty(request.EndDate)
                                ? DateTime.Parse(request.EndDate)
                                : (DateTime?)null,
                WebcamsOnlyForModerator = request.WebcamsOnlyForModerator ?? userDetailSetting.WebcamsOnlyForModerator,
                WelcomeMessage = request.WelcomeMessage ?? userDetailSetting.WelcomeMessage,
                SingleOrRepeated = request.SingleOrRepeated,
                RecordVisibility = request.RecordVisibility,
                ScheduleOrNowMeeting = ScheduleOrNowMeeting.SCHEDULE_MEETING,
                MeetingMeetingGroups = new List<MeetingMeetingGroup>(),
                MeetingModeratorLists = new List<MeetingModeratorList>(),
                MeetingGuestEmailLists = new List<MeetingGuestEmailList>(),
                MeetingScheduleDateLists = new List<MeetingScheduleDateList>()
            };

            // 3) StartDateTimeUtc ve EndDateTimeUtc hesapla (tek seferde)
            DateTime? mainStartUtc = null;
            if (!string.IsNullOrEmpty(request.StartDate) && !string.IsNullOrEmpty(request.StartTime))
            {
                var localStartString = request.StartDate + " " + request.StartTime;
                var localDateTime = DateTime.Parse(localStartString);
                mainStartUtc = TimeZoneInfo.ConvertTimeToUtc(localDateTime, userTimeZone);
            }

            DateTime? mainEndUtc = null;
            if (!string.IsNullOrEmpty(request.EndDate) && !string.IsNullOrEmpty(request.EndTime))
            {
                var localEndString = request.EndDate + " " + request.EndTime;
                var localEndDateTime = DateTime.Parse(localEndString);
                mainEndUtc = TimeZoneInfo.ConvertTimeToUtc(localEndDateTime, userTimeZone);
            }

            // 4) Meeting entity'de UTC alanlara set et
            meeting.StartDateTimeUtc = mainStartUtc;
            meeting.EndDateTimeUtc = mainEndUtc;

            // 5) Meeting Grupları, Moderatörler, Email Listeleri
            if (request.MeetingGroupIds != null)
            {
                foreach (var groupId in request.MeetingGroupIds)
                {
                    meeting.MeetingMeetingGroups.Add(new MeetingMeetingGroup
                    {
                        Meeting = meeting,
                        MeetingGroupId = groupId
                    });
                }
            }

            if (request.MeetingModeratorListIds != null)
            {
                foreach (var moderatorId in request.MeetingModeratorListIds)
                {
                    meeting.MeetingModeratorLists.Add(new MeetingModeratorList
                    {
                        Meeting = meeting,
                        AppUserId = moderatorId
                    });
                }
            }

            if (request.MeetingGuestEmailLists != null)
            {
                foreach (var email in request.MeetingGuestEmailLists)
                {
                    if (!string.IsNullOrEmpty(email))
                    {
                        meeting.MeetingGuestEmailLists.Add(new MeetingGuestEmailList
                        {
                            Meeting = meeting,
                            Email = email
                        });
                    }
                }
            }

            // 6) MeetingScheduleDateList - tekrar eden günler ya da tekil gün
            if (request.MeetingScheduleDateListDates != null && request.MeetingScheduleDateListDates.Count > 0)
            {
                // Aynı saatler tekrar ettiğini varsayalım (localStartTimeSpan)
                TimeSpan? localStartTimeSpan = null;
                if (!string.IsNullOrEmpty(request.StartTime))
                {
                    localStartTimeSpan = DateTime.Parse(request.StartTime).TimeOfDay;
                }

                foreach (var dateOnly in request.MeetingScheduleDateListDates)
                {
                    // 6a) Lokal tarih-saat oluştur
                    var localDateTime = new DateTime(
                        dateOnly.Year,
                        dateOnly.Month,
                        dateOnly.Day,
                        localStartTimeSpan?.Hours ?? 0,
                        localStartTimeSpan?.Minutes ?? 0,
                        0,
                        DateTimeKind.Unspecified
                    );

                    // 6b) UTC'ye çevir
                    var scheduleDateTimeUtc = TimeZoneInfo.ConvertTimeToUtc(localDateTime, userTimeZone);

                    // 6c) Entity ekle
                    var scheduleEntity = new MeetingScheduleDateList
                    {
                        Meeting = meeting,
                        StartDateTimeUtc = scheduleDateTimeUtc,
                        Date = dateOnly,
                        DidHappen = false
                    };
                    meeting.MeetingScheduleDateLists.Add(scheduleEntity);
                }
            }
            else
            {
                // 7) Eğer hiçbir liste gelmezse (tek seferlik kullanım),
                //    StartDate var ise en azından 1 tane MeetingScheduleDateList ekleyelim.
                if (!string.IsNullOrEmpty(request.StartDate))
                {
                    var startDateOnly = DateOnly.FromDateTime(DateTime.Parse(request.StartDate));

                    // StartTime varsa UTC'ye dönüştür
                    if (!string.IsNullOrEmpty(request.StartTime))
                    {
                        var localDateTime = new DateTime(
                            startDateOnly.Year,
                            startDateOnly.Month,
                            startDateOnly.Day,
                            DateTime.Parse(request.StartTime).Hour,
                            DateTime.Parse(request.StartTime).Minute,
                            0,
                            DateTimeKind.Unspecified
                        );

                        var scheduleDateTimeUtc = TimeZoneInfo.ConvertTimeToUtc(localDateTime, userTimeZone);

                        meeting.MeetingScheduleDateLists.Add(new MeetingScheduleDateList
                        {
                            Meeting = meeting,
                            StartDateTimeUtc = scheduleDateTimeUtc,
                            Date = startDateOnly,
                            DidHappen = false
                        });
                    }
                    else
                    {
                        // Hiç saat yoksa fallback: mainStartUtc veya DateTime.UtcNow
                        meeting.MeetingScheduleDateLists.Add(new MeetingScheduleDateList
                        {
                            Meeting = meeting,
                            StartDateTimeUtc = mainStartUtc ?? DateTime.UtcNow,
                            Date = startDateOnly,
                            DidHappen = false
                        });
                    }
                }
            }

            // 8) Kaydet
            _meetingRepository.InsertWithoutCommit(meeting);
            var result = await _meetingRepository.CommitAsync(cancellationToken);
            if (result == -1)
            {
                return Result.Fail<MeetingDto>(null, "Kayıt edilemedi");
            }

            // 9) Sonuç
            var data = _mapper.Map<MeetingDto>(meeting);
            return Result.Ok(data, "Oturum oluşturuldu");
        }
    }
}
