using Application.LearningDashboards.Model;
using Application.Packages.Model;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;

namespace Application.Packages.Services
{
    public class GetPackageByIdCheckService
    {
        private readonly IRepository<Package> _packageRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public GetPackageByIdCheckService(IRepository<Package> packageRepository, UserManager<AppUser> userManager, IMapper mapper)
        {
            _packageRepository = packageRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<(bool isValid, PackageDto package, string message, int status, DateTime? newEndDate)> GetPackageByIdAsync(int packageId, int appUserId, CancellationToken cancellationToken)
        {
            // Kullanıcıyı ve ilişkili paketi yükleyelim
            var user = await _userManager.Users
                .Include(u => u.Package) // Kullanıcının paket bilgilerini de dahil et
                .FirstOrDefaultAsync(u => u.Id == appUserId);

            if (user == null)
            {
                return (false, null, $"ID'si {appUserId} olan kullanıcı bulunamadı.", 0, null);
            }

            // Satın alınan paketi kontrol edelim
            var package = _packageRepository.GetById(packageId);
            if (package == null)
            {
                return (false, null, $"ID'si {packageId} olan paket bulunamadı.", 0, null);
            }

            // Kullanıcının aktif paketi yoksa ya da süresi dolmuşsa
            if (user.Package == null || user.PackageEndDate < DateTime.UtcNow)
            {
                var basicPackageDto = await Task.Run(() => _mapper.Map<Package, PackageDto>(package));
                return (true, basicPackageDto, "Başarılı", 0, null);
            }

            // Kullanıcının mevcut paket bilgileri
            var userPackageParentType = user.Package.PackageParentType;
            var requestedPackageParentType = package.PackageParentType;

            // Paket tipi eşleşmeleri kontrol edelim
            if (userPackageParentType == null || requestedPackageParentType == null)
            {
                return (false, null, "Paket bilgileri alınamadı.", 0, null);
            }

            // Mevcut paket daha büyükse hata döndür
            if (userPackageParentType > requestedPackageParentType)
            {
                return (false, null, "Daha düşük bir paketi sorgulayamazsınız.", 0, null);
            }

            // Aynı paket tipine sahipse paketi uzatalım
            if (userPackageParentType == requestedPackageParentType)
            {
                var newEndDate = user.PackageEndDate?.AddDays(package.ValidityTotalDay) ?? DateTime.UtcNow.AddDays(package.ValidityTotalDay);
                var packageDto = _mapper.Map<PackageDto>(package);
                return (true, packageDto, "Sahip olduğunuz paketin süresi uzatılıyor.", 1, newEndDate);
            }

            // Kullanıcının mevcut paketi hala aktifse ve farklı bir paket almak istiyorsa
            if (user.PackageEndDate >= DateTime.UtcNow)
            {
                var remainingDays = (user.PackageEndDate - DateTime.UtcNow)?.Days ?? 0;
                var newPackageDays = package.ValidityTotalDay;

                // Mevcut paket süresi yeni paketin süresinden daha uzunsa hata verelim
                if (remainingDays > newPackageDays)
                {
                    return (false, null, "Mevcut paket süreniz yeni paketin süresinden uzun. Daha kısa süreli bir paket alamazsınız.", 2, null);
                }

                // Fiyat hesaplaması yapalım
                var newPackagePrice = package.Price;
                var currentPackagePrice = user.Package.Price;

                var proratedAmount = (currentPackagePrice / 30) * remainingDays;
                var upgradePrice = newPackagePrice - proratedAmount;

                var newEndDate = user.PackageEndDate?.AddDays(newPackageDays - remainingDays) ?? DateTime.UtcNow.AddDays(newPackageDays);

                var packageDto = _mapper.Map<PackageDto>(package);
                packageDto.PackageDiscountPrice = upgradePrice;

                return (true, packageDto, "Paket yükseltme fiyatı hesaplandı.", 3, newEndDate);
            }

            return (false, null, "Geçersiz işlem.", 0, null);
        }
    }

}
