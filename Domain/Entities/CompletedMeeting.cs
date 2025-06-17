using Shared.Enum;

namespace Domain.Entities
{
    public class CompletedMeeting:BaseEntity
    {
        public int MeetingId { get; set; }
        public Guid CompletedGuid { get; set; } = Guid.NewGuid();
        public string BBBMeetingId { get; set; }
        public int MeetingType { get; set; }
        public int UserId { get; set; }
        public string? ModeratorPassword { get; set; }
        public string? AttendeePassword { get; set; }
        public int ServerId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? WelcomeMessage { get; set; }
        public bool? RecordTF { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? EndDate { get; set; }
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
        public string? GuestLink { get; set; }
        public string? MeetingExplain { get; set; }
        public string? InternalMeetingId { get; set; }
        public string? StartIpAddress { get; set; }
        public int? StartPort { get; set; }
        public CompletedMeetingCreateStartOrFinish? CreateStartOrFinish { get; set; }
        public RecordVisibility? RecordVisibility { get; set; }
        public ScheduleOrNowMeeting? ScheduleOrNowMeeting { get; set; }

    }
}
