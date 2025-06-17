using Application.GeneralAdmin.GAServer.Commands.CreateGAServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class GeneralAdminController : BaseController
    {
        [HttpPost("/api/generaladmin/createbbbserver")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromBody] CreateBBBServerCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }
    }
}