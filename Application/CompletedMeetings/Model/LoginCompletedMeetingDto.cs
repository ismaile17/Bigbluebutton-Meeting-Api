using Application.Shared.Mappings;
using AutoMapper;

namespace Application.CompletedMeetings.Model
{
    public class LoginCompletedMeetingDto : IMapFrom<Domain.Entities.CompletedMeeting>
    {
        public Guid CompletedGuid { get; set; }
        public string? GuestPolicy { get; set; }
        public int? CreateStartOrFinish { get; set; }



        public void Mapping(Profile profile)
        {
            profile.CreateMap<Domain.Entities.CompletedMeeting, LoginCompletedMeetingDto>()
                    .ForMember(dest => dest.CompletedGuid, opt => opt.MapFrom(src => src.CompletedGuid))
                    .ForMember(dest => dest.GuestPolicy, opt => opt.MapFrom(src => src.GuestPolicy))
                    .ForMember(dest => dest.CreateStartOrFinish, opt => opt.MapFrom(src => src.CreateStartOrFinish))
                    ;
        }


    }


}
