using Application.Shared.Results;
using Application.UserDetailSettings.Model;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Enum;

namespace Application.UserDetailSettings.Commands
{
    public class CreateUserDetailSettingCommand : IRequest<ResultSingle<UserDetailSettingMeetingAndInvoiceDto>>
    {
        public int AppUserId { get; set; }
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
        public bool? AllowModsToEjectCameras { get; set; }
        public bool? AllowRequestsWithoutSession { get; set; }
        public int? MeetingCameraCap { get; set; }
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
        public RecordVisibility? RecordVisibility { get; set; }


    }

    public class CreateUserDetailSettingCommandHandler : IRequestHandler<CreateUserDetailSettingCommand, ResultSingle<UserDetailSettingMeetingAndInvoiceDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public CreateUserDetailSettingCommandHandler(UserManager<AppUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ResultSingle<UserDetailSettingMeetingAndInvoiceDto>> Handle(CreateUserDetailSettingCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
            .Include(u => u.UserDetailSetting)
            .FirstOrDefaultAsync(u => u.Id == request.AppUserId);

            if (user == null)
            {
                return Result.Fail<UserDetailSettingMeetingAndInvoiceDto>(null, $"Kullanıcı bulunamadı");
            }

            var userDetailSetting = user.UserDetailSetting ?? new UserDetailSetting();
            userDetailSetting.Description = request.Description;
            userDetailSetting.WelcomeMessage = request.WelcomeMessage;
            userDetailSetting.RecordTF = request.RecordTF;
            userDetailSetting.Duration = request.Duration;
            userDetailSetting.WebcamsOnlyForModerator = request.WebcamsOnlyForModerator;
            userDetailSetting.BannerText = request.BannerText;
            userDetailSetting.LockSettingsDisableCam = request.LockSettingsDisableCam;
            userDetailSetting.LockSettingsDisableMic = request.LockSettingsDisableMic;
            userDetailSetting.LockSettingsDisablePrivateChat = request.LockSettingsDisablePrivateChat;
            userDetailSetting.LockSettingsDisablePublicChat = request.LockSettingsDisablePublicChat;
            userDetailSetting.LockSettingsHideUserList = request.LockSettingsHideUserList;
            userDetailSetting.GuestPolicy = request.GuestPolicy;
            userDetailSetting.AllowModsToEjectCameras = request.AllowModsToEjectCameras;
            userDetailSetting.AllowRequestsWithoutSession = request.AllowRequestsWithoutSession;
            userDetailSetting.MeetingCameraCap = request.MeetingCameraCap;
            userDetailSetting.Logo = request.Logo;
            userDetailSetting.InvoiceType = request.InvoiceType;
            userDetailSetting.InvoiceNameSurname = request.InvoiceNameSurname;
            userDetailSetting.InvoicePersonalNumber = request.InvoicePersonalNumber;
            userDetailSetting.InvoiceAddress = request.InvoiceAddress;
            userDetailSetting.BusinessName = request.BusinessName;
            userDetailSetting.BusinessNumber = request.BusinessNumber;
            userDetailSetting.BusinessCountry = request.BusinessCountry;
            userDetailSetting.BusinessCity = request.BusinessCity;
            userDetailSetting.BusinessProvince = request.BusinessProvince;
            userDetailSetting.BusinessAddress = request.BusinessAddress;
            userDetailSetting.BusinessVD = request.BusinessVD;
            userDetailSetting.RecordVisibility = request.RecordVisibility;


            if (user.UserDetailSetting == null)
            {
                user.UserDetailSetting = userDetailSetting;
            }


            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return Result.Fail<UserDetailSettingMeetingAndInvoiceDto>(null, $"Kullanıcı detay ayarı güncellenemedi.");

            }

            var userDetailSettingDto = _mapper.Map<UserDetailSettingMeetingAndInvoiceDto>(userDetailSetting);
            return Result.Ok(userDetailSettingDto, "Kullanıcı detay ayarı başarıyla güncellendi.");
        }
    }

}
