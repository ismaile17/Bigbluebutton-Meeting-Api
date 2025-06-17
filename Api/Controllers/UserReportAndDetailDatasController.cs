using Application.UserReportAndDetailDatas.Queries.UserCalendarAndHomePage;
using Application.UserReportAndDetailDatas.Queries.UserCalendarPageTotalCount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class UserReportAndDetailDatasController : BaseController
    {
        [HttpGet("/api/userreportanddetail/calendarpage")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetCalendarPageByUserId()
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new GetUserCalendarPageQuery { AppUserId = appUseRId }));
        }


        [HttpGet("/api/userreportanddetail/totalcount")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetTotalCountByUserId()
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new UserCalendarPageTotalCountQuery { AppUserId = appUseRId }));
        }

        [HttpGet("/api/userreportanddetail/maxlimit")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetMaxLimitByUserId()
        {
            int appUseRId = GetUserId();
            return Ok(await Mediator.Send(new UserMaxLimitQuery { AppUserId = appUseRId }));
        }

    }
}
