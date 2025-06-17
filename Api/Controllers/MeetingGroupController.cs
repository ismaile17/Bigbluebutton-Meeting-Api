using Application.Groups.Commands.CreateMeetingGroup;
using Application.Groups.Commands.EditGroup;
using Application.Groups.Queries.GetGroupById;
using Application.Groups.Queries.GetMeetingGroupByUserId;
using Application.MeetingGroups.Commands.SoftDeleteMeetingGroup;
using Application.MeetingGroups.Queries.GetAllMeetingGroups;
using Application.MeetingGroups.Queries.GetSelectedGroupByUserId;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class MeetingGroupController : BaseController
    {
        [HttpGet("/api/group/groupbyuserid")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetMeetingGroupUserListByUser()
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new GetMeetingGroupByUserIdQuery { AppUserId = appUseRId }));
        }

        [HttpPost("/api/group/creategroup")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Add([FromBody] CreateMeetingGroupCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }

        [HttpGet("/api/group/getallgroup")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAllGroup()
        {
            var userId = GetUserId();
            return Ok(await Mediator.Send(new GetAllMeetingGroupQuery()));
            //KONTROL EDİLECEK
        }

        [HttpGet("/api/group/getgroupbyid")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetGroupById(int groupId)
        {
            int appUserId = GetUserId();
            return Ok(await Mediator.Send(new GetMeetingGroupByIdQuery { GroupId = groupId, AppUserId = appUserId }));
        }

        [HttpPost("/api/group/editgroup")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> EditGroup([FromBody] EditMeetingGroupCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }

        [HttpGet("/api/group/selectedgroupbyuserid")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetSelectedGroupByUserId()
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new GetSelectedGroupByUserIdQuery { AppUserId = appUseRId }));
        }

        [HttpDelete("/api/group/soft-delete/{Id}")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SoftDeleteMeetingGroup(int Id)
        {
            int appUserId = GetUserId();
            return Ok(await Mediator.Send(new SoftDeleteMeetingGroupCommand { Id = Id, AppUserId = appUserId }));
        }
    }
}
