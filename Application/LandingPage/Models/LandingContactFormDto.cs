using Application.Shared.Mappings;
using AutoMapper;

namespace Application.LandingPage.Models
{
    public class LandingContactFormDto : IMapFrom<Domain.Entities.LandingContactForm>
    {
        public string Message { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Domain.Entities.LandingContactForm, LandingContactFormDto>()
                .ForMember(a => a.Message, opt => opt.MapFrom(s => s.Message))
                ;
        }
    }
}
