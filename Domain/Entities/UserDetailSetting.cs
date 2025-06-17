using Shared.Enum;

namespace Domain.Entities
{
    public class UserDetailSetting
    {
        public int UserId { get; set; }
        public virtual AppUser User { get; set; }
        public string? Description { get; set; }
        public string? WelcomeMessage { get; set; }
        public bool? RecordTF { get; set; }
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
        public string? InvoiceType { get; set; }
        public string? InvoiceNameSurname { get; set; }
        public string? InvoicePersonalNumber { get; set; }
        public string? InvoiceAddress { get; set; }
        public string? BusinessName { get; set; }
        public string? BusinessNumber { get; set; }
        public string? BusinessCountry { get; set; }
        public string? BusinessCity { get; set; }
        public string? BusinessProvince { get; set; }
        public string? BusinessAddress { get; set; }
        public string? BusinessVD { get; set; }
        public RecordVisibility? RecordVisibility { get; set; }

    }
}
