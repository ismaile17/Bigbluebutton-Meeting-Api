using Application.Cupon.Models;
using Application.Cupon.Service;
using Application.Shared.Results;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace Application.Cupon.Queries
{
    public class GetCuponChechByCuponQuery : IRequest<ResultSingle<CuponCheckAnswerDto>>
    {
        public string Code { get; set; }
        public int PackageId { get; set; }
    }

    public class GetCuponChechByCuponQueryHandler : IRequestHandler<GetCuponChechByCuponQuery, ResultSingle<CuponCheckAnswerDto>>
    {
        private readonly CuponCheckService _cuponCheckService;
        private readonly IMediator _mediator;
        private readonly IRepository<Package> _packageRepository;
        private readonly IRepository<Domain.Entities.Cupon> _couponRepository;

        public GetCuponChechByCuponQueryHandler(CuponCheckService cuponCheckService, IMediator mediator, IRepository<Package> packageRepository, IRepository<Domain.Entities.Cupon> couponRepository)
        {
            _cuponCheckService = cuponCheckService;
            _mediator = mediator;
            _packageRepository = packageRepository;
            _couponRepository = couponRepository;
        }

        public async Task<ResultSingle<CuponCheckAnswerDto>> Handle(GetCuponChechByCuponQuery request, CancellationToken cancellationToken)
        {
            
            if (request.Code == null)
            {
                return Result.Fail<CuponCheckAnswerDto>(new CuponCheckAnswerDto(), "Kod giriniz");
            }

            var package = await _packageRepository.GetMany(p => p.Id == request.PackageId && p.IsActive == 1).FirstOrDefaultAsync(cancellationToken);

            decimal totalAmount = package.Price;

            if (package == null)
            {
                return Result.Fail<CuponCheckAnswerDto>(new CuponCheckAnswerDto(),"Geçersiz kupon kodu.");
            }


            var normalizedCode = request.Code.ToLower();
            var cupon = _couponRepository.Get(c => c.Code.ToLower() == normalizedCode);

            if (cupon == null)
            {
                return Result.Fail<CuponCheckAnswerDto>(new CuponCheckAnswerDto(), "Geçersiz kupon kodu.");
            }

            if (cupon.ExpiryDate < DateTime.UtcNow)
            {
                return Result.Fail<CuponCheckAnswerDto>(new CuponCheckAnswerDto(), "Kuponun son kullanma tarihi geçmiş.");
            }

            if (cupon.UsedCount >= cupon.UsageLimit)
            {
                return Result.Fail<CuponCheckAnswerDto>(new CuponCheckAnswerDto(), "Kupon kullanım limiti dolmuş.");
            }

            if (cupon.MinimumOrderValue.HasValue && totalAmount < cupon.MinimumOrderValue.Value)
            {
                return Result.Fail<CuponCheckAnswerDto>(new CuponCheckAnswerDto(), $"Kuponu kullanabilmek için minimum sipariş tutarı {cupon.MinimumOrderValue} TL olmalıdır.");
               
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
                    return Result.Fail<CuponCheckAnswerDto>(new CuponCheckAnswerDto(), "Bilinmeyen indirim türü.");
            }

            discountedPrice = Math.Max(discountedPrice, 0);

            return Result.Ok(new CuponCheckAnswerDto { Id = cupon.Id, Code = request.Code, TrueFalse = true, DiscountedPrice = discountedPrice }, $"İndirim Uygulandı");
        }
    }
}
