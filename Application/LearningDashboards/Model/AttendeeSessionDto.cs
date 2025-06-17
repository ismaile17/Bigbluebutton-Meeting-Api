using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities.Learning;
using System;

namespace Application.LearningDashboards.Model
{
    public class AttendeeSessionDto : IMapFrom<Domain.Entities.Learning.AttendeeSession>
    {
        public int Id { get; set; }
        public int? AttendeeId { get; set; }
        public DateTime? JoinTime { get; set; }
        public DateTime? LeaveTime { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Domain.Entities.Learning.AttendeeSession, AttendeeSessionDto>()
                .ForMember(a => a.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(a => a.AttendeeId, opt => opt.MapFrom(s => s.AttendeeId))
                .ForMember(a => a.JoinTime, opt => opt.MapFrom(s => s.JoinTime))
                .ForMember(a => a.LeaveTime, opt => opt.MapFrom(s => s.LeaveTime));
        }
    }
}
