namespace Application.Cupon.Models
{
    public class CuponCheckAnswerDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public bool TrueFalse { get; set; }
        public decimal DiscountedPrice { get; set; }
        public string Message { get; set; }
    }
}
