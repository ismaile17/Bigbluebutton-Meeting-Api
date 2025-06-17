using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities.Learning;

namespace Application.LearningDashboards.Model
{
    public class LearningFileDto : IMapFrom<Domain.Entities.Learning.LearningFile>
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }
        public string? FileName { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Domain.Entities.Learning.LearningFile, LearningFileDto>()
                .ForMember(a => a.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(a => a.MeetingId, opt => opt.MapFrom(s => s.MeetingId))
                .ForMember(a => a.FileName, opt => opt.MapFrom(s => s.FileName));
        }
    }
}
