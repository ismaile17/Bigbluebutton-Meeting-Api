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
    public class GetUserDetailSettingMeetingByUserIdQuery : IRequest<ResultSingle<UserDetailSettingMeetingDto>>
    {
        public int AppUserId { get; set; }
    }

    public class GetUserDetailSettingByUserIdQueryHandler : IRequestHandler<GetUserDetailSettingMeetingByUserIdQuery, ResultSingle<UserDetailSettingMeetingDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public GetUserDetailSettingByUserIdQueryHandler(UserManager<AppUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ResultSingle<UserDetailSettingMeetingDto>> Handle(GetUserDetailSettingMeetingByUserIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                .Include(u => u.UserDetailSetting)
                .FirstOrDefaultAsync(u => u.Id == request.AppUserId);

            if (user == null)
            {
                return Result.Fail<UserDetailSettingMeetingDto>(null, "User bulunamadı.");
            }

            if (user.UserDetailSetting != null)
            {
                var dto = _mapper.Map<UserDetailSettingMeetingDto>(user.UserDetailSetting);
                return Result.Ok(dto,"Başarılı");
            }
            else
            {
                // Eğer null ise belirlediğiniz sabit değerlerle bir UserDetailSettingDto oluşturun
                var defaultSettingDto = new UserDetailSettingMeetingDto
                {
                    // Sabit değerleri buraya yazın. Örnek olarak:
                    Description = "Default Description",
                    WelcomeMessage = "Welcome to our system!",
                    RecordTF = true,
                    Duration = 60,
                    WebcamsOnlyForModerator = false,
                    BannerText = "This is a default banner text.",
                    LockSettingsDisableCam = true,
                    LockSettingsDisableMic = false,
                    LockSettingsDisablePrivateChat = false,
                    LockSettingsDisablePublicChat = true,
                    LockSettingsHideUserList = false,
                    GuestPolicy = GuestPolicy.ASK_MODERATOR,
                    RecordVisibility = RecordVisibility.ADMIN,
                    AllowModsToEjectCameras = false,
                    AllowRequestsWithoutSession = false,
                    MeetingCameraCap = 25,
                    Logo = "default_logo.png",
                    //AppUser = _mapper.Map<AppUserDto>(user) // AppUser bilgilerini default DTO'ya ekleyin
                };
                return Result.Ok(defaultSettingDto,"Başarılı");
            }
        }
    }
}
