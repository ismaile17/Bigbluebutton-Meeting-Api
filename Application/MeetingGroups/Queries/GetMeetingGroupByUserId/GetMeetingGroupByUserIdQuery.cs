using Application.Groups.Model;
using Application.Shared.Results;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Application.Groups.Queries.GetMeetingGroupByUserId
{
    public class GetMeetingGroupByUserIdQuery : IRequest<Result<MeetingGroupDto>>
    {
        public int AppUserId { get; set; }
    }

    public class GetMeetingGroupsByUserIdQueryHandler : IRequestHandler<GetMeetingGroupByUserIdQuery, Result<MeetingGroupDto>>
    {
        private readonly IRepository<MeetingGroup> _meetingGroupRepository;
        private readonly IMapper _mapper;

        public GetMeetingGroupsByUserIdQueryHandler(IRepository<MeetingGroup> meetingGroupRepository, IMapper mapper)
        {
            _meetingGroupRepository = meetingGroupRepository;
            _mapper = mapper;
        }

        public async Task<Result<MeetingGroupDto>> Handle(GetMeetingGroupByUserIdQuery request, CancellationToken cancellationToken)
        {
            var meetingGroups = _meetingGroupRepository.GetMany(a => a.UserId == request.AppUserId && a.IsActive == 1)
                .Include(mg => mg.MeetingGroupUserLists); // MeetingGroupUserLists'i dahil edin

            var data = await meetingGroups
                .ProjectTo<MeetingGroupDto>(_mapper.ConfigurationProvider)
                .OrderByDescending(a => a.Id)
                .ToListAsync();

            return Result.Ok(data.AsQueryable(), $"Başarılı");
        }
    }
}
