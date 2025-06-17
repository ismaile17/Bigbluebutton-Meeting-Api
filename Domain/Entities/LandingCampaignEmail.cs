namespace Domain.Entities
{
    public class LandingCampaignEmail:BaseEntity
    {
        public string Email { get; set; }
        public string? IPAddress { get; set; }
    }
}
