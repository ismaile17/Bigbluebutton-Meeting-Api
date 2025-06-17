using Application.Shared.Results;
using Application.TaskSystem.Model;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.TaskSystem.Commands.EditTaskSubmission
{
    public class EditTaskSubmissionCommand : IRequest<ResultSingle<TaskSubmissionDto>>
    {
        public int AppUserId { get; set; }
        public int TaskSubmissionId { get; set; }
        public string? ParticipantNote { get; set; }
    }

    public class EditTaskSubmissionCommandHandler : IRequestHandler<EditTaskSubmissionCommand, ResultSingle<TaskSubmissionDto>>
    {
        private readonly IRepository<TaskSubmission> _taskSubmissionRepository;
        private readonly IRepository<TaskAssignment> _assignmentRepository;
        private readonly IMapper _mapper;

        public EditTaskSubmissionCommandHandler(
            IRepository<TaskSubmission> taskSubmissionRepository,
            IRepository<TaskAssignment> assignmentRepository,
            IMapper mapper)
        {
            _taskSubmissionRepository = taskSubmissionRepository;
            _assignmentRepository = assignmentRepository;
            _mapper = mapper;
        }

        public async Task<ResultSingle<TaskSubmissionDto>> Handle(EditTaskSubmissionCommand request, CancellationToken cancellationToken)
        {
            var taskSubmission = _taskSubmissionRepository.GetById(request.TaskSubmissionId);
            // TaskSubmission kaydını alıyoruz
            if (taskSubmission == null)
            {
                return Result.Fail<TaskSubmissionDto>(null, "TaskSubmission bulunamadı.");
            }

            // Kullanıcının, ilgili TaskAssignment'ın grubunda olup olmadığını kontrol ediyoruz
            var isUserInMeetingGroupUserListByTaskAssignmentId = await _assignmentRepository
                .GetMany(t => t.Id == taskSubmission.TaskAssignmentId)
                .SelectMany(t => t.MeetingGroup.MeetingGroupUserLists)
                .AnyAsync(mgul => mgul.AppUserId == request.AppUserId, cancellationToken);

            if (!isUserInMeetingGroupUserListByTaskAssignmentId)
            {
                return Result.Fail<TaskSubmissionDto>(null, "Kişi bu grupta yetkili değil.");
            }


            // TaskSubmission'ı güncelliyoruz
            taskSubmission.ParticipantNote = request.ParticipantNote;
            taskSubmission.UpdatedBy = request.AppUserId;
            taskSubmission.UpdatedTime = DateTime.UtcNow;

            _taskSubmissionRepository.UpdateWithoutCommit(taskSubmission);
            await _taskSubmissionRepository.CommitAsync(cancellationToken);

            // Güncellenmiş TaskSubmission DTO'sunu döndürüyoruz
            var taskSubmissionDto = _mapper.Map<TaskSubmissionDto>(taskSubmission);
            return Result.Ok(taskSubmissionDto, "Kayıt güncellendi.");
        }
    }
}
