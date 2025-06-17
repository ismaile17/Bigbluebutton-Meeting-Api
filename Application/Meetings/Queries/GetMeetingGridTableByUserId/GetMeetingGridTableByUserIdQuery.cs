using Application.Meetings.Model;
using Application.Shared.Results;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Enum;

namespace Application.Meetings.Queries.GetMeetingGridTableByUserId
{
    public class GetMeetingGridTableByUserIdQuery : IRequest<Result<MeetingGridTableDto>>
    {
        public int AppUserId { get; set; }
    }

    public class GetMeetingGridTableByUserIdQueryHandler : IRequestHandler<GetMeetingGridTableByUserIdQuery, Result<MeetingGridTableDto>>
    {
        private readonly IRepository<Meeting> _meetingRepository;
        private readonly IMapper _mapper;

        public GetMeetingGridTableByUserIdQueryHandler(IRepository<Meeting> meetingRepository, IMapper mapper)
        {
            _meetingRepository = meetingRepository;
            _mapper = mapper;
        }

        public async Task<Result<MeetingGridTableDto>> Handle(GetMeetingGridTableByUserIdQuery request, CancellationToken cancellationToken)
        {
            var meetings = _meetingRepository.GetMany(a => a.UserId == request.AppUserId && a.IsActive == 1 && a.ScheduleOrNowMeeting == ScheduleOrNowMeeting.SCHEDULE_MEETING);

            var data = await Task.Run(() => meetings).Result.ProjectTo<MeetingGridTableDto>(_mapper.ConfigurationProvider).OrderByDescending(a => a.Id).ToListAsync();
            return Result.Ok(data.AsQueryable(), $"Başarılı");
        }
    }
}
