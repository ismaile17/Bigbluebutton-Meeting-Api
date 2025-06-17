using Application.FileSystem.Services;
using Application.Groups.Model;
using Application.Shared.Results;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Enum;

namespace Application.MeetingGroups.Commands.SoftDeleteMeetingGroup
{
    public class SoftDeleteMeetingGroupCommand : IRequest<ResultSingle<MeetingGroupDto>>
    {
        public int AppUserId { get; set; }
        public int Id { get; set; }
    }

    public class SoftDeleteMeetingGroupCommandHandler : IRequestHandler<SoftDeleteMeetingGroupCommand, ResultSingle<MeetingGroupDto>>
    {
        private readonly IRepository<MeetingGroup> _meetingGroupRepository;
        private readonly IRepository<TaskAssignment> _taskAssignmentRepository;
        private readonly IRepository<TaskSubmission> _taskSubmissionRepository;
        private readonly IRepository<Domain.Entities.FileSystem> _fileSystemRepository;
        private readonly IDeleteFilesService _deleteFilesService;
        private readonly ILogger<SoftDeleteMeetingGroupCommandHandler> _logger;

        public SoftDeleteMeetingGroupCommandHandler(
            IRepository<MeetingGroup> meetingGroupRepository,
            IRepository<TaskAssignment> taskAssignmentRepository,
            IRepository<TaskSubmission> taskSubmissionRepository,
            IRepository<Domain.Entities.FileSystem> fileSystemRepository,
            IDeleteFilesService deleteFilesService,
            ILogger<SoftDeleteMeetingGroupCommandHandler> logger)
        {
            _meetingGroupRepository = meetingGroupRepository;
            _taskAssignmentRepository = taskAssignmentRepository;
            _taskSubmissionRepository = taskSubmissionRepository;
            _fileSystemRepository = fileSystemRepository;
            _deleteFilesService = deleteFilesService;
            _logger = logger;
        }

        public async Task<ResultSingle<MeetingGroupDto>> Handle(SoftDeleteMeetingGroupCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. MeetingGroup kaydını alıyoruz
                var meetingGroup = _meetingGroupRepository.Get(a => a.Id == request.Id && a.UserId == request.AppUserId);
                if (meetingGroup == null)
                {
                    return Result.Fail<MeetingGroupDto>(null, "Meeting group not found.");
                }

                // 2. MeetingGroup'ı soft-delete ediyoruz
                meetingGroup.IsActive = -1;
                _meetingGroupRepository.UpdateWithoutCommit(meetingGroup);

                // 3. Veritabanı değişikliklerini kaydediyoruz
                var commitResult = await _meetingGroupRepository.CommitAsync(cancellationToken);
                if (commitResult == -1)
                {
                    return Result.Fail<MeetingGroupDto>(null, "Meeting group could not be updated.");
                }

                // 4. MeetingGroup'a ait FileSystem kayıtlarını alıyoruz (PageType = GroupTeacher)
                var meetingGroupFileIds = _fileSystemRepository.GetAll
                    .Where(f => f.PageId == request.Id && f.PageType == FileSystemPageType.GroupTeacher && !f.IsDeleted)
                    .Select(f => f.Id)
                    .ToList();

                // 5. MeetingGroup'a ait TaskAssignments'ı alıyoruz
                var taskAssignments = _taskAssignmentRepository.GetAll
                    .Where(ta => ta.MeetingGroupId == request.Id && ta.IsActive != -1)
                    .ToList();

                var taskAssignmentIds = taskAssignments.Select(ta => ta.Id).ToList();

                // 6. TaskAssignments'a ait FileSystem kayıtlarını alıyoruz (PageType = TaskAssigment)
                var taskAssignmentFileIds = _fileSystemRepository.GetAll
                    .Where(f => taskAssignmentIds.Contains(f.PageId) && f.PageType == FileSystemPageType.TaskAssigment && !f.IsDeleted)
                    .Select(f => f.Id)
                    .ToList();

                // 7. TaskAssignments'a ait TaskSubmissions'ları alıyoruz
                var taskSubmissions = _taskSubmissionRepository.GetAll
                    .Where(ts => taskAssignmentIds.Contains(ts.TaskAssignmentId) && ts.IsActive != -1)
                    .ToList();

                var taskSubmissionIds = taskSubmissions.Select(ts => ts.Id).ToList();

                // 8. TaskSubmissions'a ait FileSystem kayıtlarını alıyoruz (PageType = TaskSubmission)
                var taskSubmissionFileIds = _fileSystemRepository.GetAll
                    .Where(f => taskSubmissionIds.Contains(f.PageId) && f.PageType == FileSystemPageType.TaskSubmission && !f.IsDeleted)
                    .Select(f => f.Id)
                    .ToList();

                // 9. Tüm FileSystem ID'lerini birleştiriyoruz
                var allFileIdsToDelete = meetingGroupFileIds
                    .Concat(taskAssignmentFileIds)
                    .Concat(taskSubmissionFileIds)
                    .ToList();

                // 10. Dosyaları silme işlemini gerçekleştiriyoruz
                if (allFileIdsToDelete.Any())
                {
                    var deleteResult = await _deleteFilesService.DeleteFilesAsync(allFileIdsToDelete, request.AppUserId, cancellationToken);
                    if (!deleteResult.Success)
                    {
                        _logger.LogError($"File deletion failed: {deleteResult.Message}");

                        // TaskAssignment'ları geri almak (Opsiyonel)
                        // Daha kapsamlı bir geri alma mekanizması gerekebilir

                        return Result.Fail<MeetingGroupDto>(null, $"Failed to delete related files: {deleteResult.Message}");
                    }
                }

                // 11. TaskAssignments ve TaskSubmissions'ları soft-delete edebilirsiniz (Opsiyonel)
                // Bu adım, ilgili TaskAssignments ve TaskSubmissions'ların da silinmesini sağlar

                foreach (var taskAssignment in taskAssignments)
                {
                    taskAssignment.IsActive = -1;
                    _taskAssignmentRepository.UpdateWithoutCommit(taskAssignment);
                }

                foreach (var taskSubmission in taskSubmissions)
                {
                    taskSubmission.IsActive = -1;
                    _taskSubmissionRepository.UpdateWithoutCommit(taskSubmission);
                }

                // 12. Değişiklikleri kaydediyoruz
                var finalCommitResult = await _meetingGroupRepository.CommitAsync(cancellationToken);
                if (finalCommitResult == -1)
                {
                    return Result.Fail<MeetingGroupDto>(null, "Failed to update related TaskAssignments or TaskSubmissions.");
                }

                // 13. Başarılı sonuç döndürüyoruz
                var meetingGroupDto = new MeetingGroupDto
                {
                    Id = meetingGroup.Id,
                    Name = meetingGroup.Name,
                    Description = meetingGroup.Description,
                    // Diğer gerekli alanlar...
                };

                return Result.Ok(meetingGroupDto, "Meeting group successfully soft-deleted and related files deleted.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while soft-deleting MeetingGroup ID: {request.Id}");
                return Result.Fail<MeetingGroupDto>(null, $"An error occurred: {ex.Message}");
            }
        }
    }
}
