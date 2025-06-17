using Application.Shared.Results;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Payment;

namespace Application.TransferPaymentAndInvoice.Queries
{
    public class GetPurchasesListByUserIdQuery : IRequest<Result<PurchaseDto>>
    {
        public int AppUserId { get; set; }
    }

    public class GetPurchasesListByUserIdQueryHandler : IRequestHandler<GetPurchasesListByUserIdQuery, Result<PurchaseDto>>
    {
        private readonly IRepository<Purchase> _purchasesListRepository;
        private readonly IRepository<Domain.Entities.Cupon> _cuponRepository;
        private readonly IMapper _mapper;

        public GetPurchasesListByUserIdQueryHandler(IRepository<Purchase> purchasesListRepository, IMapper mapper)
        {
            _purchasesListRepository = purchasesListRepository;
            _mapper = mapper;
        }

        public async Task<Result<PurchaseDto>> Handle(GetPurchasesListByUserIdQuery request, CancellationToken cancellationToken)
        {
            var purchasesList = _purchasesListRepository.GetMany(a => a.AppUserId == request.AppUserId && a.StatusType == PurchaseStatusType.Approved);

            var data = await purchasesList
                .ProjectTo<PurchaseDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result.Ok(data, "Başarılı");
        }
    }
}
