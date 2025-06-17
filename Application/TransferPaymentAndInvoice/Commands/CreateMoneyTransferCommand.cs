using Application.Packages.Model;
using Application.Shared.Results;
using Application.TransferPaymentAndInvoice.Model;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.TransferPaymentAndInvoice.Commands
{
    public class CreateMoneyTransferCommand : IRequest<ResultSingle<MoneyTransferFormDto>>
    {
        public int AppUserId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PhoneNumber { get; set; }
        public int PackageId { get; set; }
    }

    public class CreateMoneyTransferCommandHandler : IRequestHandler<CreateMoneyTransferCommand, ResultSingle<MoneyTransferFormDto>>
    {
        private readonly IRepository<MoneyTransferForm> _moneyTransferRepository;
        private readonly IMapper _mapper;
        private readonly TelegramService _telegramService;

        public CreateMoneyTransferCommandHandler(IRepository<MoneyTransferForm> moneyTransferRepository, IMapper mapper, TelegramService telegramService)
        {
            _moneyTransferRepository = moneyTransferRepository;
            _mapper = mapper;
            _telegramService = telegramService;
        }

        public async Task<ResultSingle<MoneyTransferFormDto>> Handle(CreateMoneyTransferCommand request, CancellationToken cancellationToken)
        {
            var moneyTransfer = new MoneyTransferForm
            {
                CreatedBy = request.AppUserId,
                Name = request.Name,
                Surname = request.Surname,
                PhoneNumber = request.PhoneNumber,
                PackageId = request.PackageId
            };

            _moneyTransferRepository.InsertWithoutCommit(moneyTransfer);
            var result = await _moneyTransferRepository.CommitAsync(cancellationToken);

            if (result == -1)
            {
                return Result.Fail<MoneyTransferFormDto>(null, "Kayıt edilemedi");
            }

            _ = _telegramService.SendMessageAsync($"MONEY TRANSFER:\r\n Kişi: {moneyTransfer.Name} {moneyTransfer.Surname} \r\n Telefon Numarası: {moneyTransfer.PhoneNumber} \r\n Paket: {moneyTransfer.PackageId}");


            var data = _mapper.Map<MoneyTransferForm, MoneyTransferFormDto>(moneyTransfer);
            return Result.Ok<MoneyTransferFormDto>(data, "Transfer formu oluşturuldu");
        }
    }
}