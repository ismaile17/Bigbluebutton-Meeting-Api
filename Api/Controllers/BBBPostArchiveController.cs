using Application.PostArchiveForMeetingVideo.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class BBBPostArchiveController : BaseController
    {
        [HttpPost("/api/postarchive/create")]
        public async Task<IActionResult> Add([FromBody] CreatePostArchiveForMeetingVideoCommand request)
        {
            return Ok(await Mediator.Send(request));
        }
    }
}
