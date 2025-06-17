using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.LearningDashboards.Model
{
    public class LearningDashboardDto : IMapFrom<LearningDashboard>
    {
        public string Id { get; set; }
        public string? MeetingId { get; set; }
        public string? InternalMeetingId { get; set; }
        public string? Version { get; set; }
        public string? Data { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<LearningDashboard, LearningDashboardDto>()
                .ForMember(a => a.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(a => a.MeetingId, opt => opt.MapFrom(s => s.MeetingId))
                .ForMember(a => a.InternalMeetingId, opt => opt.MapFrom(s => s.InternalMeetingId))
                .ForMember(a => a.Version, opt => opt.MapFrom(s => s.Version))
                .ForMember(a => a.Data, opt => opt.MapFrom(s => s.Data))
                ;
        }
    }
}
