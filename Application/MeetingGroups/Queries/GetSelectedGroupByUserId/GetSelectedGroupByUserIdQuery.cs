using Application.MeetingGroups.Model;
using Application.Shared.Results;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Application.MeetingGroups.Queries.GetSelectedGroupByUserId
{
    public class GetSelectedGroupByUserIdQuery:IRequest<Result<SelectedGroupDto>>
    {
        public int AppUserId { get; set; }
    }

    public class GetSelectedGroupByUserIdQueryHandler : IRequestHandler<GetSelectedGroupByUserIdQuery, Result<SelectedGroupDto>>
    {
        private readonly IRepository<MeetingGroup> _meetingGroupRepository;
        private readonly IMapper _mapper;

        public GetSelectedGroupByUserIdQueryHandler(IRepository<MeetingGroup> meetingGroupRepository, IMapper mapper)
        {
            _meetingGroupRepository = meetingGroupRepository;
            _mapper = mapper;
        }

        public async Task<Result<SelectedGroupDto>> Handle(GetSelectedGroupByUserIdQuery request, CancellationToken cancellationToken)
        {
            var meetingGroups = _meetingGroupRepository.GetMany(a => a.UserId == request.AppUserId && a.IsActive == 1);

            var data = await Task.Run(() => meetingGroups).Result.ProjectTo<SelectedGroupDto>(_mapper.ConfigurationProvider).OrderByDescending(a => a.Id).ToListAsync();
            return Result.Ok(data.AsQueryable(), $"Başarılı");
        }
    }
}
