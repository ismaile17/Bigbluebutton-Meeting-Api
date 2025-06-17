using Application.Shared.Results;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Accounts.Commands.ResetPassword
{
    public class ResetPasswordCommand : IRequest<ResultSingle<bool>>
    {
        public string Key { get; set; }

        public string? Password { get; set; }
        public string? PasswordRepeat { get; set; }

    }

    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResultSingle<bool>>
    {
        private readonly IRepository<UserRecovery> _userRecoveryRepository;
        private readonly UserManager<AppUser> _userManager;

        public ResetPasswordCommandHandler(IRepository<UserRecovery> userRecoveryRepository, UserManager<AppUser> userManager)
        {
            _userRecoveryRepository = userRecoveryRepository;
            _userManager = userManager;
        }

        public async Task<ResultSingle<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            if (request.Password == null || request.PasswordRepeat == null)
            {
                return Result.Fail<bool>(false, "Şifre alanları boş olamaz!");
            }

            if (!request.Password.Equals(request.PasswordRepeat))
            {
                return Result.Fail<bool>(false, "Şifreler uyuşmuyor!");
            }


            DateTime dateTime = DateTime.Now;
            var recovery = _userRecoveryRepository.Get(a => a.Key == request.Key && a.IsActive == (short)1);
            if (recovery == null)
            {
                return Result.Fail<bool>(false, "Geçerli bir sıfırlama talebi bulunamadı.");
            }

            var appUser = await _userManager.FindByIdAsync(recovery.AppUserId.ToString());
            if (appUser == null)
            {
                return Result.Fail<bool>(false, "Kullanıcı bulunamadı");
            }

        

            var resetPassord = await _userManager.ResetPasswordAsync(appUser, await _userManager.GeneratePasswordResetTokenAsync(appUser), request.Password);

            if (!resetPassord.Succeeded)
                return Result.Fail<bool>(false, $"Şifre sıfırlanırken bir hata oldu! Lütfen tekrar deneyin");

          
            recovery.IsActive = (short)-1;
            _userRecoveryRepository.Update(recovery);


            return Result.Ok<bool>(true, "Şifreniz değiştirildi. Yeni şifrenizle giriş yapabilirsiniz.");

        }
    }

}

