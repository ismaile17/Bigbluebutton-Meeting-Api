using Application.UserDetailSettings.Commands;
using Application.UserDetailSettings.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class UserDetailSettingController:BaseController
    {
        [HttpPost("/api/userdetailsetting/createoredit")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Add([FromBody] CreateUserDetailSettingCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }

        [HttpGet("/api/userdetailsetting/getbyuserid")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetUserDetailSettingByUserId()
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new GetUserDetailSettingMeetingByUserIdQuery { AppUserId = appUseRId }));
        }

        [HttpGet("/api/userdetailsetting/invoiceandmeetinggetbyuserid")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetUserDetailSettingInvoiceAndMeetingByUserId()
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new GetUserDetailSettingMeetingAndInvoiceByUserIdQuery { AppUserId = appUseRId }));
        }
    }
}