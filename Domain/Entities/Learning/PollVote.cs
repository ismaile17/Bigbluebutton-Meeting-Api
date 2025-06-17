namespace Domain.Entities.Learning
{
    public class PollVote
    {
        public int Id { get; set; }
        public int? PollId { get; set; }
        public string? UserId { get; set; }
        public string? Vote { get; set; }
    }
}
