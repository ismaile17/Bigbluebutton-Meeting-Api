using Application.Packages.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;

namespace Application.Packages.Commands.CreatePackage
{
    public class CreatePackageCommand:IRequest<ResultSingle<PackageDto>>
    {
        public int AppUserId { get; set; }
        public int ParentId { get; set; }
        public int Duration { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }
        public int Price { get; set; }
        public string PriceCurrency { get; set; }
        public string SmsCountGift { get; set; }
        public int ValidityTotalDay { get; set; }
        public bool CloudRecording { get; set; }
        public int CloudRecordingGbSize { get; set; }
        public int SessionHours { get; set; }
        public bool SutdyRooms { get; set; }
        public string Logo { get; set; }
    }

    public class CreatePackageCommandHandler : IRequestHandler<CreatePackageCommand, ResultSingle<PackageDto>>
    {
        private readonly IRepository<Package> _packageRepository;
        private readonly IMapper _mapper;

        public CreatePackageCommandHandler(IRepository<Package> packageRepository, IMapper mapper)
        {
            _packageRepository = packageRepository;
            _mapper = mapper;
        }

        public async Task<ResultSingle<PackageDto>> Handle(CreatePackageCommand request, CancellationToken cancellationToken)
        {
            var package = new Package
            {
                CreatedBy = request.AppUserId,
                Name = request.Name,
                Description = request.Description,
                Detail = request.Detail,
                Price = request.Price,
                PriceCurrency = request.PriceCurrency,
                SmsCountGift = request.SmsCountGift,
                ValidityTotalDay = request.ValidityTotalDay,
                CloudRecording = request.CloudRecording,
                CloudRecordingGbSize = request.CloudRecordingGbSize,
                SessionHours = request.SessionHours,
                SutdyRooms = request.SutdyRooms,
                Logo = request.Logo,
                Duration = request.Duration,
                ParentID = request.ParentId
            };
            _packageRepository.InsertWithoutCommit(package);
            var result = await _packageRepository.CommitAsync(cancellationToken);
            if(result ==-1)
            {
                return Result.Fail<PackageDto>(null, $"Kayıt edilemedi");
            }
            var data = await Task.Run(() => _mapper.Map<Package, PackageDto>(package));
            return Result.Ok<PackageDto>(data, $"Paket oluşturuldu");
        }
    }
}
