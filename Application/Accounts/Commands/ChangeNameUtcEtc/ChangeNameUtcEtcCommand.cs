using Application.Accounts.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Accounts.Commands.ChangeNameUtcEtc
{
    public class ChangeNameUtcEtcCommand : IRequest<ResultSingle<UserNameMailEtcInfoDto>>
    {
        public int AppUserId { get; set; }
        public string? FullName { get; set; }
        public string? TimeZoneId { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class ChangeNameUtcEtcCommandHandler : IRequestHandler<ChangeNameUtcEtcCommand, ResultSingle<UserNameMailEtcInfoDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public ChangeNameUtcEtcCommandHandler(UserManager<AppUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ResultSingle<UserNameMailEtcInfoDto>> Handle(ChangeNameUtcEtcCommand request, CancellationToken cancellationToken)
        {
            var appUser = await _userManager.FindByIdAsync(request.AppUserId.ToString());

            if (appUser == null)
            {
                return Result.Fail<UserNameMailEtcInfoDto>(null, "Kullanıcı bulunamadı");
            }

            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                appUser.FullName = request.FullName;
            }

            if (!string.IsNullOrWhiteSpace(request.TimeZoneId))
            {
                appUser.TimeZoneId = request.TimeZoneId;
            }

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                appUser.PhoneNumber = request.PhoneNumber;
            }

            var result = await _userManager.UpdateAsync(appUser);

            if (!result.Succeeded)
            {
                return Result.Fail<UserNameMailEtcInfoDto>(null, "Bilgiler güncellenirken bir hata oluştu");
            }

            var userDto = _mapper.Map<UserNameMailEtcInfoDto>(appUser);
            return Result.Ok(userDto, "Bilgiler başarıyla güncellendi!");
        }
    }
}
