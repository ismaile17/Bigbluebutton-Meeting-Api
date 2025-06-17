using Application.Groups.Model;
using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.MeetingGroups.Model
{
    public class SelectedGroupDto:IMapFrom<MeetingGroup>
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<MeetingGroup, SelectedGroupDto>()
                .ForMember(a => a.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Length > 25 ? src.Name.Substring(0, 25) + "..." : src.Name));
        }
    }
}
