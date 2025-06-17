using Application.Meetings.Model;
using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;
using Shared.Enum;

namespace Application.UserDetailSettings.Model
{
    public class UserDetailSettingMeetingAndInvoiceDto : IMapFrom<UserDetailSetting>
    {
        public int UserId { get; set; }
        public string? Description { get; set; }
        public string? WelcomeMessage { get; set; }
        public bool? RecordTF { get; set; }
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
        public int? MeetingCameraCap { get; set; } //burası eğer başlatılan toplantı 100 kişiden fazla ise 100 limitle onaylanabilir.
        public string? Logo { get; set; }
        public string? InvoiceType { get; set; }
        public string? InvoiceNameSurname { get; set; }
        public string? InvoicePersonalNumber { get; set; }
        public string? InvoiceAddress { get; set; }
        public string? BusinessName { get; set; }
        public string? BusinessNumber { get; set; }
        public string? BusinessCountry { get; set; }
        public string? BusinessCity { get; set; }
        public string? BusinessProvince { get; set; }
        public string? BusinessAddress { get; set; }
        public string? BusinessVD { get; set; }
        //public AppUserDto AppUser { get; set; } // List yerine tek bir instance

        public void Mapping(Profile profile)
        {
            profile.CreateMap<UserDetailSetting, UserDetailSettingMeetingAndInvoiceDto>()
                .ForMember(a => a.UserId, opt => opt.MapFrom(s => s.UserId))
                .ForMember(a => a.Description, opt => opt.MapFrom(s => s.Description))
                .ForMember(a => a.WelcomeMessage, opt => opt.MapFrom(s => s.WelcomeMessage))
                .ForMember(a => a.RecordTF, opt => opt.MapFrom(s => s.RecordTF))
                .ForMember(a => a.Duration, opt => opt.MapFrom(s => s.Duration))
                .ForMember(a => a.WebcamsOnlyForModerator, opt => opt.MapFrom(s => s.WebcamsOnlyForModerator))
                .ForMember(a => a.BannerText, opt => opt.MapFrom(s => s.BannerText))
                .ForMember(a => a.LockSettingsDisableCam, opt => opt.MapFrom(s => s.LockSettingsDisableCam))
                .ForMember(a => a.LockSettingsDisableMic, opt => opt.MapFrom(s => s.LockSettingsDisableMic))
                .ForMember(a => a.LockSettingsDisablePrivateChat, opt => opt.MapFrom(s => s.LockSettingsDisablePrivateChat))
                .ForMember(a => a.LockSettingsDisablePublicChat, opt => opt.MapFrom(s => s.LockSettingsDisablePublicChat))
                .ForMember(a => a.LockSettingsHideUserList, opt => opt.MapFrom(s => s.LockSettingsHideUserList))
                .ForMember(a => a.GuestPolicy, opt => opt.MapFrom(s => s.GuestPolicy))
                .ForMember(a => a.RecordVisibility, opt => opt.MapFrom(s => s.RecordVisibility))
                .ForMember(a => a.AllowModsToEjectCameras, opt => opt.MapFrom(s => s.AllowModsToEjectCameras))
                .ForMember(a => a.AllowRequestsWithoutSession, opt => opt.MapFrom(s => s.AllowRequestsWithoutSession))
                .ForMember(a => a.MeetingCameraCap, opt => opt.MapFrom(s => s.MeetingCameraCap))
                .ForMember(a => a.Logo, opt => opt.MapFrom(s => s.Logo))
                .ForMember(a => a.InvoiceType, opt => opt.MapFrom(s => s.InvoiceType))
                .ForMember(a => a.InvoiceNameSurname, opt => opt.MapFrom(s => s.InvoiceNameSurname))
                .ForMember(a => a.InvoicePersonalNumber, opt => opt.MapFrom(s => s.InvoicePersonalNumber))
                .ForMember(a => a.InvoiceAddress, opt => opt.MapFrom(s => s.InvoiceAddress))
                .ForMember(a => a.BusinessName, opt => opt.MapFrom(s => s.BusinessName))
                .ForMember(a => a.BusinessNumber, opt => opt.MapFrom(s => s.BusinessNumber))
                .ForMember(a => a.BusinessCountry, opt => opt.MapFrom(s => s.BusinessCountry))
                .ForMember(a => a.BusinessCity, opt => opt.MapFrom(s => s.BusinessCity))
                .ForMember(a => a.BusinessProvince, opt => opt.MapFrom(s => s.BusinessProvince))
                .ForMember(a => a.BusinessAddress, opt => opt.MapFrom(s => s.BusinessAddress))
                .ForMember(a => a.BusinessVD, opt => opt.MapFrom(s => s.BusinessVD))
            //.ForMember(dest => dest.AppUser, opt => opt.MapFrom(src => src.User))
            ;
        }
    }
}