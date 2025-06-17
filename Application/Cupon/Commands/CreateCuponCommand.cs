using Application.Cupon.Models;
using Application.Shared.Results;
using AutoMapper;
using Infrastructure.Persistence;
using MediatR;

namespace Application.Cupon.Commands
{
    public class CreateCuponCommand : IRequest<ResultSingle<CuponDto>>
    {
        public int AppUserId { get; set; }
        public string? Email { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? DiscountType { get; set; } // "Percentage", "Fixed", "MinimumOrderPercentage", "MinimumOrderFixed"
        public decimal DiscountValue { get; set; }
        public decimal MinimumOrderValue { get; set; }
        public int UsageLimit { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int UsedCount { get; set; }
    }

    public class CreateCuponCommandHandler : IRequestHandler<CreateCuponCommand, ResultSingle<CuponDto>>
    {
        private readonly IRepository<Domain.Entities.Cupon> _cuponRepository;
        private readonly IMapper _mapper;

        public CreateCuponCommandHandler(IRepository<Domain.Entities.Cupon> cuponRepository, IMapper mapper)
        {
            _cuponRepository = cuponRepository;
            _mapper = mapper;
        }

        public async Task<ResultSingle<CuponDto>> Handle(CreateCuponCommand request, CancellationToken cancellationToken)
        {
            if(request.Code ==null)
            {
                request.Code = GenerateRandomString(7);
            }


            var cupon = new Domain.Entities.Cupon
            {
                CreatedBy = request.AppUserId,
                Name = request.Name,
                Code = request.Code,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                MinimumOrderValue = request.MinimumOrderValue,
                UsageLimit = request.UsageLimit,
                ExpiryDate = request.ExpiryDate,
                UsedCount = request.UsedCount
            };

            _cuponRepository.InsertWithoutCommit(cupon);
            var result = await _cuponRepository.CommitAsync(cancellationToken);
            if (result == -1)
            {
                return Result.Fail<CuponDto>(null, $"Kayıt edilemedi");
            }
            var data = await Task.Run(() => _mapper.Map<Domain.Entities.Cupon, CuponDto>(cupon));
            return Result.Ok<CuponDto>(data, $"Paket oluşturuldu");
        }
        string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var randomString = new char[length];

            for (int i = 0; i < length; i++)
            {
                randomString[i] = chars[random.Next(chars.Length)];
            }

            return new string(randomString);
        }
    }

}

