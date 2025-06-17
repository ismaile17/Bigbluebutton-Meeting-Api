namespace Domain.Entities
{
    public class Package : BaseEntity
    {
        public int? ParentID { get; set; }
        public int? Duration { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }
        public int Price { get; set; }
        public string PriceCurrency { get; set; }
        public string SmsCountGift { get; set; }
        public int ValidityTotalDay { get; set; }
        public bool CloudRecording { get; set; }
        public int CloudRecordingGbSize { get; set; }
        public int SessionHours { get; set; }
        public bool SutdyRooms { get; set; }
        public string Logo { get; set; }
        public int? PackageParentType { get; set; }
        public int? MaxParticipants { get; set; }
        public int? MaxGroup { get; set; }
        public int? MaxGroupUser { get; set; }
        public int? MaxMeetingInvitedParticipant { get; set; }
        public int? MaxParticipantUser { get; set; }
        public int? MaxDurationMinutes { get; set; }

        //public virtual ICollection<PackageAppUser> PackageAppUsers { get; set; }

    }
}