using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Meetings.Model
{
    public class SelectedMeetingDto:IMapFrom<Meeting>
    {        
        public int Id { get; set; }
        public string? Name { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Meeting, SelectedMeetingDto>()
                .ForMember(a => a.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Length > 25 ? src.Name.Substring(0, 25) + "..." : src.Name));
        }
    }

}
