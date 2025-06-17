using Application.Groups.Model;
using Application.Meetings.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Groups.Queries.GetGroupById
{
    public class GetMeetingGroupByIdQuery : IRequest<ResultSingle<MeetingGroupDto>>
    {
        public int GroupId { get; set; }
        public int AppUserId { get; set; }
    }

    public class GetGroupByIdQueryHandler : IRequestHandler<GetMeetingGroupByIdQuery, ResultSingle<MeetingGroupDto>>
    {
        private readonly IRepository<MeetingGroup> _groupRepository;
        private readonly IMapper _mapper;

        public GetGroupByIdQueryHandler(IRepository<MeetingGroup> groupRepository, IMapper mapper)
        {
            _groupRepository = groupRepository;
            _mapper = mapper;
        }

        public async Task<ResultSingle<MeetingGroupDto>> Handle(GetMeetingGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var group = await _groupRepository.GetMany(a => a.Id == request.GroupId && a.IsActive == 1)
                .Include(a => a.MeetingGroupUserLists)
                .ThenInclude(mgul => mgul.AppUser)
                .ThenInclude(au => au.ManagerParticipants)
                .FirstOrDefaultAsync();

            if (group == null)
            {
                return Result.Fail<MeetingGroupDto>(null,"ID'si {request.GroupId} olan toplantı bulunamadı.");
            }

            if (group.UserId != request.AppUserId)
            {
                return Result.Fail<MeetingGroupDto>(null, "Size ait böyle bir toplantı bulunamadı.");
            }

            var meetingGroupDto = new MeetingGroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                SpecialDescription = group.SpecialDescription,
                Image = group.Image,
                AppUsers = group.MeetingGroupUserLists?.Select(mgul => new AppUserDto
                {
                    Id = mgul.AppUser.Id,
                    Phone = mgul.AppUser.PhoneNumber,
                    Email = mgul.AppUser.Email,
                    NameSurname = mgul.AppUser.UserName,
                    IsActive = mgul.AppUser.ManagerParticipants.Any(mp => mp.ManagerId == request.AppUserId && mp.ParticipantId == mgul.AppUser.Id) ? mgul.AppUser.ManagerParticipants.FirstOrDefault(mp => mp.ManagerId == request.AppUserId && mp.ParticipantId == mgul.AppUser.Id).IsActive ?? false : false,
                    ParticipantUserName = mgul.AppUser.ManagerParticipants.Any(mp => mp.ManagerId == request.AppUserId && mp.ParticipantId == mgul.AppUser.Id)
                    ? mgul.AppUser.ManagerParticipants.FirstOrDefault(mp => mp.ManagerId == request.AppUserId && mp.ParticipantId == mgul.AppUser.Id).NameSurname ?? string.Empty
                    : string.Empty,
                    PhoneNumber = mgul.AppUser.ManagerParticipants.Any(mp => mp.ManagerId == request.AppUserId && mp.ParticipantId == mgul.AppUser.Id)
                    ? mgul.AppUser.ManagerParticipants.FirstOrDefault(mp => mp.ManagerId == request.AppUserId && mp.ParticipantId == mgul.AppUser.Id).PhoneNumber ?? string.Empty
                    : string.Empty,
                }).ToList() ?? new List<AppUserDto>()
            };

            return Result.Ok(meetingGroupDto, "Başarılı");
        }
    }
}
