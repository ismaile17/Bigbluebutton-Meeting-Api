using Application.Accounts.Commands.Login;
using Application.Packages.Model;
using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Accounts.Model
{
    public class UserDto : IMapFrom<AppUser>
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public List<RoleDto> Roles { get; set; }


        public void Mapping(Profile profile)
        {
            profile.CreateMap<AppUser, UserDto>()
                .ForMember(a => a.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(a => a.FullName, opt => opt.MapFrom(s => s.FullName))
                .ForMember(a => a.Email, opt => opt.MapFrom(s => s.Email))
                ;            
        }
    }
}