using Application.Meetings.Model;
using Application.Shared.Results;
using Application.UserDetailSettings.Model;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Enum;

namespace Application.UserDetailSettings.Queries
{
    public class GetUserDetailSettingMeetingAndInvoiceByUserIdQuery : IRequest<ResultSingle<UserDetailSettingMeetingAndInvoiceDto>>
    {
        public int AppUserId { get; set; }
    }

    public class GetUserDetailSettingMeetingAndInvoiceByUserIdQueryHandler : IRequestHandler<GetUserDetailSettingMeetingAndInvoiceByUserIdQuery, ResultSingle<UserDetailSettingMeetingAndInvoiceDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public GetUserDetailSettingMeetingAndInvoiceByUserIdQueryHandler(UserManager<AppUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ResultSingle<UserDetailSettingMeetingAndInvoiceDto>> Handle(GetUserDetailSettingMeetingAndInvoiceByUserIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                .Include(u => u.UserDetailSetting)
                .FirstOrDefaultAsync(u => u.Id == request.AppUserId);

            if (user == null)
            {
                return Result.Fail<UserDetailSettingMeetingAndInvoiceDto>(null, "User bulunamadı.");
            }

            if (user.UserDetailSetting != null)
            {
                var dto = _mapper.Map<UserDetailSettingMeetingAndInvoiceDto>(user.UserDetailSetting);
                return Result.Ok(dto, "Başarılı");
            }
            else
            {
                // Eğer null ise belirlediğiniz sabit değerlerle bir UserDetailSettingDto oluşturun
                var defaultSettingDto = new UserDetailSettingMeetingAndInvoiceDto
                {
                    // Sabit değerleri buraya yazın. Örnek olarak:
                    Description = "Toplantı Saati.Com",
                    WelcomeMessage = "Hoş Geldiniz.",
                    RecordTF = true,
                    Duration = 60,
                    WebcamsOnlyForModerator = false,
                    BannerText = "Toplantı Saati.Com",
                    LockSettingsDisableCam = true,
                    LockSettingsDisableMic = false,
                    LockSettingsDisablePrivateChat = false,
                    LockSettingsDisablePublicChat = true,
                    LockSettingsHideUserList = false,
                    GuestPolicy = GuestPolicy.ASK_MODERATOR,
                    RecordVisibility = RecordVisibility.ADMIN,
                    AllowModsToEjectCameras = false,
                    Logo = "default_logo.png",
                    //AppUser = _mapper.Map<AppUserDto>(user) // AppUser bilgilerini default DTO'ya ekleyin
                };
                return Result.Ok(defaultSettingDto, "Başarılı");
            }
        }
    }
}
