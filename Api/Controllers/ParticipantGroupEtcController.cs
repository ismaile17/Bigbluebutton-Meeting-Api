using Application.MeetingGroups.Queries.GetAllMeetingGroups;
using Application.ParticipantHomepageAndDetailPages.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class ParticipantGroupEtcController : BaseController
    {
        [HttpGet("/api/participantgroupetc/groupbyuserid")]
        [Authorize(Roles = "Admin,Participant")]
        public async Task<IActionResult> GetParticipantGroupDetailByParticipantId(int meetingGroupId)
        {
            int appUserId = GetUserId();
            return Ok(await Mediator.Send(new GetAllMeetingGroupAndDetailForParticipantQuery { MeetingGroupId = meetingGroupId, AppUserId = appUserId }));
        }

        [HttpGet("/api/participantgroupetc/meetingstoday")]
        //[Authorize(Roles = "Admin,Participant")]
        public async Task<IActionResult> GetAllMeetingsToday()
        {
            //int appUserId = GetUserId();
            int appUserId = 15;
            return Ok(await Mediator.Send(new GetAllMeetingsStartTodayQuery {AppUserId = appUserId}));
        }

        [HttpGet("/api/participantgroupetc/completedmeetingstonowstart")]
        [Authorize(Roles = "Admin,Participant")]
        public async Task<IActionResult> GetAllCompletedStartNow()
        {
            int appUserId = GetUserId();
            return Ok(await Mediator.Send(new GetAllCompletedMeetingsToStartNowQuery { AppUserId = appUserId }));
        }

        [HttpGet("/api/participantgroupetc/getallactivegroupforparticipant")]
        [Authorize(Roles = "Admin,Participant")]
        public async Task<IActionResult> GetAllActiveGroupForParticipant()
        {
            int appUserId = GetUserId();
            return Ok(await Mediator.Send(new GetActiveGroupsForUserQuery { AppUserId = appUserId }));
        }
    }
}
