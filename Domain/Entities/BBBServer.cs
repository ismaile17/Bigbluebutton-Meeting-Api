namespace Domain.Entities
{
    public class BBBServer : BaseEntity
    {
        public bool? MainServer { get; set; }=false;
        public string? ServerName { get; set; }
        public string? ServerDetail { get; set; }
        public string? Notes { get; set; }
        public string? BuyPrice { get; set; }
        public string? BuyCompany { get; set; }
        public string? ServerLocation { get; set; }
        public string? IpAdress { get; set; }
        public string? ServerApiUrl { get; set; }
        public string? SharedSecret { get; set; }

    }
}
