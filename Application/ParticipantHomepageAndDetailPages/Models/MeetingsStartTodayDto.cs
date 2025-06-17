using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.ParticipantHomepageAndDetailPages.Models
{
    public class MeetingsStartTodayDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public List<MeetingScheduleDateListDto> MeetingScheduleDateLists { get; set; }
    }

    public class MeetingScheduleDateListDto
    {
        public DateOnly Date { get; set; }
    }
}
