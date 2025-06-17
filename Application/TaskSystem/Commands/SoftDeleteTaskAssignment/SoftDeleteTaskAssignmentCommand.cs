using Application.FileSystem.Services;
using Application.Shared.Results;
using Application.TaskSystem.Model;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Enum;

namespace Application.TaskSystem.Commands.SoftDeleteTaskAssignment
{
    public class SoftDeleteTaskAssignmentCommand : IRequest<ResultSingle<TaskAssignmentDto>>
    {
        public int AppUserId { get; set; }
        public int Id { get; set; }
    }

    public class SoftDeleteTaskAssignmentCommandHandler : IRequestHandler<SoftDeleteTaskAssignmentCommand, ResultSingle<TaskAssignmentDto>>
    {
        private readonly IRepository<TaskAssignment> _taskAssignmentRepository;
        private readonly IRepository<TaskSubmission> _taskSubmissionRepository;
        private readonly IRepository<Domain.Entities.FileSystem> _fileSystemRepository;
        private readonly IDeleteFilesService _deleteFilesService;
        private readonly ILogger<SoftDeleteTaskAssignmentCommandHandler> _logger;

        public SoftDeleteTaskAssignmentCommandHandler(
            IRepository<TaskAssignment> taskAssignmentRepository,
            IRepository<TaskSubmission> taskSubmissionRepository,
            IRepository<Domain.Entities.FileSystem> fileSystemRepository,
            IDeleteFilesService deleteFilesService,
            ILogger<SoftDeleteTaskAssignmentCommandHandler> logger)
        {
            _taskAssignmentRepository = taskAssignmentRepository;
            _taskSubmissionRepository = taskSubmissionRepository;
            _fileSystemRepository = fileSystemRepository;
            _deleteFilesService = deleteFilesService;
            _logger = logger;
        }

        public async Task<ResultSingle<TaskAssignmentDto>> Handle(SoftDeleteTaskAssignmentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. TaskAssignment kaydını alıyoruz
                var taskAssignment = _taskAssignmentRepository.Get(a => a.Id == request.Id && a.CreatedBy == request.AppUserId);
                if (taskAssignment == null)
                {
                    return Result.Fail<TaskAssignmentDto>(null, "Task assignment not found.");
                }

                // 2. TaskAssignment'ı soft-delete ediyoruz
                taskAssignment.IsActive = -1;
                _taskAssignmentRepository.UpdateWithoutCommit(taskAssignment);
                var commitResult = await _taskAssignmentRepository.CommitAsync(cancellationToken);
                if (commitResult == -1)
                {
                    return Result.Fail<TaskAssignmentDto>(null, "Task assignment could not be updated.");
                }

                // 3. İlgili FileSystem kayıtlarını alıyoruz (PageType = TaskAssigment)
                var taskAssignmentFileIds = _fileSystemRepository.GetAll
                    .Where(f => f.PageId == request.Id && f.PageType == FileSystemPageType.TaskAssigment && !f.IsDeleted)
                    .Select(f => f.Id)
                    .ToList();

                // 4. TaskAssignment'a ait TaskSubmissions'ı alıyoruz
                var taskSubmissions = _taskSubmissionRepository.GetAll
                    .Where(ts => ts.TaskAssignmentId == request.Id && ts.IsActive != -1)
                    .ToList();

                var taskSubmissionIds = taskSubmissions.Select(ts => ts.Id).ToList();

                // 5. TaskSubmissions'a ait FileSystem kayıtlarını alıyoruz (PageType = TaskSubmission)
                var taskSubmissionFileIds = _fileSystemRepository.GetAll
                    .Where(f => taskSubmissionIds.Contains(f.PageId) && f.PageType == FileSystemPageType.TaskSubmission && !f.IsDeleted)
                    .Select(f => f.Id)
                    .ToList();

                // 6. TaskSubmissions'ı soft-delete ediyoruz
                foreach (var submission in taskSubmissions)
                {
                    submission.IsActive = -1;
                    _taskSubmissionRepository.UpdateWithoutCommit(submission);
                }

                var taskSubmissionCommitResult = await _taskSubmissionRepository.CommitAsync(cancellationToken);
                if (taskSubmissionCommitResult == -1)
                {
                    // TaskAssignment'ı geri almak
                    taskAssignment.IsActive = 1;
                    _taskAssignmentRepository.UpdateWithoutCommit(taskAssignment);
                    await _taskAssignmentRepository.CommitAsync(cancellationToken);

                    return Result.Fail<TaskAssignmentDto>(null, "Task submissions could not be updated.");
                }

                // 7. Tüm FileSystem ID'lerini birleştiriyoruz
                var allFileIdsToDelete = taskAssignmentFileIds
                    .Concat(taskSubmissionFileIds)
                    .ToList();

                // 8. Dosyaları silme işlemini gerçekleştiriyoruz
                if (allFileIdsToDelete.Any())
                {
                    var deleteResult = await _deleteFilesService.DeleteFilesAsync(allFileIdsToDelete, request.AppUserId, cancellationToken);
                    if (!deleteResult.Success)
                    {
                        _logger.LogError($"File deletion failed: {deleteResult.Message}");

                        // TaskAssignment ve TaskSubmissions'ı geri almak
                        taskAssignment.IsActive = 1;
                        _taskAssignmentRepository.UpdateWithoutCommit(taskAssignment);
                        await _taskAssignmentRepository.CommitAsync(cancellationToken);

                        foreach (var submission in taskSubmissions)
                        {
                            submission.IsActive = 1;
                            _taskSubmissionRepository.UpdateWithoutCommit(submission);
                        }
                        await _taskSubmissionRepository.CommitAsync(cancellationToken);

                        return Result.Fail<TaskAssignmentDto>(null, $"Failed to delete related files: {deleteResult.Message}");
                    }
                }

                // 9. Başarılı sonuç döndürüyoruz
                var taskAssignmentDto = new TaskAssignmentDto
                {
                    Id = taskAssignment.Id
                    // Diğer gerekli alanları doldurabilirsiniz
                };

                return Result.Ok(taskAssignmentDto, "Task assignment successfully soft-deleted and related files deleted.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while soft-deleting TaskAssignment ID: {request.Id}");
                return Result.Fail<TaskAssignmentDto>(null, $"An error occurred: {ex.Message}");
            }
        }
    }
}
