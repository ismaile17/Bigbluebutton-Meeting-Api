using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class CuponConfiguration : IEntityTypeConfiguration<Cupon>
    {
        public void Configure(EntityTypeBuilder<Cupon> builder)
        {

            // Kupon kodu (Code) için zorunlu alan ve maksimum uzunluk
            builder.Property(c => c.Code)
                   .IsRequired()
                   .HasMaxLength(50);

            // İndirim türü (DiscountType) için zorunlu alan ve maksimum uzunluk
            builder.Property(c => c.DiscountType)
                   .IsRequired()
                   .HasMaxLength(50);

            // İndirim değeri (DiscountValue) için zorunlu alan
            builder.Property(c => c.DiscountValue)
                   .IsRequired()
                   .HasColumnType("decimal(10, 2)");

            // Minimum sipariş tutarı (MinimumOrderValue) için opsiyonel alan
            builder.Property(c => c.MinimumOrderValue)
                   .HasColumnType("decimal(10, 2)");

            // Kullanım limiti (UsageLimit) için zorunlu alan
            builder.Property(c => c.UsageLimit)
                   .IsRequired();

            // Son kullanma tarihi (ExpiryDate) için zorunlu alan
            builder.Property(c => c.ExpiryDate)
                   .IsRequired();

            // Kullanım sayısı (UsedCount) için zorunlu alan ve varsayılan değer
            builder.Property(c => c.UsedCount)
                   .IsRequired()
                   .HasDefaultValue(0);
        }
    }
}
