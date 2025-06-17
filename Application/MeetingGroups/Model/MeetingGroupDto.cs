using Application.Meetings.Model;
using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Groups.Model
{
    public class MeetingGroupDto : IMapFrom<MeetingGroup>
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? SpecialDescription { get; set; }
        public string? Image { get; set; }
        public int MemberCount { get; set; } // Yeni özellik

        public List<AppUserDto> AppUsers { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<MeetingGroup, MeetingGroupDto>()
                .ForMember(a => a.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(a => a.Name, opt => opt.MapFrom(s => s.Name))
                .ForMember(a => a.Description, opt => opt.MapFrom(s => s.Description))
                .ForMember(a => a.SpecialDescription, opt => opt.MapFrom(s => s.SpecialDescription))
                            .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.MeetingGroupUserLists.Count))

                .ForMember(a => a.Image, opt => opt.MapFrom(s => s.Image));

                //.ForMember(a => a.AppUsers, opt => opt.MapFrom(s => s.MeetingGroupUserLists.Select(a => a.AppUser).ToList()));


        }
    }
}
