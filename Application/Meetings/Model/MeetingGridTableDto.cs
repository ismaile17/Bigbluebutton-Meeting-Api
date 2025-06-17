using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Meetings.Model
{
    public class MeetingGridTableDto : IMapFrom<Meeting>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool? SingleOrRepeated { get; set; }
        public DateTime StartDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public DateTime? EndDate { get; set; }
        public int? TotalMeeting { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Meeting, MeetingGridTableDto>()
                .ForMember(a => a.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Length > 25 ? src.Name.Substring(0, 25) + "..." : src.Name))
                .ForMember(a => a.SingleOrRepeated, opt => opt.MapFrom(s => s.SingleOrRepeated))
                .ForMember(a => a.StartDate, opt => opt.MapFrom(s => s.StartDate))
                .ForMember(a => a.StartTime, opt => opt.MapFrom(s => s.StartTime))
                .ForMember(a => a.EndTime, opt => opt.MapFrom(s => s.EndTime))
                .ForMember(a => a.EndDate, opt => opt.MapFrom(s => s.EndDate))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description.Length > 25 ? src.Description.Substring(0, 25) + "..." : src.Description))
                .ForMember(dest => dest.TotalMeeting, opt => opt.MapFrom(src => src.MeetingScheduleDateLists != null ? src.MeetingScheduleDateLists.Count() : 0));

        }
    }
}
