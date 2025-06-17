using Application.Packages.Model;
using Application.Packages.Services;
using Application.Shared.Results;
using AutoMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Packages.Queries
{
    public class GetPackageByIdQuery : IRequest<ResultSingle<PackageDto>>
    {
        public int PackageId { get; set; }
        public int AppUserId { get; set; }
    }

    public class GetPackageByIdQueryHandler : IRequestHandler<GetPackageByIdQuery, ResultSingle<PackageDto>>
    {
        private readonly GetPackageByIdCheckService _getPackageService;
        private readonly IMapper _mapper;

        public GetPackageByIdQueryHandler(GetPackageByIdCheckService getPackageService, IMapper mapper)
        {
            _getPackageService = getPackageService;
            _mapper = mapper;
        }

        public async Task<ResultSingle<PackageDto>> Handle(GetPackageByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _getPackageService.GetPackageByIdAsync(request.PackageId, request.AppUserId, cancellationToken);

            bool isValid = result.isValid;
            PackageDto package = result.package;
            string message = result.message;
            int status = result.status;
            DateTime? newEndDate = result.newEndDate;

            if (!isValid)
            {
                return Result.Fail<PackageDto>(null, message);
            }

            var packageDto = _mapper.Map<PackageDto>(package);
            packageDto.PackageDiscountPrice = package.PackageDiscountPrice;
            packageDto.NewEndDate = newEndDate;

            return Result.Ok(packageDto, message);
        }
    }
}
