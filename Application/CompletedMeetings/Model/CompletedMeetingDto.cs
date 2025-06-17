using Application.Shared.Mappings;
using AutoMapper;
using Shared.Enum;

namespace Application.CompletedMeetings.Model
{
    public class CompletedMeetingDto:IMapFrom<Domain.Entities.CompletedMeeting>
    {
        public int Id { get; set; }
        public string BBBMeetingId { get; set; }
        public string CompletedGuid { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? PublicOrPrivate { get; set; }
        public string? GuestLink { get; set; }
        public string? CreatedTime { get; set; }
        public string? InternalMeetingId { get; set; }
        public bool? SingleOrRepeated { get; set; }
        public ScheduleOrNowMeeting? ScheduleOrNowMeeting { get; set; }




        public void Mapping(Profile profile)
        {
            profile.CreateMap<Domain.Entities.CompletedMeeting, CompletedMeetingDto>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                    .ForMember(dest => dest.BBBMeetingId, opt => opt.MapFrom(src => src.BBBMeetingId))
                    .ForMember(dest => dest.CompletedGuid, opt => opt.MapFrom(src => src.CompletedGuid))
                    .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                    .ForMember(dest => dest.SingleOrRepeated, opt => opt.MapFrom(src => src.SingleOrRepeated))
                    .ForMember(dest => dest.ScheduleOrNowMeeting, opt => opt.MapFrom(src => src.ScheduleOrNowMeeting))
                    .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => src.CreatedTime)) // Hem tarih hem saat bilgisiyle.
                    .ForMember(dest => dest.GuestLink, opt => opt.MapFrom(src =>
                        src.CreatedTime.Date >= DateTime.UtcNow.Date.AddDays(-2)
                            ? src.GuestLink
                            : null))
                    .ForMember(dest => dest.InternalMeetingId, opt => opt.MapFrom(src => src.InternalMeetingId));
        }


    }


}
