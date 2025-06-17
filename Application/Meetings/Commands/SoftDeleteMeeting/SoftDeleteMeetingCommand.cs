using Application.Meetings.Model;
using Application.Shared.Results;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Meetings.Commands.DeleteMeeting
{
    public class SoftDeleteMeetingCommand :  IRequest<ResultSingle<MeetingDto>>
    {
        public int AppUserId { get; set; }
        public int Id { get; set; }

    }

    public class SoftDeleteMeetingCommandHandler : IRequestHandler<SoftDeleteMeetingCommand, ResultSingle<MeetingDto>>
    {
        private readonly IRepository<Meeting> _meetingRepository;

        public SoftDeleteMeetingCommandHandler(IRepository<Meeting> meetingRepository)
        {
            _meetingRepository = meetingRepository;
        }

        public async Task<ResultSingle<MeetingDto>> Handle(SoftDeleteMeetingCommand request, CancellationToken cancellationToken)
        {
            var data = _meetingRepository.Get(a => a.Id == request.Id && a.UserId == request.AppUserId);
            if (data == null)
            {
                return Result.Fail<MeetingDto>(null, $"Oturum bulunamadı.");
            }

            data.IsActive = -1;
            _meetingRepository.UpdateWithoutCommit(data);
            var result = await _meetingRepository.CommitAsync(cancellationToken);
            if (result == -1)
            {
                return Result.Fail<MeetingDto>(null, $"Kayıt edilemedi");
            }
            return Result.Ok(new MeetingDto { }, "Silme işlemi başarılı");
        }
    }
}

