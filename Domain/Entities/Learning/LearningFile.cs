namespace Domain.Entities.Learning
{
    public class LearningFile
    {
        public int Id { get; set; }
        public int? MeetingId { get; set; }
        public string? FileName { get; set; }
    }
}
