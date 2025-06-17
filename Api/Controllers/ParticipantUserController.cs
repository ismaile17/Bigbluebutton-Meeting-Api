using Application.ParticipantAccounts.Commands.ParticipantRegister;
using Application.Participants.Commands.EditParticipant;
using Application.ParticipantUserAccounts.Commands.ParticipantUserLogin;
using Application.ParticipantUserAccounts.Queries.GetAllParticipantUserByManagerIdQuery;
using Application.ParticipantUserAccounts.Queries.GetParticipantUserByParticipantId;
using Application.ParticipantUserAccounts.Queries.GetSelectedGroupByUserId;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class ParticipantUserController : BaseController
    {

        [HttpPost("/api/participant/participantregister")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] ParticipantUserRegisterCommand request)
        {
            request.ManagerId = GetUserId();
            return Ok(await Mediator.Send(request));
        }

        [HttpPost("/api/participant/participantlogin")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] ParticipantUserLoginCommand request)
        {
            return Ok(await Mediator.Send(request));
        }


        [HttpGet("/api/participant/getallparticipantbymanagerid")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAllParticipantByManagerId()
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new GetAllParticipantUserByManagerIdQuery { AppUserId = appUseRId }));
        }

        [HttpGet("/api/participant/getselectedparticipantbyuserid")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetSelectedParticipantByUserId()
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new GetSelectedParticipantByUserIdQuery { AppUserId = appUseRId }));
        }

        [HttpGet("/api/participant/getparticipantbyparticipantid")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetParticipantByParticipantId(int participantId)
        {
            int appUserId = GetUserId();
            return Ok(await Mediator.Send(new GetParticipantUserByParticipantIdQuery { ParticipantId = participantId, AppUserId = appUserId }));
        }

        [HttpPost("/api/participant/editparticipant")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> EditParticipant([FromBody] EditParticipantCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }

        [HttpGet("/api/participant/timezones")]
        //[Authorize(Roles = "Admin,Manager,Participant")]
        public IActionResult GetTimeZones()
        {
            var timeZones = TimeZoneInfo.GetSystemTimeZones()
                .Select(tz => new
                {
                    Id = tz.Id, // Veritabanına kaydedilecek benzersiz kimlik
                    DisplayName = tz.DisplayName, // Kullanıcıya gösterilecek ad
                })
                .ToList();

            return Ok(timeZones);
        }
    }
}
