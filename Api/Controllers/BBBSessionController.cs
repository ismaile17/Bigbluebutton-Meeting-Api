using Application.BBBSessions.Commands.CreateBBBSession;
using Application.BBBSessions.Commands.CreateNowMeeting;
using Application.BBBSessions.Commands.LoginBBBSession;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class BBBSessionController : BaseController
    {
        [HttpPost("/api/session/create")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Add([FromBody] CreateBBBSessionCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }

        [HttpPost("/api/session/createnowmeeting")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Add([FromBody] CreateBBBSessionNowMeetingCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }


        [HttpPost("/api/session/login")]
        public async Task<IActionResult> Add([FromBody] LoginBBBSessionCommand request)
        {
            request.MeetingType = 1;
            request.MeetingEntryType = 1;
            request.GuestFullName = "selam";
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }
    }
}
