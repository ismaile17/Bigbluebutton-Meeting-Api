using Application.Shared.Results;
using Application.UserReportAndDetailDatas.Model.UserCalendarPageTotalCount;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.UserReportAndDetailDatas.Queries.UserCalendarPageTotalCount
{
    public class UserCalendarPageTotalCountQuery : IRequest<ResultSingle<UserCalendarPageTotalCountDto>>
    {
        public int AppUserId { get; set; }
    }

    public class UserCalendarPageTotalCountQueryHandler : IRequestHandler<UserCalendarPageTotalCountQuery, ResultSingle<UserCalendarPageTotalCountDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRepository<MeetingGroup> _meetingGroupRepository;
        private readonly UserManager<AppUser> _userManager;

        public UserCalendarPageTotalCountQueryHandler(IMapper mapper, IRepository<MeetingGroup> meetingGroupRepository, UserManager<AppUser> userManager)
        {
            _mapper = mapper;
            _meetingGroupRepository = meetingGroupRepository;
            _userManager = userManager;
        }

        public async Task<ResultSingle<UserCalendarPageTotalCountDto>> Handle(UserCalendarPageTotalCountQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var managerId = request.AppUserId;

                // Tek sorguda PackageEndDate ve TotalActiveUserCount'ı almak
                var userData = await _userManager.Users
                    .Where(u => u.Id == managerId)
                    .Select(u => new
                    {
                        u.TimeZoneId,
                        u.PackageEndDate,
                        TotalActiveUserCount = _userManager.Users
                            .Where(x => x.ManagerParticipants.Any(mp => mp.ManagerId == managerId && mp.IsActive == true) && x.LockoutEnd == null)
                            .Count()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (userData == null)
                {
                    // Kullanıcı bulunamadıysa
                    return ResultSingle<UserCalendarPageTotalCountDto>.Fail(new UserCalendarPageTotalCountDto(), "Kullanıcı bulunamadı.");
                }

                // Toplam Aktif Grup Sayısını almak (farklı repository üzerinden)
                var totalActiveGroupCountTask = _meetingGroupRepository.GetAll
                    .Where(mg => mg.IsActive == 1 && mg.UserId == managerId)
                    .CountAsync(cancellationToken);

                // Aynı anda çalıştırmak için Task.WhenAll kullanıyoruz
                await Task.WhenAll(totalActiveGroupCountTask);

                var totalActiveGroupCount = totalActiveGroupCountTask.Result;

                int totalDayRemaining = 0;

                if (userData.PackageEndDate.HasValue)
                {
                    totalDayRemaining = (userData.PackageEndDate.Value - DateTime.UtcNow).Days;
                }

                // Sonuçları DTO'ya doldur
                var result = new UserCalendarPageTotalCountDto
                {
                    TotalActiveUserCount = userData.TotalActiveUserCount,
                    TotalActiveGroup = totalActiveGroupCount,
                    TotalDay = totalDayRemaining,
                    TimeZoneId = userData.TimeZoneId
                };

                return ResultSingle<UserCalendarPageTotalCountDto>.Ok(result, "Başarılı");
            }
            catch (Exception ex)
            {
                // Hata durumunda varsayılan değerlerle döndür
                var result = new UserCalendarPageTotalCountDto
                {
                    TotalActiveUserCount = 0,
                    TotalActiveGroup = 0,
                    TotalDay = 0
                };
                return ResultSingle<UserCalendarPageTotalCountDto>.Fail(result, $"Bir hata oluştu: {ex.Message}");
            }
        }
    }
}
