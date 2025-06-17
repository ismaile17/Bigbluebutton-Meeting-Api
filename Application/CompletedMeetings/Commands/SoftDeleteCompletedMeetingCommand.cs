using Application.CompletedMeetings.Model;
using Application.Shared.Results;
using Infrastructure.Persistence;
using MediatR;

namespace Application.CompletedMeetings.Commands
{
    public class SoftDeleteCompletedMeetingCommand:IRequest<ResultSingle<CompletedMeetingDto>>
    {
        public int AppUserId { get; set; }
        public int Id { get; set; }
    }

    public class SoftDeleteCompletedMeetingCommandHandler : IRequestHandler<SoftDeleteCompletedMeetingCommand, ResultSingle<CompletedMeetingDto>>
    {
        private readonly IRepository<Domain.Entities.CompletedMeeting> _complatedMeeting;

        public SoftDeleteCompletedMeetingCommandHandler(IRepository<Domain.Entities.CompletedMeeting> complatedMeeting)
        {
            _complatedMeeting = complatedMeeting;
        }

        public async Task<ResultSingle<CompletedMeetingDto>> Handle(SoftDeleteCompletedMeetingCommand request, CancellationToken cancellationToken)
        {
            var data = _complatedMeeting.Get(a => a.Id == request.Id && a.UserId == request.AppUserId);
            if (data == null)
            {
                return Result.Fail<CompletedMeetingDto>(null, $"Oturum bulunamadı.");
            }

            data.IsActive = -1;
            _complatedMeeting.UpdateWithoutCommit(data);
            var result = await _complatedMeeting.CommitAsync(cancellationToken);
            if (result == -1)
            {
                return Result.Fail<CompletedMeetingDto>(null, $"Kayıt edilemedi");
            }
            return Result.Ok(new CompletedMeetingDto { }, "Silme işlemi başarılı");
        }
    }
}
