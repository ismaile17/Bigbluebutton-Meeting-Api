using Application.Accounts.Model;
using Application.Emails.Commands.SendEmail;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Accounts.Commands.Register
{
    public class RegisterCommand : IRequest<Result<UserDto>>
    {
        public string Email { get; set; }

        public string FullName { get; set; }

        public string? Phone { get; set; }
        public string Password { get; set; }
    }

    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<UserDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public RegisterCommandHandler()
        {
        }

        public RegisterCommandHandler(UserManager<AppUser> userManager, IMapper mapper, IMediator mediator)
        {
            _userManager = userManager;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<Result<UserDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            request.Email = request.Email.ToLower().Trim();
           
            var appUser = await _userManager.FindByEmailAsync(request.Email.ToLower());

            if (appUser != null)
            {
                return Result.Fail<UserDto>($"{request.Email} mail adresi daha önce kayıt olmuş.");
            }
          
            var createAppUser = await _userManager.CreateAsync(new AppUser { UserName = request.Email.ToLower(), Email = request.Email.ToLower(), UserType = 1 }, request.Password);

            if (!createAppUser.Succeeded)
            {
                return Result.Fail<UserDto>($"Kayıt olurken bir hatayla karşılaşıldı. Formu inceleyiniz");
            }

            var getAppUser = await _userManager.FindByEmailAsync(request.Email.ToLower());
            if (getAppUser == null)
            {
                return Result.Fail<UserDto>($"Kayıt olurken bir hatayla karşılaşıldı. Formu inceleyiniz");
            }
            var addRole = await _userManager.AddToRoleAsync(getAppUser, "Teacher");
            if (!addRole.Succeeded)
            {
                await _userManager.DeleteAsync(getAppUser);
                return Result.Fail<UserDto>($"Kayıt olurken bir hatayla karşılaşıldı. Lütfen iletişime geçiniz");
            }
         
            var sendMail = await _mediator.Send(new SendEmailCommand { Type = 1, Email = request.Email, FullName = request.FullName, Subject = "Hoş " });
            return Result.Ok<UserDto>($"Kayıt başarılı. Giriş yapabilirsiniz.");

        }
    }

}
