using Application.Groups.Model;
using Application.Meetings.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;

namespace Application.MeetingGroups.Queries.GetAllMeetingGroups
{
    public class GetAllMeetingGroupQuery : IRequest<Result<MeetingGroupDto>>
    {
        //KONTROL EDİLECEK
    }
    public class GetAllMeetingGroupQueryHandler : IRequestHandler<GetAllMeetingGroupQuery, Result<MeetingGroupDto>>
    {
        private readonly IRepository<MeetingGroup> _meetingGroupRepository;
        private readonly IMapper _mapper;

        public GetAllMeetingGroupQueryHandler(IRepository<MeetingGroup> meetingGroupRepository, IMapper mapper)
        {
            _meetingGroupRepository = meetingGroupRepository;
            _mapper = mapper;
        }

        public async Task<Result<MeetingGroupDto>> Handle(GetAllMeetingGroupQuery request, CancellationToken cancellationToken)
        {
            // Direkt repository'den veriyi çekip, DTO'ya dönüştürme.
            var meetingGroups = await Task.Run(() => _meetingGroupRepository.GetAll);
            var meetingGroupDtos = meetingGroups.Select(group => new MeetingGroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                SpecialDescription = group.SpecialDescription,
                Image = group.Image,
                // AppUsers dönüşümü için gerekli olan kısım aşağıda yer almalı
                AppUsers = group.MeetingGroupUserLists.Select(mgul => _mapper.Map<AppUserDto>(mgul.AppUser)).ToList()
            }).ToList();

            return Result.Ok(meetingGroupDtos, "Başarılı");
        }
    }
}