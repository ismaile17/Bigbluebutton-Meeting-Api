using Application.Tickets.Commands;
using Application.Tickets.Queris;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class TicketController : BaseController
    {
        [HttpPost("/api/ticket/createticket")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Add([FromBody] CreateTicketCommand request)
        {

            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }


        [HttpPost("/api/ticket/createticketmessage")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Add([FromBody] CreateTicketMessageCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }

        [HttpPost("/api/ticket/createcategoryorpriorityorclosed")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Add([FromBody] CreateCategoryOrPriorityOrTicketChangeCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }


        [HttpGet("/api/ticket/getallticketbyuserid")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAllTicket()
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new GetAllTicketQuery { AppUserId = appUseRId }));
        }

        [HttpGet("/api/ticket/getonlyticket")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetOnlyTicket()
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new GetAllOnlyTicketsQuery { AppUserId = appUseRId }));
        }

        [HttpGet("/api/ticket/getonlyticketandmessagebyuserid")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetOnlyTicketAndMessage(int ticketId)
        {
            int appUserId = GetUserId();
            return Ok(await Mediator.Send(new GetOnlyTicketAndMessageQuery { TicketId = ticketId, AppUserId = appUserId }));
        }

        [HttpGet("/api/ticket/getallopenadmin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOpenAdmin()
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new GetAllOpenTicketsQueryAdmin { AppUserId = appUseRId }));
        }

        [HttpGet("/api/ticket/getallcloseadmin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllCloseAdmin()
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new GetAllCloseTicketsQueryAdmin { AppUserId = appUseRId }));
        }
    }
}
