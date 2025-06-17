using Application.Packages.Commands.CreatePackage;
using Application.Packages.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class PackageController : BaseController
    {
        [HttpPost("/api/package/create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromBody] CreatePackageCommand request)
        {
            request.AppUserId = GetUserId();
            return Ok(await Mediator.Send(request));
        }

        [HttpGet("/api/package/package")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAllPackage()
        {
            return Ok(await Mediator.Send(new GetAllPackageQuery()));
        }

        [HttpGet("/api/package/getpackagebyid")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetPackageById(int packageId)
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new GetPackageByIdQuery { PackageId = packageId, AppUserId = appUseRId }));
        }

        [HttpGet("/api/package/getuserpackagebyuserid")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetUserPackage()
        {
            int appUseRId = GetUserId();

            return Ok(await Mediator.Send(new GetUserPackageQuery { AppUserId = appUseRId }));
        }
    }
}
