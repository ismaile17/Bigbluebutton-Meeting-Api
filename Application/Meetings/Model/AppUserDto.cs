using Application.Groups.Model;
using Application.ParticipantAccounts.Model;
using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Meetings.Model
{
    public class AppUserDto:IMapFrom<AppUser>
    {
        public int Id { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string NameSurname { get; set; }
        public string ParticipantUserName { get; set; } // Yeni alan
        public bool IsActive { get; set; } // Yeni alan
        public string PhoneNumber { get; set; } // Yeni alan

        //public List<MeetingGroupDto> MeetingGroups { get; set; }
        //public List<ManagerParticipantDto> ManagerParticipants { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<AppUser, AppUserDto>()
                .ForMember(a => a.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(a => a.Phone, opt => opt.MapFrom(s => s.PhoneNumber))
                .ForMember(a => a.Email, opt => opt.MapFrom(s => s.Email))
                .ForMember(a => a.NameSurname, opt => opt.MapFrom(s => s.UserName))
             .ForMember(dest => dest.IsActive, opt => opt.Ignore()) // Bu örnekte, IsActive bilgisi dışarıdan sağlanacak.
                .ForMember(dest => dest.ParticipantUserName, opt => opt.Ignore()) // Bu bilgi de dışarıdan sağlanacak.
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber)); // Bu alan doğrudan eşlenebilir.
                //.ForMember(dest => dest.MeetingGroups, opt => opt.MapFrom(src => src.MeetingGroupUserLists.Select(mgul => mgul.MeetingGroup)));
                //.ForMember(dest => dest.ManagerParticipants, opt => opt.MapFrom(src => src.ManagerParticipants.Select(mgul => mgul.ParticipantId)));





        }
    }
}