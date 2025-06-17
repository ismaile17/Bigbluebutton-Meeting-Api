using Application.Shared.Results;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Accounts.Commands.ChangePassword
{
    public class ChangePasswordCommand : IRequest<ResultSingle<bool>>
    {
        public int AppUserId { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }

    }

    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ResultSingle<bool>>
    {
        private readonly IRepository<UserRecovery> _userRecoveryRepository;
        private readonly UserManager<AppUser> _userManager;

        public ChangePasswordCommandHandler( IRepository<UserRecovery> userRecoveryRepository, UserManager<AppUser> userManager)
        {
            _userRecoveryRepository = userRecoveryRepository;
            _userManager = userManager;
        }

        public async Task<ResultSingle<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
          
            if (request.CurrentPassword != null && request.ConfirmPassword != null && request.NewPassword != null)
            {
                var appUser = await _userManager.FindByIdAsync(request.AppUserId.ToString());
                if (appUser == null)
                {
                    return Result.Fail<bool>(false, "Kullanıcı bulunamadı");
                }

                var checkPassword = await _userManager.CheckPasswordAsync(appUser, request.CurrentPassword);
                if (!checkPassword)
                {
                    return Result.Fail<bool>(false, "Şu anki şifreniz yanlış!");
                }
                if (!request.NewPassword.Equals(request.ConfirmPassword))
                {
                    return Result.Fail<bool>(false, "Yeni şifreler birbiriyle uyuşmuyor!");
                }
                var resetPassord = await _userManager.ResetPasswordAsync(appUser, await _userManager.GeneratePasswordResetTokenAsync(appUser), request.NewPassword);
                if (!resetPassord.Succeeded)
                {
                    return Result.Fail<bool>(false, "Şifre değiştirme başarısız!");
                }
            }
            else
            {
                return Result.Fail<bool>(false, "Şifreleri kontrol ediniz!");
            }


            return Result.Ok<bool>(true, $"Bilgileriniz güncellendi!");

        }
    }

}

