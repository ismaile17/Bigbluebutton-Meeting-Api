using Application.Shared.Mappings;
using AutoMapper;

namespace Application.LandingPage.Models
{
    public class LandingCampaignEmailDto : IMapFrom<Domain.Entities.LandingCampaignEmail>
    {
        public string Email { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Domain.Entities.LandingCampaignEmail, LandingCampaignEmailDto>()
                .ForMember(a => a.Email, opt => opt.MapFrom(s => s.Email))
                ;
        }
    }
}
