namespace Domain.Entities
{
    public class TicketStatusHistory
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public DateTime ChangedDate { get; set; } = DateTime.Now;

        // Foreign Key
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        public int ChangedByUserId { get; set; }
        public AppUser ChangedByUser { get; set; }
    }
}
