using Application.Accounts.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Accounts.Queries
{
    public class GetUserNameEmailEtcQuery : IRequest<ResultSingle<UserNameMailEtcInfoDto>>
    {
        public int AppUserId { get; set; } // Kullanıcıyı sorgulamak için gerekli ID
    }

    public class GetUserNameEmailEtcQueryHandler : IRequestHandler<GetUserNameEmailEtcQuery, ResultSingle<UserNameMailEtcInfoDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public GetUserNameEmailEtcQueryHandler(UserManager<AppUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ResultSingle<UserNameMailEtcInfoDto>> Handle(GetUserNameEmailEtcQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync((request.AppUserId).ToString());

            if (user == null)
            {
                return ResultSingle<UserNameMailEtcInfoDto>.Fail(new UserNameMailEtcInfoDto(), "Kullanıcı bulunamadı.");
            }

            var userDto = _mapper.Map<UserNameMailEtcInfoDto>(user);
            return ResultSingle<UserNameMailEtcInfoDto>.Ok(userDto, "Başarılı");

        }
    }
}
