using Application.LearningDashboards.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities.Learning;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.LearningDashboards.Queries.GetMeetingByMeetingId
{
    public class GetDashboardByMeetingIdQuery : IRequest<ResultSingle<LearningMeetingDto>>
    {
        public string MeetingId { get; set; }

        public GetDashboardByMeetingIdQuery(string meetingId)
        {
            MeetingId = meetingId;
        }

        public class GetDashboardByMeetingIdQueryHandler : IRequestHandler<GetDashboardByMeetingIdQuery, ResultSingle<LearningMeetingDto>>
        {
            private readonly IRepository<LearningMeeting> _learningMeetingRepository;
            private readonly IMapper _mapper;

            public GetDashboardByMeetingIdQueryHandler(IRepository<LearningMeeting> learningMeetingRepository, IMapper mapper)
            {
                _learningMeetingRepository = learningMeetingRepository;
                _mapper = mapper;
            }

            public async Task<ResultSingle<LearningMeetingDto>> Handle(GetDashboardByMeetingIdQuery request, CancellationToken cancellationToken)
            {
                var query = _learningMeetingRepository.GetAll
                    .Include(m => m.Attendees)
                        .ThenInclude(a => a.Sessions)
                    .Include(m => m.Files)
                    .Include(m => m.Polls)
                        .ThenInclude(p => p.PollVotes)
                    .Where(m => m.InternalMeetingId == request.MeetingId);

                var learningMeeting = await query.FirstOrDefaultAsync(cancellationToken);

                if (learningMeeting == null)
                {
                    return Result.Fail<LearningMeetingDto>(null, "Veri bulunamadı");
                }

                var learningMeetingDto = _mapper.Map<LearningMeetingDto>(learningMeeting);
                return Result.Ok(learningMeetingDto, "Veri başarıyla getirildi");
            }
        }
    }
}
