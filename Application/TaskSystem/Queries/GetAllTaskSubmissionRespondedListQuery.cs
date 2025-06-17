using Application.Shared.Results;
using Application.TaskSystem.Model;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Shared.Enum;
using Application.FileSystem.Models;
using Application.Accounts.Services; // FileListDto için eklendi

namespace Application.TaskSystem.Queries
{
    public class GetAllTaskSubmissionRespondedListQuery : IRequest<ResultSingle<TaskSubmissionAndUserListDto>>
    {
        public int TaskAssignmentId { get; set; }
        public int AppUserId { get; set; }
    }

    public class GetAllTaskSubmissionRespondedListQueryHandler : IRequestHandler<GetAllTaskSubmissionRespondedListQuery, ResultSingle<TaskSubmissionAndUserListDto>>
    {
        private readonly IRepository<TaskSubmission> _taskSubmissionRepository;
        private readonly IRepository<MeetingGroup> _meetingGroupRepository;
        private readonly IRepository<Domain.Entities.FileSystem> _fileSystemRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly Accounts.Services.ITimeZoneService _timeZoneService;

        public GetAllTaskSubmissionRespondedListQueryHandler(IRepository<TaskSubmission> taskSubmissionRepository, IRepository<MeetingGroup> meetingGroupRepository, IRepository<Domain.Entities.FileSystem> fileSystemRepository, UserManager<AppUser> userManager, IMapper mapper, ITimeZoneService timeZoneService)
        {
            _taskSubmissionRepository = taskSubmissionRepository;
            _meetingGroupRepository = meetingGroupRepository;
            _fileSystemRepository = fileSystemRepository;
            _userManager = userManager;
            _mapper = mapper;
            _timeZoneService = timeZoneService;
        }

        public async Task<ResultSingle<TaskSubmissionAndUserListDto>> Handle(GetAllTaskSubmissionRespondedListQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // MeetingGroup'u alıyoruz
                var meetingGroup = _meetingGroupRepository.Get(
                    m => m.TaskAssignments.Any(ta => ta.Id == request.TaskAssignmentId),
                    m => m.MeetingGroupUserLists,
                    m => m.TaskAssignments
                );

                if (meetingGroup == null)
                {
                    return Result.Fail<TaskSubmissionAndUserListDto>(null, "TaskAssignment bulunamadı.");
                }

                // TaskAssignment'ı buluyoruz
                var assignment = meetingGroup.TaskAssignments.FirstOrDefault(ta => ta.Id == request.TaskAssignmentId);
                if (assignment == null)
                {
                    return Result.Fail<TaskSubmissionAndUserListDto>(null, "Görev ataması bulunamadı.");
                }

                // Kullanıcının zaman dilimini al
                var userTimeZoneId = await _timeZoneService.GetUserTimeZoneAsync(request.AppUserId);

                // Görev tarihlerini zaman dilimine dönüştür
                var dueDate = assignment.DueDate.HasValue
                    ? _timeZoneService.ConvertToUserTimeZone(assignment.DueDate.Value, userTimeZoneId)
                    : (DateTime?)null;

                var lateDueDate = assignment.LateDueDate.HasValue
                    ? _timeZoneService.ConvertToUserTimeZone(assignment.LateDueDate.Value, userTimeZoneId)
                    : (DateTime?)null;

                // Gruba bağlı kullanıcıların ID'lerini alıyoruz
                var userIds = meetingGroup.MeetingGroupUserLists?
                    .Select(mgu => mgu.AppUserId)
                    .ToList() ?? new List<int>();

                // Kullanıcıları alıyoruz
                var users = _userManager.Users
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new TaskUserDto
                    {
                        Id = u.Id,
                        UserName = u.UserName
                    })
                    .ToList();

                // Kullanıcıların gönderimlerini alıyoruz
                var taskSubmissions = _taskSubmissionRepository.GetMany(
                    ts => ts.TaskAssignmentId == request.TaskAssignmentId && userIds.Contains(ts.CreatedBy ?? 0)
                ).ToList();

                // Gönderimleri kullanıcı ID'sine göre sözlüğe ekliyoruz
                var taskSubmissionDict = taskSubmissions
                    .Where(ts => ts.CreatedBy.HasValue)
                    .ToDictionary(ts => ts.CreatedBy.Value);

                // Dosyaları alıyoruz
                var taskSubmissionIds = taskSubmissions.Select(ts => ts.Id).ToList();

                var fileSystems = _fileSystemRepository.GetMany(
                    fs => taskSubmissionIds.Contains(fs.PageId) && fs.PageType == FileSystemPageType.TaskSubmission
                ).ToList();

                var fileSystemDict = fileSystems
                    .GroupBy(fs => fs.PageId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Kullanıcı gönderimlerini hazırlıyoruz
                var userSubmissions = new List<UserTaskSubmissionDto>();

                foreach (var user in users)
                {
                    taskSubmissionDict.TryGetValue(user.Id, out var taskSubmission);
                    var hasSubmitted = taskSubmission != null;

                    List<FileListDto>? files = null;
                    string? participantNote = null;
                    string? feedBack = null;
                    decimal? grade = null;

                    if (hasSubmitted)
                    {
                        if (fileSystemDict.TryGetValue(taskSubmission.Id, out var fileSystemList))
                        {
                            // Dosyaları FileListDto'ya mapliyoruz
                            files = _mapper.Map<List<FileListDto>>(fileSystemList);
                        }

                        participantNote = taskSubmission.ParticipantNote;
                        feedBack = taskSubmission.Feedback;
                        grade = taskSubmission.Grade;
                    }

                    var userTaskSubmissionDto = CreateUserTaskSubmissionDto(user, hasSubmitted, files, participantNote, feedBack, grade);
                    userSubmissions.Add(userTaskSubmissionDto);
                }

                // DTO oluştur ve zaman dönüşümlerini uygula
                var resultDto = new TaskSubmissionAndUserListDto
                {
                    TaskAssignmentId = request.TaskAssignmentId,
                    Title = assignment.Title,
                    Description = assignment.Description,
                    DueDate = dueDate,
                    LateDueDate = lateDueDate,
                    AllowLateSubmissions = assignment.AllowLateSubmissions ?? false,
                    MaxGrade = assignment.MaxGrade,
                    Users = userSubmissions
                };

                return Result.Ok(resultDto, "Başarılı");
            }
            catch (Exception ex)
            {
                // Hata loglama yapılabilir
                return Result.Fail<TaskSubmissionAndUserListDto>(null, $"Bir hata oluştu: {ex.Message}");
            }
        }

        private UserTaskSubmissionDto CreateUserTaskSubmissionDto(TaskUserDto user, bool hasSubmitted, List<FileListDto>? files, string? participantNote, string? feedBack, decimal? grade)
        {
            return new UserTaskSubmissionDto
            {
                User = user,
                HasSubmitted = hasSubmitted,
                Files = files,
                ParticipantNote = participantNote,
                Feedback = feedBack,
                Grade = grade
            };
        }
    }

}
