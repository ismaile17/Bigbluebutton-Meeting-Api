using Application.ParticipantHomepageAndDetailPages.Models;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.ParticipantHomepageAndDetailPages.Queries
{
    public class GetAllMeetingsStartTodayQuery : IRequest<Result<MeetingsStartTodayDto>>
    {
        public int AppUserId { get; set; }
    }

    public class GetAllMeetingsStartTodayQueryHandler : IRequestHandler<GetAllMeetingsStartTodayQuery, Result<MeetingsStartTodayDto>>
    {
        private readonly IRepository<Meeting> _meetingRepository;
        private readonly IRepository<ManagerParticipant> _managerParticipantRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public GetAllMeetingsStartTodayQueryHandler(
            IRepository<Meeting> meetingRepository,
            IRepository<ManagerParticipant> managerParticipantRepository,
            IMapper mapper,
            UserManager<AppUser> userManager)
        {
            _meetingRepository = meetingRepository;
            _managerParticipantRepository = managerParticipantRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<Result<MeetingsStartTodayDto>> Handle(
            GetAllMeetingsStartTodayQuery request,
            CancellationToken cancellationToken)
        {
            // 1) Kullanıcının TimeZone bilgisini çek
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == request.AppUserId, cancellationToken);

            var userTimeZoneId = user?.TimeZoneId ?? "Turkey Standard Time";
            var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneId);

            // 2) Kullanıcının "bugün" ve "1 ay sonrası" arasını local bazda hesapla
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, userTimeZone);
            var localToday = new DateOnly(localNow.Year, localNow.Month, localNow.Day);

            var localNextMonth = localToday.AddMonths(1);
            var endOfNextMonth = new DateOnly(
                localNextMonth.Year,
                localNextMonth.Month,
                DateTime.DaysInMonth(localNextMonth.Year, localNextMonth.Month));

            // localToday 00:00 => UTC
            var localTodayStart = new DateTime(
                localToday.Year,
                localToday.Month,
                localToday.Day,
                0, 0, 0,
                DateTimeKind.Unspecified);
            var todayStartUtc = TimeZoneInfo.ConvertTimeToUtc(localTodayStart, userTimeZone);

            // endOfNextMonth 23:59:59 => UTC
            var localEndOfNextMonth = new DateTime(
                endOfNextMonth.Year,
                endOfNextMonth.Month,
                endOfNextMonth.Day,
                23, 59, 59,
                DateTimeKind.Unspecified);
            var endOfNextMonthUtc = TimeZoneInfo.ConvertTimeToUtc(localEndOfNextMonth, userTimeZone);

            // 3) ManagerParticipants sorgusu
            var activeManagerParticipants = await _managerParticipantRepository.GetMany(mp =>
                mp.ParticipantId == request.AppUserId &&
                mp.IsActive == true &&
                (
                    mp.ExpiryDateIsActive != true ||
                    (mp.ExpiryDate != null && mp.ExpiryDate >= DateTime.UtcNow)
                ))
                .ToListAsync(cancellationToken);

            var managerIds = activeManagerParticipants
                .Select(mp => mp.ManagerId)
                .Distinct()
                .ToList();

            // 4) Veritabanından ham veriyi çek (UTC veriler)
            var rawMeetings = await _meetingRepository
                .GetMany(m =>
                    m.CreatedBy.HasValue &&
                    managerIds.Contains(m.CreatedBy.Value) &&
                    m.MeetingScheduleDateLists.Any(d =>
                        d.StartDateTimeUtc >= todayStartUtc &&
                        d.StartDateTimeUtc <= endOfNextMonthUtc &&
                        d.DidHappen == false))
                .Include(m => m.MeetingScheduleDateLists)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.Description,
                    StartDateTimeUtc = m.StartDateTimeUtc,
                    EndDateTimeUtc = m.EndDateTimeUtc,
                    DateLists = m.MeetingScheduleDateLists
                        .Where(d =>
                            d.StartDateTimeUtc >= todayStartUtc &&
                            d.StartDateTimeUtc <= endOfNextMonthUtc &&
                            d.DidHappen == false)
                        .Select(d => new
                        {
                            d.StartDateTimeUtc
                        })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            // 5) Bellek tarafında (in-memory) dönüştürme
            var finalList = rawMeetings
                .Select(m => new MeetingsStartTodayDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,

                    // StartTime => UTC den local'e => TimeOfDay
                    StartTime = m.StartDateTimeUtc.HasValue
                        ? TimeZoneInfo.ConvertTimeFromUtc(
                            m.StartDateTimeUtc.Value,
                            userTimeZone).TimeOfDay
                        : (TimeSpan?)null,

                    EndTime = m.EndDateTimeUtc.HasValue
                        ? TimeZoneInfo.ConvertTimeFromUtc(
                            m.EndDateTimeUtc.Value,
                            userTimeZone).TimeOfDay
                        : (TimeSpan?)null,

                    MeetingScheduleDateLists = m.DateLists
                        .Select(d => new MeetingScheduleDateListDto
                        {
                            Date = ConvertToLocalDateOnly(d.StartDateTimeUtc, userTimeZone)
                        })
                        .ToList()
                })
                .ToList();

            if (!finalList.Any())
            {
                return Result<MeetingsStartTodayDto>
                    .Fail<MeetingsStartTodayDto>(
                        "Belirtilen tarih aralığında toplantı bulunamadı.");
            }

            return Result<MeetingsStartTodayDto>
                .Ok(finalList.AsQueryable(),
                    "Toplantılar başarıyla listelendi.");
        }

        // -- Helper method: UTC -> Local DateOnly
        private static DateOnly ConvertToLocalDateOnly(DateTime utcDateTime, TimeZoneInfo tz)
        {
            var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tz);
            return new DateOnly(localDateTime.Year, localDateTime.Month, localDateTime.Day);
        }
    }
}
