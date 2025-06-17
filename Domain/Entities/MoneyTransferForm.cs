namespace Domain.Entities
{
    public class MoneyTransferForm:BaseEntity
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PhoneNumber { get; set; }
        public string? AdminNote { get; set; }
        public int PackageId { get; set; }
        public bool Success { get; set; } =false;
    }
}
