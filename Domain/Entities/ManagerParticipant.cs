namespace Domain.Entities
{
    public class ManagerParticipant:BaseEntity
    {
        public int ManagerId { get; set; }
        public AppUser? Manager { get; set; }
        public int ParticipantId { get; set; }
        public AppUser? Participant { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? SpecialDescription { get; set; }
        public bool? IsActive { get; set; } = true;
        public bool? ExpiryDateIsActive { get; set; } = false;
        public int AppUserId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? NameSurname { get; set; }


    }
}