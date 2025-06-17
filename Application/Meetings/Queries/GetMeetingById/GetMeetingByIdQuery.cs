using Application.Meetings.Model;
using Application.Shared.Results;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Meetings.Queries.GetMeetingById
{
    public class GetMeetingByIdQuery:IRequest<ResultSingle<MeetingFullDto>>
    {
        public int MeetingId { get; set; }
        public int AppUserId { get; set; }

    }

    public class GetMeetingByIdQueryHandler : IRequestHandler<GetMeetingByIdQuery, ResultSingle<MeetingFullDto>>
    {
        private readonly IRepository<Meeting> _meetingRepository;
        private readonly IMapper _mapper;

        public GetMeetingByIdQueryHandler(IRepository<Meeting> meetingRepository, IMapper mapper)
        {
            _meetingRepository = meetingRepository;
            _mapper = mapper;
        }

        public async Task<ResultSingle<MeetingFullDto>> Handle(GetMeetingByIdQuery request, CancellationToken cancellationToken)
        {
            //var data = _meetingRepository.GetById(request.MeetingId);

            var data = await _meetingRepository.GetAll
                .Include(m => m.MeetingMeetingGroups)
                    .ThenInclude(mg => mg.MeetingGroup)
                .Include(m => m.MeetingModeratorLists)
                    .ThenInclude(mm => mm.AppUser)
                .Include(m => m.MeetingGuestEmailLists)
                .Include(m => m.MeetingScheduleDateLists)
                .FirstOrDefaultAsync(m => m.Id == request.MeetingId);



            if (data == null)
            {
                    return Result.Fail<MeetingFullDto>(null, $"ID'si {request.MeetingId} olan toplantı bulunamadı.");
            }
            else
            {
                var meetingDto = _mapper.Map<MeetingFullDto>(data);
                return Result.Ok(meetingDto, $"Başarılı.");
            }
        }

    }
}
