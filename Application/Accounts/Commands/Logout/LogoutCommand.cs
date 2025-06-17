using Application.Accounts.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Accounts.Commands.Logout
{
    public class LogoutCommand : IRequest<Result<UserDto>>
    {
        public int AppUserId { get; set; }
    }

    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<UserDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public LogoutCommandHandler(UserManager<AppUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<Result<UserDto>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {

      
            var appUser = await _userManager.FindByIdAsync(request.AppUserId.ToString());
            if (appUser == null)
            {
                return Result.Fail<UserDto>($"Kullanıcı bulunamadı!");
            }
            var logout = await _userManager.RemoveAuthenticationTokenAsync(appUser, "meeting.api", "AccessToken");
            if (!logout.Succeeded)
            {
                return Result.Fail<UserDto>($"Çıkış yapılırken hata oluştu");
            }
            return Result.Ok<UserDto>($"İşlem başarılı");

        }
    }

}
