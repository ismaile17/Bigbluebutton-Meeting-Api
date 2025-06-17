using Application.Groups.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;

namespace Application.Groups.Commands.EditGroup
{
    public class EditMeetingGroupCommand : IRequest<ResultSingle<MeetingGroupDto>>
    {
        public int Id { get; set; }
        public int AppUserId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? SpecialDescription { get; set; }
        public string? Image { get; set; }

        public List<int?> MeetingGroupUserLists { get; set; }
    }

    public class EditGroupCommandHandler : IRequestHandler<EditMeetingGroupCommand, ResultSingle<MeetingGroupDto>>
    {
        private readonly IRepository<MeetingGroup> _meetingGroupRepository;
        private readonly IMapper _mapper;

        public EditGroupCommandHandler(IRepository<MeetingGroup> meetingGroupRepository, IMapper mapper)
        {
            _meetingGroupRepository = meetingGroupRepository;
            _mapper = mapper;
        }

        public async Task<ResultSingle<MeetingGroupDto>> Handle(EditMeetingGroupCommand request, CancellationToken cancellationToken)
        {   
            var groupdata = _meetingGroupRepository.Get(a => a.Id == request.Id, c => c.MeetingGroupUserLists);
            if (groupdata == null)
            {
                return Result.Fail<MeetingGroupDto>(null, $"Group notfound");
            }


            groupdata.UpdatedBy = request.AppUserId;
            groupdata.Name = request.Name;
            groupdata.UserId = request.AppUserId;
            groupdata.Description = request.Description;
            groupdata.SpecialDescription = request.SpecialDescription;
            groupdata.Image = request.Image;



            var meetingGroupUserLists = groupdata.MeetingGroupUserLists;


            var deletedMatchData = meetingGroupUserLists.Select(a => (int?)a.AppUserId).Except(request.MeetingGroupUserLists)
                                .ToList();
            foreach (var item in deletedMatchData)
            {
                var groupUser = meetingGroupUserLists.FirstOrDefault(a => a.AppUserId == item);
                groupdata.MeetingGroupUserLists.Remove(groupUser);
            }


            var newMatchData = request.MeetingGroupUserLists.Where(x => !meetingGroupUserLists.Any(z => z.AppUserId == x)).ToList();
            foreach (var userId in newMatchData)
            {
                var meetingGroupUserList = new MeetingGroupUserList
                {
                    MeetingGroup = groupdata,
                    AppUserId = userId.Value
                };
                groupdata.MeetingGroupUserLists.Add(meetingGroupUserList);

            }


            _meetingGroupRepository.UpdateWithoutCommit(groupdata);

            var result = await _meetingGroupRepository.CommitAsync(cancellationToken);

            if (result == -1)
            {
                return Result.Fail<MeetingGroupDto>(null, $"Kayıt edilemedi");
            }
            var data = await Task.Run(() => _mapper.Map<MeetingGroup, MeetingGroupDto>(groupdata));
            return Result.Ok(data, $"Oturum oluşturuldu");


        }
    }
}
