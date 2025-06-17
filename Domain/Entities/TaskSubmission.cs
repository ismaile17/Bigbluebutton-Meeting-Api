namespace Domain.Entities
{
    public class TaskSubmission:BaseEntity
    {
        public decimal? Grade { get; set; } // Öğretmenin verdiği not
        public string? Feedback { get; set; } // Öğretmenin geri bildirimi
        public string? ParticipantNote { get; set; } // Öğretmenin geri bildirimi
        public int TaskAssignmentId { get; set; } // Teslimin ait olduğu ödevin ID'si
        public virtual TaskAssignment TaskAssignment { get; set; } // Teslimin ait olduğu ödev
    }
}
