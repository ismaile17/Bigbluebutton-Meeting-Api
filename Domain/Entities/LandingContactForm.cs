namespace Domain.Entities
{
    public class LandingContactForm:BaseEntity
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string? AboutYou { get; set; }
        public string Message { get; set; }
        public string? IPAddress { get; set; }
    }
}
