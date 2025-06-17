using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Enum;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;

namespace Application.FileSystem.Services
{
    public class FileSystemAuthControlService
    {
        private readonly IRepository<MeetingGroup> _groupRepository;
        private readonly IRepository<TaskAssignment> _assignmentRepository;
        private readonly IRepository<TaskSubmission> _submissionRepository;
        private readonly IRepository<Domain.Entities.FileSystem> _fileRepository;
        private readonly UserManager<AppUser> _userManager;

        public FileSystemAuthControlService(IRepository<MeetingGroup> groupRepository, IRepository<TaskAssignment> assignmentRepository, IRepository<TaskSubmission> submissionRepository, IRepository<Domain.Entities.FileSystem> fileRepository, UserManager<AppUser> userManager)
        {
            _groupRepository = groupRepository;
            _assignmentRepository = assignmentRepository;
            _submissionRepository = submissionRepository;
            _fileRepository = fileRepository;
            _userManager = userManager;
        }

        public async Task<bool> FileAuthControl(int appUserId, FileSystemPageType fileType, int entityId, bool isDelete, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(appUserId.ToString());
            
            if (user.UserType == 1 && fileType != FileSystemPageType.TaskSubmission)
            {
                return true;
            }

            if (user.UserType == 1 && fileType == FileSystemPageType.TaskSubmission)
            {
                var createdBy = await _submissionRepository
                    .GetMany(s => s.Id == entityId)
                    .Select(s => s.TaskAssignment.CreatedBy)
                    .FirstOrDefaultAsync(cancellationToken);

                if (createdBy != null)
                {
                    return true;
                }
            }

            if (user.UserType == 2 && fileType == FileSystemPageType.TaskSubmission)
            {
                var hasSubmission = await _submissionRepository
                    .GetMany(s => s.Id == entityId && s.CreatedBy == user.Id)
                    .AnyAsync(cancellationToken);

                if (hasSubmission)
                {
                    var submission = await _submissionRepository.GetMany(s => s.Id == entityId && s.CreatedBy == user.Id)
                        .Select(s => new { s.TaskAssignment.DueDate, s.TaskAssignment.AllowLateSubmissions })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (submission != null)
                    {
                        // Eğer isDelete false ise ve tarih koşulları karşılanıyorsa
                        if (!isDelete &&
                            (submission.DueDate >= DateTime.UtcNow || (submission.AllowLateSubmissions == true && submission.DueDate < DateTime.UtcNow)))
                        {
                            return true;
                        }
                    }
                }
            }

            if (user.UserType == 2 && fileType == FileSystemPageType.TaskAssigment)
            {
                var isUserInMeetingGroupUserListByTaskAssignmentId = await _assignmentRepository
                    .GetMany(t => t.Id == entityId)
                    .SelectMany(t => t.MeetingGroup.MeetingGroupUserLists)
                    .AnyAsync(mgul => mgul.AppUserId == user.Id, cancellationToken);

                if (isUserInMeetingGroupUserListByTaskAssignmentId)
                {
                    return true;
                }
            }

            if (user.UserType == 2 && fileType == FileSystemPageType.GroupTeacher)
            {
                var isUserInMeetingGroupUserListByGroupId = await _groupRepository
                    .GetMany(mg => mg.Id == entityId)
                    .SelectMany(mg => mg.MeetingGroupUserLists)
                    .AnyAsync(mgul => mgul.AppUserId == user.Id, cancellationToken);

                if (isUserInMeetingGroupUserListByGroupId)
                {
                    return true;
                }
            }

            // Tüm diğer durumlar için false döndür
            return false;
        }
    }
}
