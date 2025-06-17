namespace Domain.Entities
{
    public class LearningDashboard:BaseEntity
    {
        public int Id { get; set; }
        public string MeetingId { get; set; }
        public string InternalMeetingId { get; set; }
        public string Version { get; set; }
        public string Data { get; set; }
    }
}
