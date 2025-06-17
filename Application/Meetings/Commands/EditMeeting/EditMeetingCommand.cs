using Application.Meetings.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Shared.Enum;

namespace Application.Meetings.Commands.EditMeeting
{
    public class EditMeetingCommand : IRequest<ResultSingle<MeetingDto>>
    {
        public int Id { get; set; }
        public int AppUserId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? WelcomeMessage { get; set; }
        public bool? RecordTF { get; set; }
        public string? StartDate { get; set; }
        public string? StartTime { get; set; }
        public string? EndDate { get; set; }
        public string? EndTime { get; set; }
        public int? Duration { get; set; }
        public bool? WebcamsOnlyForModerator { get; set; }
        public string? BannerText { get; set; }
        public bool? LockSettingsDisableCam { get; set; }
        public bool? LockSettingsDisableMic { get; set; }
        public bool? LockSettingsDisablePrivateChat { get; set; }
        public bool? LockSettingsDisablePublicChat { get; set; }
        public bool? LockSettingsHideUserList { get; set; }
        public GuestPolicy? GuestPolicy { get; set; }
        public RecordVisibility? RecordVisibility { get; set; }
        public bool? AllowModsToEjectCameras { get; set; }
        public bool? AllowRequestsWithoutSession { get; set; }
        public int? MeetingCameraCap { get; set; }
        public string? Logo { get; set; }
        public bool? SingleOrRepeated { get; set; }
        public bool? PublicOrPrivate { get; set; }
        public List<int>? MeetingGroupIds { get; set; }
        public List<int>? MeetingModeratorListIds { get; set; }
        public List<string>? MeetingGuestEmailLists { get; set; }
        public List<DateOnly>? MeetingScheduleDateListDates { get; set; }
    }

    public class EditMeetingCommandHandler : IRequestHandler<EditMeetingCommand, ResultSingle<MeetingDto>>
    {
        private readonly IRepository<Meeting> _meetingRepository;
        private readonly IMapper _mapper;

        // Örnek olsun diye sabit "Turkey Standard Time" yazıyorum;
        // dilerseniz user'ın TimeZone bilgisini db'den alıp burada kullanabilirsiniz.
        private readonly string _timeZoneId = "Turkey Standard Time";

        public EditMeetingCommandHandler(IRepository<Meeting> meetingRepository, IMapper mapper)
        {
            _meetingRepository = meetingRepository;
            _mapper = mapper;
        }

        public async Task<ResultSingle<MeetingDto>> Handle(EditMeetingCommand request, CancellationToken cancellationToken)
        {
            var meetingdata = _meetingRepository.Get(a => a.Id == request.Id,
                                                    c => c.MeetingMeetingGroups,
                                                    c => c.MeetingModeratorLists,
                                                    c => c.MeetingGuestEmailLists,
                                                    c => c.MeetingScheduleDateLists);

            if (meetingdata == null)
            {
                return Result.Fail<MeetingDto>(null, $"Meeting not found");
            }

            // -- 1) Time Zone hazırlığı
            var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(_timeZoneId);

            // -- 2) Gerekirse Meeting entity'sinin kendi StartDateTimeUtc / EndDateTimeUtc'sini de güncelleyelim:
            DateTime? mainStartUtc = null;
            if (!string.IsNullOrEmpty(request.StartDate) && !string.IsNullOrEmpty(request.StartTime))
            {
                var localStartString = request.StartDate + " " + request.StartTime;
                var localDateTime = DateTime.Parse(localStartString);
                mainStartUtc = TimeZoneInfo.ConvertTimeToUtc(localDateTime, userTimeZone);
            }

            // Burada EndDateTimeUtc isteniyorsa benzer mantık:
            DateTime? mainEndUtc = null;
            if (!string.IsNullOrEmpty(request.EndDate) && !string.IsNullOrEmpty(request.EndTime))
            {
                var localEndString = request.EndDate + " " + request.EndTime;
                var localEndDateTime = DateTime.Parse(localEndString);
                mainEndUtc = TimeZoneInfo.ConvertTimeToUtc(localEndDateTime, userTimeZone);
            }

            meetingdata.StartDateTimeUtc = mainStartUtc;   // Create'de yaptığınız gibi
            meetingdata.EndDateTimeUtc = mainEndUtc;

            // -- 3) Diğer Meeting alanları:
            meetingdata.UpdatedBy = request.AppUserId;
            meetingdata.Name = request.Name;
            meetingdata.AllowModsToEjectCameras = request.AllowModsToEjectCameras ?? meetingdata.AllowModsToEjectCameras;
            meetingdata.AllowRequestsWithoutSession = request.AllowRequestsWithoutSession ?? meetingdata.AllowRequestsWithoutSession;
            meetingdata.BannerText = request.BannerText ?? meetingdata.BannerText;
            meetingdata.Description = request.Description ?? meetingdata.Description;
            meetingdata.Duration = request.Duration ?? meetingdata.Duration;
            meetingdata.GuestPolicy = request.GuestPolicy ?? meetingdata.GuestPolicy;
            meetingdata.LockSettingsDisableCam = request.LockSettingsDisableCam ?? meetingdata.LockSettingsDisableCam;
            meetingdata.LockSettingsDisableMic = request.LockSettingsDisableMic ?? meetingdata.LockSettingsDisableMic;
            meetingdata.LockSettingsDisablePrivateChat = request.LockSettingsDisablePrivateChat ?? meetingdata.LockSettingsDisablePrivateChat;
            meetingdata.LockSettingsDisablePublicChat = request.LockSettingsDisablePublicChat ?? meetingdata.LockSettingsDisablePublicChat;
            meetingdata.LockSettingsHideUserList = request.LockSettingsHideUserList ?? meetingdata.LockSettingsHideUserList;
            meetingdata.Logo = request.Logo ?? meetingdata.Logo;
            meetingdata.MeetingCameraCap = request.MeetingCameraCap ?? meetingdata.MeetingCameraCap;
            meetingdata.RecordTF = request.RecordTF ?? meetingdata.RecordTF;
            meetingdata.StartTime = request.StartTime != null
                                    ? (TimeSpan?)DateTime.Parse(request.StartTime).TimeOfDay
                                    : meetingdata.StartTime;
            meetingdata.EndTime = request.EndTime != null
                                  ? (TimeSpan?)DateTime.Parse(request.EndTime).TimeOfDay
                                  : meetingdata.EndTime;
            meetingdata.StartDate = request.StartDate != null
                                    ? Convert.ToDateTime(request.StartDate)
                                    : meetingdata.StartDate;
            meetingdata.EndDate = request.EndDate != null
                                  ? Convert.ToDateTime(request.EndDate)
                                  : meetingdata.EndDate;
            meetingdata.WebcamsOnlyForModerator = request.WebcamsOnlyForModerator ?? meetingdata.WebcamsOnlyForModerator;
            meetingdata.WelcomeMessage = request.WelcomeMessage ?? meetingdata.WelcomeMessage;
            meetingdata.SingleOrRepeated = request.SingleOrRepeated ?? meetingdata.SingleOrRepeated;
            meetingdata.RecordVisibility = request.RecordVisibility ?? meetingdata.RecordVisibility;

            // -- 4) Groups
            if (request.MeetingGroupIds == null || !request.MeetingGroupIds.Any())
            {
                meetingdata.MeetingMeetingGroups.Clear();
            }
            else
            {
                var deletedGroups = meetingdata.MeetingMeetingGroups
                    .Where(a => !request.MeetingGroupIds.Contains(a.MeetingGroupId))
                    .ToList();

                foreach (var group in deletedGroups)
                {
                    meetingdata.MeetingMeetingGroups.Remove(group);
                }

                var newGroups = request.MeetingGroupIds
                    .Where(id => !meetingdata.MeetingMeetingGroups.Any(g => g.MeetingGroupId == id))
                    .ToList();

                foreach (var id in newGroups)
                {
                    var newGroup = new MeetingMeetingGroup
                    {
                        Meeting = meetingdata,
                        MeetingGroupId = id
                    };
                    meetingdata.MeetingMeetingGroups.Add(newGroup);
                }
            }

            // -- 5) Moderators
            if (request.MeetingModeratorListIds == null || !request.MeetingModeratorListIds.Any())
            {
                meetingdata.MeetingModeratorLists.Clear();
            }
            else
            {
                var deletedModerators = meetingdata.MeetingModeratorLists
                    .Where(a => !request.MeetingModeratorListIds.Contains(a.AppUserId))
                    .ToList();

                foreach (var moderator in deletedModerators)
                {
                    meetingdata.MeetingModeratorLists.Remove(moderator);
                }

                var newModerators = request.MeetingModeratorListIds
                    .Where(id => !meetingdata.MeetingModeratorLists.Any(m => m.AppUserId == id))
                    .ToList();

                foreach (var id in newModerators)
                {
                    var newModerator = new MeetingModeratorList
                    {
                        Meeting = meetingdata,
                        AppUserId = id
                    };
                    meetingdata.MeetingModeratorLists.Add(newModerator);
                }
            }

            // -- 6) Guest Emails
            if (request.MeetingGuestEmailLists == null || !request.MeetingGuestEmailLists.Any())
            {
                meetingdata.MeetingGuestEmailLists.Clear();
            }
            else
            {
                var deletedEmails = meetingdata.MeetingGuestEmailLists
                    .Where(a => !request.MeetingGuestEmailLists.Contains(a.Email))
                    .ToList();

                foreach (var email in deletedEmails)
                {
                    meetingdata.MeetingGuestEmailLists.Remove(email);
                }

                var newEmails = request.MeetingGuestEmailLists
                    .Where(email => !meetingdata.MeetingGuestEmailLists.Any(e => e.Email == email))
                    .ToList();

                foreach (var email in newEmails)
                {
                    var newEmail = new MeetingGuestEmailList
                    {
                        Meeting = meetingdata,
                        Email = email
                    };
                    meetingdata.MeetingGuestEmailLists.Add(newEmail);
                }
            }

            // -- 7) MeetingScheduleDateLists
            // EĞER hiç tarih yoksa veya SingleOrRepeated false ise (tekrar yoksa),
            // var olan ve henüz gerçekleşmemiş (DidHappen == false) kayıtları siliyoruz:
            if (request.MeetingScheduleDateListDates == null
                || !request.MeetingScheduleDateListDates.Any()
                || request.SingleOrRepeated == false)
            {
                var itemsToRemove = meetingdata.MeetingScheduleDateLists
                    .Where(x => x.DidHappen == false)
                    .ToList();

                foreach (var item in itemsToRemove)
                {
                    meetingdata.MeetingScheduleDateLists.Remove(item);
                }
            }
            else
            {
                // Silinecek tarihler (istekte yok ama veritabanında var, DidHappen==false)
                var deletedDates = meetingdata.MeetingScheduleDateLists
                    .Where(a => !request.MeetingScheduleDateListDates.Contains(a.Date)
                             && a.DidHappen == false)
                    .ToList();

                foreach (var date in deletedDates)
                {
                    meetingdata.MeetingScheduleDateLists.Remove(date);
                }

                // Eklenmesi gereken yeni tarihler (istekte var ama DB'de yok)
                var newDates = request.MeetingScheduleDateListDates
                    .Where(d => !meetingdata.MeetingScheduleDateLists.Any(s => s.Date == d))
                    .ToList();

                // Bu kısımda, CREATE mantığındaki gibi StartDateTimeUtc hesaplayarak ekleyeceğiz:
                foreach (var date in newDates)
                {
                    DateTime scheduleDateTimeUtc;

                    if (meetingdata.StartTime.HasValue)
                    {
                        // StartTime varsa local DateTime hesaplayıp UTC'ye çeviriyoruz
                        var localDateTime = new DateTime(
                            date.Year,
                            date.Month,
                            date.Day,
                            meetingdata.StartTime.Value.Hours,
                            meetingdata.StartTime.Value.Minutes,
                            0,
                            DateTimeKind.Unspecified
                        );
                        scheduleDateTimeUtc = TimeZoneInfo.ConvertTimeToUtc(localDateTime, userTimeZone);
                    }
                    else
                    {
                        // StartTime yoksa fallback
                        // meetingdata.StartDateTimeUtc veya DateTime.UtcNow gibi bir mantık
                        scheduleDateTimeUtc = meetingdata.StartDateTimeUtc ?? DateTime.UtcNow;
                    }

                    var newDateItem = new MeetingScheduleDateList
                    {
                        Meeting = meetingdata,
                        Date = date,
                        StartDateTimeUtc = scheduleDateTimeUtc,
                        DidHappen = false
                    };
                    meetingdata.MeetingScheduleDateLists.Add(newDateItem);
                }
            }

            // Eğer tüm tarihleri temizlediysek ama yine de "StartDate" var ise 
            // (kodunuzdaki else senaryosu):
            if (request.MeetingScheduleDateListDates == null
                || !request.MeetingScheduleDateListDates.Any()
                || request.SingleOrRepeated == false)
            {
                if (!string.IsNullOrEmpty(request.StartDate))
                {
                    var startDateOnly = DateOnly.FromDateTime(DateTime.Parse(request.StartDate));

                    // StartTime var ise UTC'ye dönüştürerek ekliyoruz (Create mantığındaki gibi)
                    DateTime scheduleDateTimeUtc;
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
                        scheduleDateTimeUtc = TimeZoneInfo.ConvertTimeToUtc(localDateTime, userTimeZone);
                    }
                    else
                    {
                        scheduleDateTimeUtc = meetingdata.StartDateTimeUtc ?? DateTime.UtcNow;
                    }

                    var singleDateItem = new MeetingScheduleDateList
                    {
                        Meeting = meetingdata,
                        Date = startDateOnly,
                        StartDateTimeUtc = scheduleDateTimeUtc,
                        DidHappen = false
                    };
                    meetingdata.MeetingScheduleDateLists.Add(singleDateItem);
                }
            }

            // -- 8) Kaydet
            _meetingRepository.UpdateWithoutCommit(meetingdata);

            try
            {
                var result = await _meetingRepository.CommitAsync(cancellationToken);

                if (result == -1)
                {
                    return Result.Fail<MeetingDto>(null, $"Save failed: Unknown error occurred during commit.");
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Commit failed with error: {ex.Message}";
                return Result.Fail<MeetingDto>(null, errorMessage);
            }

            // -- 9) Map & return
            var data = await Task.Run(() => _mapper.Map<Meeting, MeetingDto>(meetingdata));
            return Result.Ok(data, $"Meeting updated successfully");
        }
    }
}
