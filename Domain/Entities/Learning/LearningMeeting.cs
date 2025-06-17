namespace Domain.Entities.Learning
{
    public class LearningMeeting:BaseEntity
    {
        public int Id { get; set; }
        public string? Version { get; set; }
        public string? MeetingId { get; set; }
        public string? InternalMeetingId { get; set; }
        public bool? IsBreakout { get; set; }
        public string? MeetingName { get; set; }
        public int? Duration { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? Finish { get; set; }
        public ICollection<Attendee>? Attendees { get; set; }
        public ICollection<LearningFile>? Files { get; set; }
        public ICollection<Poll>? Polls { get; set; }
    }
}
