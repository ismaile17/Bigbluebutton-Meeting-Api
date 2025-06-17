namespace Domain.Entities
{
    public class Cupon:BaseEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string DiscountType { get; set; } // "Percentage", "Fixed", "MinimumOrderPercentage", "MinimumOrderFixed"

        // DiscountValue, indirim miktarını belirtir. Bu değer, indirim türüne bağlı olarak yüzdelik veya sabit tutar olabilir.
        // Örneğin:
        // - "Percentage" için: Bu değer, toplam tutardan uygulanacak yüzdeyi ifade eder (örn. %10 için 10.00).
        // - "Fixed" için: Bu değer, toplam tutardan düşülecek sabit tutarı ifade eder (örn. 50 TL için 50.00).
        // - "MinimumOrderPercentage" için: MinimumOrderValue'den büyük bir toplam tutar varsa, bu değer, yüzde olarak uygulanacak indirimi ifade eder.
        // - "MinimumOrderFixed" için: MinimumOrderValue'den büyük bir toplam tutar varsa, bu değer, sabit tutar olarak uygulanacak indirimi ifade eder.
        public decimal DiscountValue { get; set; }

        // MinimumOrderValue, "MinimumOrderPercentage" ve "MinimumOrderFixed" indirim türleri için geçerlidir.
        // Bu değer, indirimin uygulanabilmesi için gereken minimum sipariş tutarını belirtir.
        // Örneğin:
        // - MinimumOrderValue 300 TL ise, sipariş tutarı 300 TL veya daha fazla olmalıdır.
        public decimal? MinimumOrderValue { get; set; }

        // UsageLimit, kuponun kaç kez kullanılabileceğini belirtir.
        // Örneğin:
        // - UsageLimit 5 ise, bu kupon toplamda 5 kez kullanılabilir.
        public int UsageLimit { get; set; }

        // ExpiryDate, kuponun son kullanma tarihini belirtir.
        // Bu tarihten sonra kupon kullanılamaz.
        public DateTime ExpiryDate { get; set; }

        // UsedCount, kuponun kaç kez kullanıldığını takip eder.
        // Bu değer, kupon kullanıldıkça artar ve UsageLimit'e ulaştığında kupon devre dışı kalır.
        public int UsedCount { get; set; }

        // Purchases, bu kuponun kullanıldığı satın alımların listesini tutar.
        public virtual ICollection<Purchase> Purchases { get; set; }

        // Kupon türleri ve işleyişleri:
        // - "Percentage": Toplam tutardan girilen DiscountValue yüzdesi kadar indirim uygulanır.
        //   Örneğin, DiscountValue %10 ve toplam tutar 100 TL ise, indirimli tutar 90 TL olur.
        // - "Fixed": Toplam tutardan girilen DiscountValue sabit tutarı kadar indirim uygulanır.
        //   Örneğin, DiscountValue 50 TL ve toplam tutar 200 TL ise, indirimli tutar 150 TL olur.
        // - "MinimumOrderPercentage": MinimumOrderValue'den büyük bir toplam tutar varsa, girilen DiscountValue yüzdesi kadar indirim uygulanır.
        //   Örneğin, MinimumOrderValue 300 TL, DiscountValue %15 ve toplam tutar 350 TL ise, indirimli tutar 297.5 TL olur.
        // - "MinimumOrderFixed": MinimumOrderValue'den büyük bir toplam tutar varsa, girilen DiscountValue sabit tutarı kadar indirim uygulanır.
        //   Örneğin, MinimumOrderValue 300 TL, DiscountValue 50 TL ve toplam tutar 350 TL ise, indirimli tutar 300 TL olur.
    }
}