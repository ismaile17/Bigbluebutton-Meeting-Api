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
    public class CreatePackageDefinitionForAdminCommand : IRequest<ResultSingle<PurchaseDto>>
    {
        public int AppUserId { get; set; }
        public int DefinedAppUserId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PhoneNumber { get; set; }
        public string? AdminNote { get; set; }
        public double Price { get; set; }
        public int PackageId { get; set; }
        public int? SendEndDay { get; set; }  // Kaç gün olduğunu belirtecek integer
        public PurchaseType PurchaseType { get; set; }
        public int? MoneyTransferFormId { get; set; }  // Eğer transfer form ID varsa
    }

    public class CreatePackageDefinitionForAdminCommandHandler : IRequestHandler<CreatePackageDefinitionForAdminCommand, ResultSingle<PurchaseDto>>
    {
        private readonly IRepository<MoneyTransferForm> _moneyTransferRepository;
        private readonly IRepository<Purchase> _purchaseRepository;
        private readonly IRepository<Package> _packageRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public CreatePackageDefinitionForAdminCommandHandler(IRepository<MoneyTransferForm> moneyTransferRepository, IRepository<Purchase> purchaseRepository, IRepository<Package> packageRepository, UserManager<AppUser> userManager, IMapper mapper)
        {
            _moneyTransferRepository = moneyTransferRepository;
            _purchaseRepository = purchaseRepository;
            _packageRepository = packageRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ResultSingle<PurchaseDto>> Handle(CreatePackageDefinitionForAdminCommand request, CancellationToken cancellationToken)
        {
            // Admin kontrolü
            var adminControl = await _userManager.FindByIdAsync(request.AppUserId.ToString());
            if (adminControl == null)
                return Result.Fail<PurchaseDto>(null, "Kullanıcı bulunamadı!");

            var isAdmin = await _userManager.IsInRoleAsync(adminControl, "Admin");
            if (!isAdmin)
                return Result.Fail<PurchaseDto>(null, "Sadece Admin yetkisine sahip kullanıcılar işlem yapabilir.");

            // Kullanıcıyı alıyoruz
            var user = await _userManager.FindByIdAsync(request.DefinedAppUserId.ToString());
            if (user == null)
                return Result.Fail<PurchaseDto>(null, "Kullanıcı bulunamadı.");

            // Satın alınan paketi PackageId üzerinden alıyoruz
            var package = _packageRepository.GetById(request.PackageId);
            if (package == null)
                return Result.Fail<PurchaseDto>(null, "Paket bulunamadı.");

            // Kullanıcının aktif paket bilgisi üzerinden işlem yapalım
            DateTime? packageEndDate = user.PackageEndDate;
            bool isValid = true;
            DateTime? newEndDate = null;
            string message = string.Empty;


                // Paket bilgisi kontrolü için servisi çağırıyoruz
                var packageService = new GetPackageByIdCheckService(_packageRepository, _userManager, _mapper);
                var result = await packageService.GetPackageByIdAsync(request.PackageId, request.DefinedAppUserId, cancellationToken);

                isValid = result.isValid;

                if (isValid == false)
                {
                    return Result.Fail<PurchaseDto>(null, "Service hata.");
                }

            if (request.SendEndDay != null)
            {
                if (result.status == 3 && isValid == true)
                {
                    newEndDate = result.newEndDate;
                }
                if (result.status == 0 && isValid == true)
                {
                    newEndDate = DateTime.Now.AddDays(package.Duration ?? 30);
                }
                else if (result.status == 1 && isValid == true)
                {
                    if (packageEndDate < DateTime.Now || user.PackageId == null)
                    {
                        // Eğer PackageEndDate bugünden küçükse, bugünün tarihine duration ekle
                        newEndDate = DateTime.Now.AddDays(package.Duration ?? 30);
                    }
                    else
                    {
                        // Eğer PackageEndDate bugünden büyükse, mevcut PackageEndDate'e duration ekle
                        newEndDate = packageEndDate?.AddDays(package.Duration ?? 0) ?? DateTime.Now.AddDays(package.Duration ?? 0);
                    }
                }
                message = result.message;
            }
            else
            {
                newEndDate = DateTime.Now.AddDays(request.SendEndDay.Value);
                message = "Admin Ataması";
            }

            

            if (isValid)
            {
                var packageFinalEndDate = newEndDate;

                // Kullanıcının paket bilgilerini güncelliyoruz
                user.PackageId = package.Id;
                user.PackageBuyDate = DateTime.Now;
                user.PackageEndDate = packageFinalEndDate;
                user.CheckMessage = message;

                var userUpdateResult = await _userManager.UpdateAsync(user);
                if (!userUpdateResult.Succeeded)
                {
                    return Result.Fail<PurchaseDto>(null, "Kullanıcı bilgileri güncellenemedi.");
                }

                // Satın alma işlemi oluşturuluyor
                var purchase = new Purchase
                {
                    AppUserId = request.DefinedAppUserId,
                    PackageId = request.PackageId,
                    PurchaseType = request.PurchaseType,
                    OrderId = Guid.NewGuid().ToString(),
                    Explain = request.AdminNote ?? message,
                    Price = request.Price,
                    StatusType = PurchaseStatusType.Approved,
                    PurchaseToken = Guid.NewGuid().ToString(),
                    EndDate = packageFinalEndDate
                };

                if (request.MoneyTransferFormId != null)
                {
                    // Transfer formu varsa, success değerini true yapalım
                    var moneyTransferForm = _moneyTransferRepository.GetById(request.MoneyTransferFormId.Value);
                    if (moneyTransferForm == null)
                    {
                        return Result.Fail<PurchaseDto>(null, "Geçersiz MoneyTransferFormId");
                    }
                    moneyTransferForm.Success = true;
                    moneyTransferForm.UpdatedTime = DateTime.Now;
                    moneyTransferForm.UpdatedBy = adminControl.Id;
                    moneyTransferForm.AdminNote = request.AdminNote;
                    _moneyTransferRepository.Update(moneyTransferForm);
                    await _moneyTransferRepository.CommitAsync(cancellationToken);
                }

                _purchaseRepository.InsertWithoutCommit(purchase);

                var purchaseResult = await _purchaseRepository.CommitAsync(cancellationToken);
                if (purchaseResult == -1)
                {
                    return Result.Fail<PurchaseDto>(null, "Kayıt edilemedi");
                }

                var purchaseDto = _mapper.Map<Purchase, PurchaseDto>(purchase);
                return Result.Ok<PurchaseDto>(purchaseDto, "Satın alma kaydedildi");
            }
            else
            {
                return Result.Fail<PurchaseDto>(null, "Kayıt edilemedi. Check Service Hata Döndü.");
            }
        }
    }
}
