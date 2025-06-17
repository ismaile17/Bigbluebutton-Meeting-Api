using Application.Shared.Results;
using Application.TransferPaymentAndInvoice.Model;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.TransferPaymentAndInvoice.Queries
{
    public class GetMoneyTransferListForAdminQuery : IRequest<Result<MoneyTransferFormDto>>
    {
        public int AppUserId { get; set; }

    }

    public class GetMoneyTransferListForAdminQueryHandler : IRequestHandler<GetMoneyTransferListForAdminQuery, Result<MoneyTransferFormDto>>
    {
        private readonly IRepository<MoneyTransferForm> _moneyTransferFormRepository;
        private readonly IMapper _mapper;

        public GetMoneyTransferListForAdminQueryHandler(IRepository<MoneyTransferForm> moneyTransferFormRepository, IMapper mapper)
        {
            _moneyTransferFormRepository = moneyTransferFormRepository;
            _mapper = mapper;
        }

        public async Task<Result<MoneyTransferFormDto>> Handle(GetMoneyTransferListForAdminQuery request, CancellationToken cancellationToken)
        {

            var moneyTransferList = _moneyTransferFormRepository.GetMany(a => !a.Success);

            var data = await moneyTransferList
                .ProjectTo<MoneyTransferFormDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result.Ok(data, "Başarılı");

        }

    }
    
}
