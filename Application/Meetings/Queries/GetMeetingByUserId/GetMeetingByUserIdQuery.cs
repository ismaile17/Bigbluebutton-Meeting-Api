using Application.Meetings.Model;
using Application.Shared.Results;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Meetings.Queries.GetMeetingByUserId
{
    public class GetMeetingByUserIdQuery : IRequest<Result<MeetingDto>>
    {
        public int AppUserId { get; set; }
    }
    public class GetMeetingByUserIdQueryHandler : IRequestHandler<GetMeetingByUserIdQuery, Result<MeetingDto>>
    {
        private readonly IRepository<Meeting> _meetingRepository;
        private readonly IMapper _mapper;

        public GetMeetingByUserIdQueryHandler(IRepository<Meeting> meetingRepository, IMapper mapper)
        {
            _meetingRepository = meetingRepository;
            _mapper = mapper;
        }

        public async Task<Result<MeetingDto>> Handle(GetMeetingByUserIdQuery request, CancellationToken cancellationToken)
        {


            //var query1 = from meeting in _meetingRepository.GetAll
            //             join 


            var query = _meetingRepository.GetMany(a => a.UserId == request.AppUserId && a.IsActive == 1);

            var data = await Task.Run(() => query).Result.ProjectTo<MeetingDto>(_mapper.ConfigurationProvider).OrderByDescending(a => a.Id).ToListAsync();
            return Result.Ok(data.AsQueryable(), $"Başarılı");



        }
    }
}