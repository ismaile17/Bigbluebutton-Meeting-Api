using Application.Shared.Mappings;
using AutoMapper;

namespace Application.LearningDashboards.Model
{
    public class LearningMeetingDto : IMapFrom<Domain.Entities.Learning.LearningMeeting>
    {
        public int Id { get; set; }
        public string? MeetingId { get; set; }
        public string? InternalMeetingId { get; set; }
        public string? Version { get; set; }
        public string? MeetingName { get; set; }
        public bool? IsBreakout { get; set; }
        public int? Duration { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? Finish { get; set; }
        public List<AttendeeDto>? Attendees { get; set; }
        public List<LearningFileDto>? Files { get; set; }
        public List<PollDto>? Polls { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Domain.Entities.Learning.LearningMeeting, LearningMeetingDto>()
                .ForMember(a => a.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(a => a.MeetingId, opt => opt.MapFrom(s => s.MeetingId))
                .ForMember(a => a.InternalMeetingId, opt => opt.MapFrom(s => s.InternalMeetingId))
                .ForMember(a => a.Version, opt => opt.MapFrom(s => s.Version))
                .ForMember(a => a.MeetingName, opt => opt.MapFrom(s => s.MeetingName))
                .ForMember(a => a.IsBreakout, opt => opt.MapFrom(s => s.IsBreakout))
                .ForMember(a => a.Duration, opt => opt.MapFrom(s => s.Duration))
                .ForMember(a => a.Start, opt => opt.MapFrom(s => s.Start))
                .ForMember(a => a.Finish, opt => opt.MapFrom(s => s.Finish))
                .ForMember(a => a.Attendees, opt => opt.MapFrom(s => s.Attendees))
                .ForMember(a => a.Files, opt => opt.MapFrom(s => s.Files))
                .ForMember(a => a.Polls, opt => opt.MapFrom(s => s.Polls));
        }
    }
}
