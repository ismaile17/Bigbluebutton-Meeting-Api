using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Accounts.Model
{
    public class UserNameMailEtcInfoDto : IMapFrom<AppUser>
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? TimeZoneId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<AppUser, UserNameMailEtcInfoDto>()
                .ForMember(a => a.FullName, opt => opt.MapFrom(s => s.FullName))
                .ForMember(a => a.Email, opt => opt.MapFrom(s => s.Email))
                .ForMember(a => a.PhoneNumber, opt => opt.MapFrom(s => s.PhoneNumber))
                .ForMember(a => a.TimeZoneId, opt => opt.MapFrom(s => s.TimeZoneId))
                ;
        }
    }
}