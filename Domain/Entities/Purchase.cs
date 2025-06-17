using Shared.Payment;
using System.Numerics;

namespace Domain.Entities
{
    public class Purchase:BaseEntity
    {
        public int AppUserId { get; set; }

        public virtual AppUser AppUser { get; set; }

        public string OrderId { get; set; }
        public string Explain { get; set; }
        public string? ReceiptId { get; set; }
        

        public int PackageId { get; set; }

        public virtual Package Package { get; set; }

        public PurchaseStatusType StatusType { get; set; }
        public PurchaseType PurchaseType { get; set; }


        public double Price { get; set; }

        public string PurchaseToken { get; set; }

        public DateTime? EndDate { get; set; }

        public int? CouponId { get; set; }

        public virtual Cupon Coupon { get; set; }
        public bool? InvoiceBeenCreate { get; set; } = false;
    }
}
