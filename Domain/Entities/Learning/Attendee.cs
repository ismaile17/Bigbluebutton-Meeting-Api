namespace Domain.Entities.Learning
{
    public class Attendee
    {
        public int Id { get; set; }
        public int? MeetingId { get; set; }
        public string? ExtUserId { get; set; }
        public string? Name { get; set; }
        public bool? Moderator { get; set; }
        public int? Duration { get; set; }
        public string? RecentTalkingTime { get; set; }
        public int? EngagementChats { get; set; }
        public int? EngagementTalks { get; set; }
        public int? EngagementRaisehand { get; set; }
        public int? EngagementEmojis { get; set; }
        public int? EngagementPollVotes { get; set; }
        public int? EngagementTalkTime { get; set; }
        public ICollection<AttendeeSession>? Sessions { get; set; }
    }
}
