using Application.Shared.Results;
using Application.UserReportAndDetailDatas.Model.UserCalendarPageTotalCount;
using Application.UserReportAndDetailDatas.Model.UserMaxLimit;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Telegram.Bot.Types;

namespace Application.UserReportAndDetailDatas.Queries.UserCalendarPageTotalCount
{
    public class UserMaxLimitQuery : IRequest<ResultSingle<UserMaxLimitDto>>
    {
        public int AppUserId { get; set; }
    }

    public class UserMaxLimitQueryHandler : IRequestHandler<UserMaxLimitQuery, ResultSingle<UserMaxLimitDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRepository<MeetingGroup> _meetingGroupRepository;
        private readonly IRepository<Package> _packageRepository;
        private readonly UserManager<AppUser> _userManager;

        public UserMaxLimitQueryHandler(
            IMapper mapper,
            IRepository<MeetingGroup> meetingGroupRepository,
            IRepository<Package> packageRepository,
            UserManager<AppUser> userManager)
        {
            _mapper = mapper;
            _meetingGroupRepository = meetingGroupRepository;
            _packageRepository = packageRepository;
            _userManager = userManager;
        }

        public async Task<ResultSingle<UserMaxLimitDto>> Handle(UserMaxLimitQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = request.AppUserId;

                // Retrieve user data and package information in one query
                var userWithPackage = await _userManager.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new
                    {
                        u.PackageId,
                        u.PackageEndDate,
                        TotalActiveUserCount = _userManager.Users
                            .Where(x => x.ManagerParticipants.Any(mp => mp.ManagerId == userId && mp.IsActive == true) && x.LockoutEnd == null)
                            .Count()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (userWithPackage == null)
                {
                    return ResultSingle<UserMaxLimitDto>.Fail(new UserMaxLimitDto(), "Kullanıcı bulunamadı.");
                }

                var package = await _packageRepository
                    .GetMany(u => u.Id == userWithPackage.PackageId)
                    .FirstOrDefaultAsync();

                var packageMaxGroup = package?.MaxGroup ?? 0;
                var packageMaxGroupUser = package?.MaxGroupUser ?? 0;


                var totalActiveGroupCount = await _meetingGroupRepository.GetAll
                    .Where(mg => mg.IsActive == 1 && mg.UserId == userId)
                    .CountAsync(cancellationToken);

                var totalActiveParticipantCount = await _userManager.Users
                    .Where(u => u.ManagerParticipants.Any(mp => mp.ManagerId == userId && mp.IsActive == true))
                    .CountAsync(cancellationToken);

                int maxGroupWithBuffer = packageMaxGroup + 5;
                int maxParticipantWithBuffer = packageMaxGroupUser + 5;

                // Calculate remaining limits
                int remainingGroups = Math.Max(maxGroupWithBuffer - totalActiveGroupCount, 0);
                int remainingParticipants = Math.Max(maxParticipantWithBuffer - totalActiveParticipantCount, 0);

                // Calculate remaining days
                int totalDayRemaining = userWithPackage.PackageEndDate.HasValue
                    ? (userWithPackage.PackageEndDate.Value - DateTime.UtcNow).Days
                    : 0;

                // Populate DTO
                var result = new UserMaxLimitDto
                {
                    MaxGroup = remainingGroups,
                    MaxParticipant = remainingParticipants,
                    TotalDay = totalDayRemaining,
                    MaxMeeting = 1
                };

                return ResultSingle<UserMaxLimitDto>.Ok(result, "Başarılı");
            }
            catch (Exception ex)
            {
                var result = new UserMaxLimitDto
                {
                    MaxGroup = 0,
                    MaxParticipant = 0,
                    TotalDay = 0,
                    MaxMeeting = 0
                };
                return ResultSingle<UserMaxLimitDto>.Fail(result, $"Bir hata oluştu: {ex.Message}");
            }
        }

    }
}
