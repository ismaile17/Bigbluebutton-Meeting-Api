using Application.Meetings.Model;
using Application.Shared.Results;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Application.Meetings.Queries.GetSelectedMeetingByUserId
{
    public class GetSelectedMeetingByUserIdQuery : IRequest<Result<SelectedMeetingDto>>
    {
        public int AppUserId { get; set; }
    }

    public class GetSelectedMeetingByUserIdQueryHandler : IRequestHandler<GetSelectedMeetingByUserIdQuery, Result<SelectedMeetingDto>>
    {
        private readonly IRepository<Meeting> _meetingRepository;
        private readonly IMapper _mapper;

        public GetSelectedMeetingByUserIdQueryHandler(IRepository<Meeting> meetingRepository, IMapper mapper)
        {
            _meetingRepository = meetingRepository;
            _mapper = mapper;
        }

        public async Task<Result<SelectedMeetingDto>> Handle(GetSelectedMeetingByUserIdQuery request, CancellationToken cancellationToken)
        {
            var meetings = _meetingRepository.GetMany(a => a.UserId == request.AppUserId && a.IsActive == 1);

            var data = await Task.Run(() => meetings).Result.ProjectTo<SelectedMeetingDto>(_mapper.ConfigurationProvider).OrderByDescending(a => a.Id).ToListAsync();
            return Result.Ok(data.AsQueryable(), $"Başarılı");
        }
    }
}
