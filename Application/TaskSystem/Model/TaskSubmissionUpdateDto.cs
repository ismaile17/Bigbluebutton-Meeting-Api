namespace Application.TaskSystem.Model
{
    public class TaskSubmissionUpdateDto
    {
        public int UserId { get; set; }
        public decimal? Grade { get; set; }
        public string? Feedback { get; set; }
    }
}