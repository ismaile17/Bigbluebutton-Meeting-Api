using Application.TaskSystem.Commands.CreateTaskAssignment;
using Application.TaskSystem.Commands.CreateTaskAssignmentSubmissionDetails;
using Application.TaskSystem.Commands.EditTaskSubmission;
using Application.TaskSystem.Commands.SoftDeleteTaskAssignment;
using Application.TaskSystem.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class TaskController : BaseController
    {
        [HttpPost("/api/task/createoredit")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Add([FromBody] CreateOrEditTaskAssignmentCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }

        [HttpGet("/api/task/getalltaskbygroupid")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetTasksByGroupId(int groupId)
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new GetAllTaskAssignmentByGroupIdQuery { AppUserId = appUseRId, GroupId = groupId }));
        }

        [HttpDelete("/api/task/soft-delete/{Id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SoftDeleteTaskAssignment(int Id)
        {
            int appUserId = GetUserId();
            return Ok(await Mediator.Send(new SoftDeleteTaskAssignmentCommand { Id = Id, AppUserId = appUserId }));
        }

        [HttpPost("/api/task/editsubmission")]
        [Authorize(Roles = "Admin,Manager,Participant")]
        public async Task<IActionResult> Add([FromBody] EditTaskSubmissionCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }

        [HttpGet("/api/task/getalltasksubmissionresponted")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAllTaskSubmissionResponted(int taskAssignmentId)
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new GetAllTaskSubmissionRespondedListQuery { AppUserId = appUseRId, TaskAssignmentId = taskAssignmentId }));
        }

        [HttpPost("/api/task/createtaskassignmentsubmissiondetails")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateTaskAssignmentSubmissionDetailsGrade([FromBody] CreateTaskAssignmentSubmissionDetailsCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }

    }
}
