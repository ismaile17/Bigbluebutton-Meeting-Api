using Application.CompletedMeetings.Model;
using Application.Shared.Results;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CompletedMeeting.Queries.GetCompletedMeetingByUserId
{
    public class GetCompletedMeetingByManagerUserIdQuery:IRequest<Result<CompletedMeetingDto>>
    {
        public int AppUserId { get; set; }
    }

    public class GetCompletedMeetingByManagerUserIdQueryHandler : IRequestHandler<GetCompletedMeetingByManagerUserIdQuery, Result<CompletedMeetingDto>>
    {
        private readonly IRepository<Domain.Entities.CompletedMeeting> _completedMeetingRepository;
        private readonly IMapper _mapper;

        public GetCompletedMeetingByManagerUserIdQueryHandler(IRepository<Domain.Entities.CompletedMeeting> completedMeetingRepository, IMapper mapper)
        {
            _completedMeetingRepository = completedMeetingRepository;
            _mapper = mapper;
        }

        public async Task<Result<CompletedMeetingDto>> Handle(GetCompletedMeetingByManagerUserIdQuery request, CancellationToken cancellationToken)
        {
            var query = _completedMeetingRepository.GetMany(a=>a.UserId == request.AppUserId && a.IsActive == 1);

            var data = await Task.Run(() => query).Result.ProjectTo<CompletedMeetingDto>(_mapper.ConfigurationProvider).OrderByDescending(a => a.Id).ToListAsync();
            return Result.Ok(data.AsQueryable(), $"Başarılı");
        }
    }
}
