using Application.Groups.Model;
using Application.MeetingGroups.Model;
using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;
using Shared.Enum;

namespace Application.Meetings.Model
{
    public class MeetingFullDto : IMapFrom<Meeting>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? WelcomeMessage { get; set; }
        public bool? RecordTF { get; set; }
        public string? StartDate { get; set; }
        public string? StartTime { get; set; }
        public string? EndDate { get; set; }
        public string? EndTime { get; set; }
        public int? Duration { get; set; }
        public bool? WebcamsOnlyForModerator { get; set; }
        public string? BannerText { get; set; }
        public bool? LockSettingsDisableCam { get; set; }
        public bool? LockSettingsDisableMic { get; set; }
        public bool? LockSettingsDisablePrivateChat { get; set; }
        public bool? LockSettingsDisablePublicChat { get; set; }
        public bool? LockSettingsHideUserList { get; set; }
        public GuestPolicy? GuestPolicy { get; set; }
        public RecordVisibility? RecordVisibility { get; set; }
        public bool? AllowModsToEjectCameras { get; set; }
        public bool? AllowRequestsWithoutSession { get; set; }
        public int? MeetingCameraCap { get; set; }
        public string? Logo { get; set; }
        public bool? SingleOrRepeated { get; set; }

        // Alt tablolar
        public List<SelectedGroupDto>? MeetingGroups { get; set; }
        public List<AppUserDto>? MeetingModeratorLists { get; set; }
        public List<string>? MeetingGuestEmailLists { get; set; }
        public List<string>? MeetingScheduleDates { get; set; }

        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<Meeting, MeetingFullDto>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                    .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                    .ForMember(dest => dest.WelcomeMessage, opt => opt.MapFrom(src => src.WelcomeMessage))
                    .ForMember(dest => dest.RecordTF, opt => opt.MapFrom(src => src.RecordTF))
                    .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ToString("yyyy-MM-dd")))
                    .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.HasValue ? src.StartTime.Value.ToString(@"hh\:mm") : null))
                    .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.HasValue ? src.EndDate.Value.ToString("yyyy-MM-dd") : null))
                    .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.HasValue ? src.EndTime.Value.ToString(@"hh\:mm") : null))
                    .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration))
                    .ForMember(dest => dest.WebcamsOnlyForModerator, opt => opt.MapFrom(src => src.WebcamsOnlyForModerator))
                    .ForMember(dest => dest.BannerText, opt => opt.MapFrom(src => src.BannerText))
                    .ForMember(dest => dest.LockSettingsDisableCam, opt => opt.MapFrom(src => src.LockSettingsDisableCam))
                    .ForMember(dest => dest.LockSettingsDisableMic, opt => opt.MapFrom(src => src.LockSettingsDisableMic))
                    .ForMember(dest => dest.LockSettingsDisablePrivateChat, opt => opt.MapFrom(src => src.LockSettingsDisablePrivateChat))
                    .ForMember(dest => dest.LockSettingsDisablePublicChat, opt => opt.MapFrom(src => src.LockSettingsDisablePublicChat))
                    .ForMember(dest => dest.LockSettingsHideUserList, opt => opt.MapFrom(src => src.LockSettingsHideUserList))
                    .ForMember(dest => dest.GuestPolicy, opt => opt.MapFrom(src => src.GuestPolicy))
                    .ForMember(dest => dest.RecordVisibility, opt => opt.MapFrom(src => src.RecordVisibility))
                    .ForMember(dest => dest.AllowModsToEjectCameras, opt => opt.MapFrom(src => src.AllowModsToEjectCameras))
                    .ForMember(dest => dest.AllowRequestsWithoutSession, opt => opt.MapFrom(src => src.AllowRequestsWithoutSession))
                    .ForMember(dest => dest.MeetingCameraCap, opt => opt.MapFrom(src => src.MeetingCameraCap))
                    .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => src.Logo))
                    .ForMember(dest => dest.SingleOrRepeated, opt => opt.MapFrom(src => src.SingleOrRepeated))

                    // Alt tablolar
                    .ForMember(dest => dest.MeetingGroups, opt => opt.MapFrom(src => src.MeetingMeetingGroups.Select(mg => mg.MeetingGroup)))
                    .ForMember(dest => dest.MeetingModeratorLists, opt => opt.MapFrom(src => src.MeetingModeratorLists.Select(mm => mm.AppUser)))
                    .ForMember(dest => dest.MeetingGuestEmailLists, opt => opt.MapFrom(src => src.MeetingGuestEmailLists.Select(mgel => mgel.Email)))
                    .ForMember(dest => dest.MeetingScheduleDates, opt => opt.MapFrom(src => src.MeetingScheduleDateLists.Select(msdl => msdl.Date.ToString("yyyy-MM-dd"))));
            }
        }
    }
}
