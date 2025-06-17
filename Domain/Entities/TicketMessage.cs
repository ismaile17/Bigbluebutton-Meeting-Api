namespace Domain.Entities
{
    public class TicketMessage
    {
        public int Id { get; set; }
        public string MessageText { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Foreign Key
        public int TicketId { get; set; }
        public bool WasRead { get; set; }
        public Ticket Ticket { get; set; }

        public int CreatedByUserId { get; set; }
        public AppUser CreatedByUser { get; set; }
    }
}
