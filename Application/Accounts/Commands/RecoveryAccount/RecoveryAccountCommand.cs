using Application.Accounts.Commands.Login;
using Application.Accounts.Model;
using Application.Emails.Commands.SendEmail;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Shared.Jwt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Accounts.Commands.RecoveryAccount
{
    public class RecoveryAccountCommand : IRequest<ResultSingle<UserDto>>
    {
        public string Email { get; set; }

    }

    public class RecoveryAccountCommandHandler : IRequestHandler<RecoveryAccountCommand, ResultSingle<UserDto>>
    {
        private readonly IRepository<UserRecovery> _userRecoveryRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly IMapper _mapper;
        private readonly MeetingDbContext _dbContext;
        private readonly IMediator _mediator;

        public RecoveryAccountCommandHandler( IRepository<UserRecovery> userRecoveryRepository, UserManager<AppUser> userManager, IOptions<JwtSettings> jwtSettings, IMapper mapper, MeetingDbContext dbContext, IMediator mediator)
        {
            _userRecoveryRepository = userRecoveryRepository;
            _userManager = userManager;
            _jwtSettings = jwtSettings;
            _mapper = mapper;
            _dbContext = dbContext;
            _mediator = mediator;
        }

        public async Task<ResultSingle<UserDto>> Handle(RecoveryAccountCommand request, CancellationToken cancellationToken)
        {
            var appUser = await _userManager.FindByEmailAsync(request.Email);
            if (appUser == null)
            {
                return Result.Fail<UserDto>(null, "Kullanıcı bulunamadı");
            }

       


            var recovery = _userRecoveryRepository.GetMany(g => g.AppUserId == appUser.Id).OrderByDescending(a => a.Id);
            if (recovery.Any())
            {
                DateTime dateTime = DateTime.Now;
                if ((dateTime - recovery.FirstOrDefault().CreatedTime).Minutes <= 1)
                    return Result.Fail<UserDto>(null, $"1 dk içinde bu kadar sık istekte bulunamazsınız");
            }

            var userRecovery = new UserRecovery
            {
                AppUserId = appUser.Id,
                Key = Guid.NewGuid().ToString()
            };
            _userRecoveryRepository.InsertWithoutCommit(userRecovery);
            var result = await _userRecoveryRepository.CommitAsync(cancellationToken);
            if (result == -1)
                return Result.Fail<UserDto>(null, $"Şifre oluşturulurken hata oluştu. Tekrar deneyiniz.");



            var email = await _mediator.Send(new SendEmailCommand { Display = "Block", Type = 2, Email = request.Email, FullName = appUser.UserName, Subject = "Şifre Yenileme Talebi", ButtonText = "Şifreyi Sıfırla", Url = $"={userRecovery.Key}", Body = "Şifre yenileme isteğini aldık.", Information = "Şifre yenileme linki 60 dakika sonra iptal olacaktır. " });
            if (email.Success)
                return Result.Ok<UserDto>(new UserDto { }, $"Mail adresinize şifre sıfırlama linki gönderildi.");
            return Result.Fail<UserDto>(null, "Lütfen tekrar deneyin");

        }
    }

}

