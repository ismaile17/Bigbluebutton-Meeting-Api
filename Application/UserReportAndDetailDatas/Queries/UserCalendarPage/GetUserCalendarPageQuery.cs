using Application.Shared.Results;
using Application.UserReportAndDetailDatas.Model.UserCalendarPage;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UserReportAndDetailDatas.Queries.UserCalendarAndHomePage
{
    public class GetUserCalendarPageQuery : IRequest<Result<UserCalendarPageDto>>
    {
        public int AppUserId { get; set; }
    }

    public class UserCalendarPageQueryHandler : IRequestHandler<GetUserCalendarPageQuery, Result<UserCalendarPageDto>>
    {
        private readonly IRepository<Meeting> _meetingRepository;
        private readonly IRepository<Domain.Entities.CompletedMeeting> _completedMeetingRepository;

        public UserCalendarPageQueryHandler(IRepository<Meeting> meetingRepository, IRepository<Domain.Entities.CompletedMeeting> completedMeetingRepository)
        {
            _meetingRepository = meetingRepository;
            _completedMeetingRepository = completedMeetingRepository;
        }

public async Task<Result<UserCalendarPageDto>> Handle(GetUserCalendarPageQuery request, CancellationToken cancellationToken)
{
    var today = DateOnly.FromDateTime(DateTime.Today);

    // Kullanıcıya ait tüm meeting verilerini çekme
    var meetings = await _meetingRepository.GetMany(m => m.UserId == request.AppUserId && m.IsActive == 1)
        .Include(m => m.MeetingScheduleDateLists)
        .ToListAsync(cancellationToken);

    // Tüm toplantı sorgusu, her biri MeetingScheduleDateList üzerinden yapılıyor
    var allMeetings = meetings
        .SelectMany(m => m.MeetingScheduleDateLists, (m, s) => new
        {
            Meeting = m,
            Schedule = s
        })
        .Select(ms => new UserCalendarPageDto
        {
            Id = ms.Meeting.Id,
            Title = ms.Meeting.Name,
            Description = ms.Meeting.Description,
            StartDateAndTime = ms.Schedule.Date.ToDateTime(TimeOnly.FromTimeSpan(ms.Meeting.StartTime ?? TimeSpan.Zero)),
            EndDateAndTime = ms.Schedule.Date.ToDateTime(TimeOnly.FromTimeSpan(ms.Meeting.EndTime ?? TimeSpan.Zero)),
            // Duruma göre MeetingType belirleniyor
            MeetingType = ms.Schedule.Date == today && ms.Schedule.DidHappen == true
                ? 4 // Bugünkü toplantılar gerçekleşen
            : ms.Schedule.Date == today && ms.Schedule.DidHappen == false
            ? 5 // Bugünkü toplantılar gerçekleşmeyenler
                : ms.Schedule.DidHappen
                    ? 2 // Geçmişte gerçekleşenler
                    : ms.Schedule.Date < today
                        ? 3 // Geçmişte gerçekleşmeyenler (missed)
                        : 1 // Gelecek toplantılar
        })
        .ToList();

    return new Result<UserCalendarPageDto>(allMeetings.AsQueryable(), true, "Calendar Page");
}

    }
}
