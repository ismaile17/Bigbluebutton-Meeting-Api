using Application.FileSystem.Commands;
using Application.FileSystem.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Enum;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileSystemController : BaseController
    {
        [HttpPost("upload")]
        [Authorize(Roles = "Admin,Manager,Participant")]
        public async Task<IActionResult> FileUpload(IFormFile formFile, [FromForm] int pageId, [FromForm] FileSystemPageType pageType)
        {
            if (formFile == null || formFile.Length == 0)
                return BadRequest("No file provided.");

            int appUseRId = GetUserId();

            var command = new CreateAwsFileCommand
            {
                AppUserId = appUseRId,
                FileName = formFile.FileName,
                FileStream = formFile.OpenReadStream(),
                ContentType = formFile.ContentType,
                FileType = Path.GetExtension(formFile.FileName),
                PageId = pageId,
                PageType = pageType
            };

            var result = await Mediator.Send(command);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result);
        }


        [HttpDelete("delete")]
        [Authorize(Roles = "Admin,Manager,Participant")]
        public async Task<IActionResult> DeleteFile([FromQuery] int fileId)
        {
            if (fileId == null)
                return BadRequest("File key is required.");

            int appUseRId = GetUserId();

            var command = new DeleteAwsFileCommand
            {
                FileId = fileId,
                AppUserId = appUseRId
            };

            var result = await Mediator.Send(command);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpGet("download-link")]
        [Authorize(Roles = "Admin,Manager,Participant")]
        public async Task<IActionResult> GetDownloadLink([FromQuery] int fileId)
        {
            int appUseRId = GetUserId();

            var command = new CreateDownloadAwsLinkCommand
            {
                FileId = fileId,
                AppUserId = appUseRId
            };

            var result = await Mediator.Send(command);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPost("upload-multiple")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FileUploadMultiple(List<IFormFile> formFiles, [FromForm] int pageId, [FromForm] FileSystemPageType pageType)
        {
            if (formFiles == null || formFiles.Count == 0)
                return BadRequest("No files provided.");

            int appUseRId = GetUserId();

            var command = new CreateMultipleAwsFileCommand
            {
                AppUserId = appUseRId,
                Files = formFiles,
                PageId = pageId,
                PageType = pageType
            };

            var result = await Mediator.Send(command);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result);
        }


        [HttpGet("getfilelist-byuserid")]
        [Authorize(Roles = "Admin,Manager,Participant")]
        public async Task<IActionResult> GetFileListByUserId(int pageId, FileSystemPageType pageType, int? participantId)
        {
            //int appUseRId = GetUserId();
            int appUseRId = 2;

            return Ok(await Mediator.Send(new GetFileListQuery { AppUserId = appUseRId, PageId = pageId, PageType = pageType, ParticipantIdForTeacher = participantId }));
        }

    }
}
