using Shared.Enum;

namespace Domain.Entities
{
    public class Meeting : BaseEntity
    {
        public int UserId { get; set; }
        public virtual AppUser User { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? WelcomeMessage { get; set; }
        public bool? RecordTF { get; set; }
        public DateTime StartDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public DateTime? EndDate { get; set; }

        public DateTime? StartDateTimeUtc { get; set; }
        public DateTime? EndDateTimeUtc { get; set; }

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
        public bool? SingleOrRepeated { get; set; } //Burası toplantıyı edit ederken tek tarih mi girileceği çok tarih mi girileceğini kontrol ediyor.
        public ScheduleOrNowMeeting? ScheduleOrNowMeeting { get; set; } //Burası da NOW meetingleri sadece gerçekleşenler listesinde göstermek için düzenlendi. Bu meeting kayıtlarını tekrar kullanamazlar. Sadece o verilerin hangi ayarlarda oluşturulduğunun kaydıdır ve tüm meetingleri göreceğimiz asıl alandır.
        public RecordVisibility? RecordVisibility { get; set; }




        public virtual ICollection<MeetingMeetingGroup>? MeetingMeetingGroups { get; set; }
        public virtual ICollection<MeetingModeratorList>? MeetingModeratorLists { get; set; }
        public virtual ICollection<MeetingGuestEmailList>? MeetingGuestEmailLists { get; set; }
        public virtual ICollection<MeetingScheduleDateList>? MeetingScheduleDateLists { get; set; }
    }
}