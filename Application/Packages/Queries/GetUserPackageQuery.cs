using Application.Packages.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Packages.Queries
{
    public class GetUserPackageQuery : IRequest<ResultSingle<PackageDto>>
    {
        public int AppUserId { get; set; }
    }

    public class GetUserPackageQueryHandler : IRequestHandler<GetUserPackageQuery, ResultSingle<PackageDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public GetUserPackageQueryHandler(UserManager<AppUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ResultSingle<PackageDto>> Handle(GetUserPackageQuery request, CancellationToken cancellationToken)
        {
            // Kullanıcıyı ve ilişkili paket bilgilerini alıyoruz
            var user = await _userManager.Users
                .Include(u => u.Package) // Kullanıcının paket bilgilerini dahil ediyoruz
                .FirstOrDefaultAsync(u => u.Id == request.AppUserId, cancellationToken);

            if (user == null || user.Package == null)
            {
                return Result.Fail<PackageDto>(null, "Kullanıcının paketi yok.");
            }

            var packageDto = _mapper.Map<PackageDto>(user.Package);

            // Paket bitiş gününü hesaplıyoruz
            var remainingDays = (user.PackageEndDate.HasValue)
                ? (user.PackageEndDate.Value - DateTime.UtcNow).Days
                : (int?)null;  // PaketEndDate null ise null dönecek.

            packageDto.PackageFinishDay = remainingDays ?? 0;  // Eğer gün yoksa 0 olarak ayarlanır.

            return Result.Ok(packageDto, "Başarılı");
        }
    }
}
