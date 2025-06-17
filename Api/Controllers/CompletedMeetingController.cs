using Application.CompletedMeeting.Queries.GetCompletedMeetingByUserId;
using Application.CompletedMeetings.Commands;
using Application.CompletedMeetings.Queries.GetCompletedMeetingById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class CompletedMeetingController : BaseController
    {

        [HttpGet("/api/completedmeeting/bymanagerid")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAllByManagerId()
        {
            int appUseRId = GetUserId();

            return Ok(await Mediator.Send(new GetCompletedMeetingByManagerUserIdQuery { AppUserId = appUseRId }));
        }


        [HttpGet("/api/completedmeeting/getcompletedmeetingbyid")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetCompletedMeetingById(int completedMeetingId)
        {
            int appUserId = GetUserId();
            return Ok(await Mediator.Send(new GetCompletedMeetingByIdQuery { CompletedMeetingId = completedMeetingId, AppUserId = appUserId }));
        }

        [HttpDelete("/api/completedmeeting/soft-delete/{Id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SoftDeleteCompletedMeeting(int Id)
        {
            int appUserId = GetUserId();
            return Ok(await Mediator.Send(new SoftDeleteCompletedMeetingCommand { Id = Id, AppUserId = appUserId }));
        }

        [HttpGet("/api/completedmeeting/logincompletedmeeting")]
        public async Task<IActionResult> GetMeetingById(Guid completedMeetingGuId)
        {
            int appUserId = GetUserId();
            return Ok(await Mediator.Send(new GetLoginCompletedMeetingQuery { CompletedMeetingGuid = completedMeetingGuId, AppUserId = appUserId }));
        }
    }
}
