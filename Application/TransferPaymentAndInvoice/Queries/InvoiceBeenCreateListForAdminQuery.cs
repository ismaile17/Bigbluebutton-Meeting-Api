using Application.Shared.Results;
using Application.TransferPaymentAndInvoice.Model;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Payment;

namespace Application.TransferPaymentAndInvoice.Queries
{
    public class InvoiceBeenCreateListForAdminQuery : IRequest<Result<PurchaseDto>>
    {
        public int AppUserId { get; set; }

    }

    public class InvoiceBeenCreateForAdminQueryHandler : IRequestHandler<InvoiceBeenCreateListForAdminQuery, Result<PurchaseDto>>
    {
        private readonly IRepository<Purchase> _purchaseRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public InvoiceBeenCreateForAdminQueryHandler(IRepository<Purchase> purchaseRepository, UserManager<AppUser> userManager, IMapper mapper)
        {
            _purchaseRepository = purchaseRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<Result<PurchaseDto>> Handle(InvoiceBeenCreateListForAdminQuery request, CancellationToken cancellationToken)
        {
            var purchaseListInvoice = _purchaseRepository.GetMany(a => a.StatusType == PurchaseStatusType.Approved && (a.InvoiceBeenCreate == false || a.InvoiceBeenCreate == null))
                                         .Include(a => a.AppUser)
                                         .ThenInclude(u => u.UserDetailSetting); // UserDetailSetting'ı dahil ediyoruz

            var purchases = await purchaseListInvoice.ToListAsync(cancellationToken);

            var purchaseDtos = purchases.Select(p => {
                var dto = new PurchaseDto
                {
                    // Purchase alanlarını map et
                    Id = p.Id,
                    PurchaseType = p.PurchaseType.ToString(),
                    AppUserId = p.AppUserId,
                    PackageId = p.PackageId,
                    Price = p.Price,
                    CouponId = p.CouponId,
                    CreatedTime = p.CreatedTime,
                    // ... diğer Purchase alanlarını map edin

                    // Bireysel fatura bilgilerini map et
                    Name = p.AppUser?.UserName,
                    Email = p.AppUser?.Email,
                    InvoiceType = p.AppUser?.UserDetailSetting?.InvoiceType,
                    InvoiceNameSurname = p.AppUser?.UserDetailSetting?.InvoiceNameSurname,
                    InvoiceNumber = p.AppUser?.UserDetailSetting?.InvoicePersonalNumber,
                    PhoneNumber = p.AppUser?.PhoneNumber,
                    InvoiceAddress = p.AppUser?.UserDetailSetting?.InvoiceAddress
                };

                // Kurumsal fatura bilgilerini ekleme
                if (dto.InvoiceType == "Kurumsal Fatura")
                {
                    dto.InvoiceNameSurname = p.AppUser?.UserDetailSetting?.BusinessName;
                    dto.InvoiceNumber = p.AppUser?.UserDetailSetting?.BusinessNumber;
                    dto.InvoiceCountry = p.AppUser?.UserDetailSetting?.BusinessCountry +" / "+p.AppUser?.UserDetailSetting?.BusinessCity+" / "+p.AppUser?.UserDetailSetting?.BusinessProvince;
                    dto.InvoiceAddress = p.AppUser?.UserDetailSetting?.BusinessAddress;
                    dto.InvoiceVD = p.AppUser?.UserDetailSetting?.BusinessVD;
                }

                return dto;
            }).ToList();

            return Result.Ok(purchaseDtos, "Başarılı");

        }

    }

}
