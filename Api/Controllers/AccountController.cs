using Application.Accounts.Commands.ChangeNameUtcEtc;
using Application.Accounts.Commands.ChangePassword;
using Application.Accounts.Commands.Login;
using Application.Accounts.Commands.Logout;
using Application.Accounts.Commands.RecoveryAccount;
using Application.Accounts.Commands.Register;
using Application.Accounts.Commands.ResetPassword;
using Application.Accounts.Queries;
using Application.CompletedMeeting.Queries.GetCompletedMeetingByUserId;
using Application.Meetings.Commands.EditMeeting;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Api.Controllers
{

    public class AccountController : BaseController
    {
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterCommand request)
        {
            return Ok(await Mediator.Send(request));
        }


        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginCommand request)
        {
            return Ok(await Mediator.Send(request));
        }

        [HttpGet("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout(bool IsMobile = false)
        {
            return Ok(await Mediator.Send(new LogoutCommand { AppUserId = GetUserId() }));
        }

        [HttpPost("/api/account/recovery")]
        [AllowAnonymous]
        public async Task<IActionResult> RecoveryPassword([FromBody] RecoveryAccountCommand request)
        {
            return Ok(await Mediator.Send(request));
        }

        [HttpPost("/api/account/reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand request)
        {
            return Ok(await Mediator.Send(request));
        }


        [HttpPost("/api/account/changepassword")]
        [Authorize(Roles = "Admin,Manager,Participant")]
        [DisplayName("Login olan şifresini değiştirebilir")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }


        [HttpGet("/api/account/usernameemailetc")]
        [Authorize(Roles = "Admin,Manager,Participant")]
        [DisplayName("Login olanın kullanıcı bilgilerini almasını sağlar. Setting page için.")]
        public async Task<IActionResult> GetUserNameEmailEtc()
        {
            int appUserId = GetUserId();
            return Ok(await Mediator.Send(new GetUserNameEmailEtcQuery { AppUserId = appUserId }));

        }

        [HttpPost("/api/account/usernameedit")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit([FromBody] ChangeNameUtcEtcCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }
    }
}
