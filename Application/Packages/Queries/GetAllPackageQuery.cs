using Application.Packages.Model;
using Application.Shared.Results;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Packages.Queries
{
    public class GetAllPackageQuery:IRequest<Result<PackageDto>>
    {
    }

    public class GetAllPackageQueryHandler : IRequestHandler<GetAllPackageQuery, Result<PackageDto>>
    {
        private readonly IRepository<Package> _packageRepository;
        private readonly IMapper _mapper;

        public GetAllPackageQueryHandler(IRepository<Package> packageRepository, IMapper mapper)
        {
            _packageRepository = packageRepository;
            _mapper = mapper;
        }

        public async Task<Result<PackageDto>> Handle(GetAllPackageQuery request, CancellationToken cancellationToken)
        {
            var data = await Task.Run(() => _packageRepository.GetAll.Where(p=>p.IsActive==1)).Result.ProjectTo<PackageDto>(_mapper.ConfigurationProvider).OrderByDescending(a => a.Id).ToListAsync();
            return Result.Ok(data.AsQueryable(), $"Başarılı");
        }
    }
}
