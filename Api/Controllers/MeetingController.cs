using Application.Meetings.Commands.CreateMeeting;
using Application.Meetings.Commands.DeleteMeeting;
using Application.Meetings.Commands.EditMeeting;
using Application.Meetings.Queries.GetMeetingById;
using Application.Meetings.Queries.GetMeetingByParticipantUserId;
using Application.Meetings.Queries.GetMeetingByUserId;
using Application.Meetings.Queries.GetMeetingGridTableByUserId;
using Application.Meetings.Queries.GetSelectedMeetingByUserId;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{

    public class MeetingController : BaseController
    {

        [HttpGet("/api/meeting")]
        //[Authorize(Roles = "Admin,Manager")] 
        public async Task<IActionResult> GetMeetingByUser()
        {
            int appUseRId = GetUserId();

            return Ok(await Mediator.Send(new GetMeetingByUserIdQuery { AppUserId = appUseRId }));
        }

        [HttpPost("/api/meeting/create")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Add([FromBody] CreateMeetingCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }

        [HttpDelete("/api/meeting/hard-delete/{Id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int Id)
        {
            int appUserId = GetUserId();
            return Ok(await Mediator.Send(new DeleteMeetingCommand { Id = Id, AppUserId = appUserId}));
        }

        [HttpDelete("/api/meeting/soft-delete/{Id}")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SoftDeleteMeeting(int Id)
        {
            int appUserId = GetUserId();
            return Ok(await Mediator.Send(new SoftDeleteMeetingCommand { Id = Id, AppUserId = appUserId }));
        }

        [HttpPost("/api/meeting/edit")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit([FromBody] EditMeetingCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }

        [HttpGet("/api/meeting/getmeetingbyid")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetMeetingById(int meetingId)
        {
            int appUserId = GetUserId();
            return Ok(await Mediator.Send(new GetMeetingByIdQuery { MeetingId = meetingId, AppUserId = appUserId }));
        }

        [HttpGet("/api/meeting/getmeetingbyuserid")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetMeetingByUserId()
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new GetMeetingByUserIdQuery { AppUserId = appUseRId }));
        }

        [HttpGet("/api/meeting/getmeetingforparticipant")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetMeetingForParticipantId()
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new GetMeetingByParticipantUserIdQuery { AppUserId = appUseRId }));
        }

        [HttpGet("/api/meeting/selectedmeetingbyuserid")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetSelectedMeetingByUserId()
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new GetSelectedMeetingByUserIdQuery { AppUserId = appUseRId }));
        }

        [HttpGet("/api/meeting/gridtablemeetingbyuserid")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetMeetingGridTableByUserId()
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new GetMeetingGridTableByUserIdQuery { AppUserId = appUseRId }));
        }

    }
}