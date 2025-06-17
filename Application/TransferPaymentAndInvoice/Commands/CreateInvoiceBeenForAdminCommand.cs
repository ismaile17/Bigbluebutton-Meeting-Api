using Application.Packages.Services;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Payment;

namespace Application.TransferPaymentAndInvoice.Commands
{
    public class CreateInvoiceBeenForAdminCommand : IRequest<ResultSingle<PurchaseDto>>
    {
        public int AppUserId { get; set; }
        public int PurchaseId { get; set; }

    }

    public class InvoiceBeenCreateForAdminCommandHandler : IRequestHandler<CreateInvoiceBeenForAdminCommand, ResultSingle<PurchaseDto>>
    {
        private readonly IRepository<Purchase> _purchaseRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public InvoiceBeenCreateForAdminCommandHandler(IRepository<Purchase> purchaseRepository, UserManager<AppUser> userManager, IMapper mapper)
        {
            _purchaseRepository = purchaseRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ResultSingle<PurchaseDto>> Handle(CreateInvoiceBeenForAdminCommand request, CancellationToken cancellationToken)
        {
            // Admin kontrolü
            var adminControl = await _userManager.FindByIdAsync(request.AppUserId.ToString());
            if (adminControl == null)
                return Result.Fail<PurchaseDto>(null, "Kullanıcı bulunamadı!");

            var isAdmin = await _userManager.IsInRoleAsync(adminControl, "Admin");
            if (!isAdmin)
                return Result.Fail<PurchaseDto>(null, "Sadece Admin yetkisine sahip kullanıcılar işlem yapabilir.");

            var purchase = _purchaseRepository.Get(a => a.Id == request.PurchaseId);

            purchase.InvoiceBeenCreate = true;

            _purchaseRepository.UpdateWithoutCommit(purchase);


            var purchaseResult = await _purchaseRepository.CommitAsync(cancellationToken);
                if (purchaseResult == -1)
                {
                    return Result.Fail<PurchaseDto>(null, "Kayıt edilemedi");
                }

                var purchaseDto = _mapper.Map<Purchase, PurchaseDto>(purchase);
                return Result.Ok<PurchaseDto>(purchaseDto, "Satın alma kaydedildi");
            }
        }
}
