namespace Domain.Entities
{
    public class Ticket:BaseEntity
    {
        public string Title { get; set; }
        public DateTime? ClosedDate { get; set; }
        public DateTime? UpdatedTimeForMessage { get; set; } = DateTime.UtcNow;
        public int StatusId { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }

        public int AssignedUserId { get; set; }
        public bool WasReadTicket { get; set; }
        public bool WasReadTicketAdmin { get; set; }
        public AppUser AssignedUser { get; set; }

        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        // Navigation Properties
        public ICollection<TicketMessage> Messages { get; set; }
        public ICollection<TicketStatusHistory> TicketStatusHistories { get; set; }
    }
}
