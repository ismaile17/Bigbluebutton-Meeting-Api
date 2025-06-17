using Application.Groups.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Groups.Commands.CreateMeetingGroup
{
    public class CreateMeetingGroupCommand : IRequest<ResultSingle<MeetingGroupDto>>
    {
        public int AppUserId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? SpecialDescription { get; set; }
        public string? Image { get; set; }
        public List<int?> MeetingGroupUserLists { get; set; }

    }
    public class CreateMeetingGroupCommandHandler : IRequestHandler<CreateMeetingGroupCommand, ResultSingle<MeetingGroupDto>>
    {
        private readonly IRepository<MeetingGroup> _meetingGroupRepository;
        private readonly IRepository<Package> _packageRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public CreateMeetingGroupCommandHandler(IRepository<MeetingGroup> meetingGroupRepository, IRepository<Package> packageRepository, IMapper mapper, UserManager<AppUser> userManager)
        {
            _meetingGroupRepository = meetingGroupRepository;
            _packageRepository = packageRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<ResultSingle<MeetingGroupDto>> Handle(CreateMeetingGroupCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                             .FirstOrDefaultAsync(u => u.Id == request.AppUserId);

            if (user == null)
            {
                return Result.Fail<MeetingGroupDto>(null, "Kullanıcı bulunamadı.");
            }

            var userPackage = await _packageRepository
                .GetMany(u => u.Id == user.PackageId)
                .FirstOrDefaultAsync();


            var groupCount = _meetingGroupRepository.GetMany(mg => mg.UserId == request.AppUserId && mg.IsActive == 1).Count();


            var maxAllowedGroups = (userPackage?.MaxGroup ?? 0) + 5;

            if (groupCount >= maxAllowedGroups)
            {
                return Result.Fail<MeetingGroupDto>(null, $"Max Group : {maxAllowedGroups}");
            }


            var meetingGroup = new MeetingGroup
            {
                CreatedBy = request.AppUserId,
                Name = request.Name,
                Image = request.Image,
                Description = request.Description,
                SpecialDescription = request.SpecialDescription,
                UserId = request.AppUserId,
                MeetingGroupUserLists = new List<MeetingGroupUserList>()
            };

            foreach (var group in request.MeetingGroupUserLists)
            {
                meetingGroup.MeetingGroupUserLists.Add(new MeetingGroupUserList { MeetingGroup = meetingGroup, AppUserId = group .Value});
            }

            _meetingGroupRepository.InsertWithoutCommit(meetingGroup);
            var result = await _meetingGroupRepository.CommitAsync(cancellationToken);
            if (result == -1)
            {
                return Result.Fail<MeetingGroupDto>(null, $"Kayıt edilemedi");
            }
            var data = await Task.Run(() => _mapper.Map<MeetingGroup, MeetingGroupDto>(meetingGroup));
            return Result.Ok(data, $"Grup oluşturuldu");


        }
    }

}

