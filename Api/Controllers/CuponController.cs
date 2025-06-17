using Application.Cupon.Commands;
using Application.Cupon.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class CuponController : BaseController
    {
        [HttpPost("/api/cupon/create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromBody] CreateCuponCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }

        [HttpGet("/api/cupon/check")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Check(string code, int packageId)
        {
            return Ok(await Mediator.Send(new GetCuponChechByCuponQuery { Code = code, PackageId = packageId }));
        }

        [HttpPost("/api/cupon/rfidsystem")]
        public async Task<IActionResult> Add([FromBody] CreateRfidReaderCommand request)
        {
            return Ok(await Mediator.Send(request));
        }
    }
}