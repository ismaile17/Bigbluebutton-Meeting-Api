using Application.LandingPage.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class LandingPageController : BaseController
    {

        [HttpPost("/api/landing/createlandingcontact")]
        [AllowAnonymous]
        public async Task<IActionResult> AddContact([FromBody] CreateLandingContactFormCommand request)
        {
            return Ok(await Mediator.Send(request));
        }


        [HttpPost("/api/landing/createcampaignemail")]
        [AllowAnonymous]
        public async Task<IActionResult> AddMail([FromBody] CreateLandingCampaignEmailCommand request)
        {
            return Ok(await Mediator.Send(request));
        }

    }
}
