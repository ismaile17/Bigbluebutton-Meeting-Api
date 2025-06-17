using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Cupon.Service
{
    public class CuponCheckService
    {
        private readonly IRepository<Domain.Entities.Cupon> _cuponRepository;
        private readonly IRepository<Package> _packageRepository;

        public CuponCheckService(IRepository<Domain.Entities.Cupon> cuponRepository, IRepository<Package> packageRepository)
        {
            _cuponRepository = cuponRepository;
            _packageRepository = packageRepository;
        }

        public async Task<(bool isValid, decimal discountedPrice, string message)> ApplyCuponAsync(string code, int packageId, CancellationToken cancellationToken)
        {
            var package = await _packageRepository.GetMany(p => p.Id == packageId && p.IsActive == 1).FirstOrDefaultAsync(cancellationToken);

            decimal totalAmount = package.Price;

            if (package == null)
            {
                return (false, totalAmount, "Geçersiz kupon kodu.");
            }

            if (code == null)
            {
                return (false, totalAmount, "Code yazınız");
            }



            var normalizedCode = code.ToLower();
            var cupon = _cuponRepository.Get(c => c.Code.ToLower() == normalizedCode);

            if (cupon == null)
            {
                return (false, totalAmount, "Geçersiz kupon kodu.");
            }

            if (cupon.ExpiryDate < DateTime.UtcNow)
            {
                return (false, totalAmount, "Kuponun son kullanma tarihi geçmiş.");
            }

            if (cupon.UsedCount >= cupon.UsageLimit)
            {
                return (false, totalAmount, "Kupon kullanım limiti dolmuş.");
            }

            if (cupon.MinimumOrderValue.HasValue && totalAmount < cupon.MinimumOrderValue.Value)
            {
                return (false, totalAmount, $"Kuponu kullanabilmek için minimum sipariş tutarı {cupon.MinimumOrderValue} TL olmalıdır.");
            }

            decimal discountedPrice = totalAmount;

            switch (cupon.DiscountType)
            {
                case "Percentage":
                    discountedPrice -= totalAmount * (cupon.DiscountValue / 100);
                    break;
                case "Fixed":
                    discountedPrice -= cupon.DiscountValue;
                    break;
                case "MinimumOrderPercentage":
                    if (totalAmount >= cupon.MinimumOrderValue)
                    {
                        discountedPrice -= totalAmount * (cupon.DiscountValue / 100);
                    }
                    break;
                case "MinimumOrderFixed":
                    if (totalAmount >= cupon.MinimumOrderValue)
                    {
                        discountedPrice -= cupon.DiscountValue;
                    }
                    break;
                default:
                    return (false, totalAmount, "Bilinmeyen indirim türü.");
            }

            discountedPrice = Math.Max(discountedPrice, 0);

            return (true, discountedPrice, "Kupon başarıyla uygulandı.");
        }
    }
}