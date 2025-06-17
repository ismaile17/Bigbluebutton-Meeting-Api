using Application.FileSystem.Models;
using Application.MeetingGroupDetailForParticipant.Models;
using Application.Shared.Results;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Enum;

namespace Application.MeetingGroups.Queries.GetAllMeetingGroups
{
    public class GetAllMeetingGroupAndDetailForParticipantQuery
        : IRequest<ResultSingle<MeetingGroupDetailDto>>
    {
        public int AppUserId { get; set; }
        public int MeetingGroupId { get; set; }
    }

    public class GetAllMeetingGroupAndDetailForParticipantQueryHandler
        : IRequestHandler<GetAllMeetingGroupAndDetailForParticipantQuery, ResultSingle<MeetingGroupDetailDto>>
    {
        private readonly IRepository<MeetingGroup> _meetingGroupRepository;
        private readonly IRepository<Domain.Entities.FileSystem> _fileSystemRepository;
        private readonly IRepository<TaskSubmission> _taskSubmissionRepository;
        private readonly IRepository<Meeting> _meetingRepository;
        private readonly IRepository<ManagerParticipant> _managerParticipantRepository;
        private readonly UserManager<AppUser> _userManager;

        public GetAllMeetingGroupAndDetailForParticipantQueryHandler(
            IRepository<MeetingGroup> meetingGroupRepository,
            IRepository<Domain.Entities.FileSystem> fileSystemRepository,
            IRepository<TaskSubmission> taskSubmissionRepository,
            IRepository<Meeting> meetingRepository,
            IRepository<ManagerParticipant> managerParticipantRepository,
            UserManager<AppUser> userManager)
        {
            _meetingGroupRepository = meetingGroupRepository;
            _fileSystemRepository = fileSystemRepository;
            _taskSubmissionRepository = taskSubmissionRepository;
            _meetingRepository = meetingRepository;
            _managerParticipantRepository = managerParticipantRepository;
            _userManager = userManager;
        }

        public async Task<ResultSingle<MeetingGroupDetailDto>> Handle(
            GetAllMeetingGroupAndDetailForParticipantQuery request,
            CancellationToken cancellationToken)
        {
            // 1) MeetingGroup çek
            var meetingGroup = _meetingGroupRepository.Get(
                x => x.Id == request.MeetingGroupId,
                x => x.MeetingGroupUserLists,
                x => x.MeetingMeetingGroups,
                x => x.TaskAssignments
            );

            if (meetingGroup == null)
            {
                return Result.Fail<MeetingGroupDetailDto>(
                    null, "Grup bulunamadı.");
            }

            // 2) Kullanıcı gerçekten bu grupta var mı?
            var userInGroup = meetingGroup.MeetingGroupUserLists
                .FirstOrDefault(mgul => mgul.AppUserId == request.AppUserId);
            if (userInGroup == null)
            {
                return Result.Fail<MeetingGroupDetailDto>(
                    null, "Kullanıcı bu toplantı grubuna kayıtlı değil.");
            }

            // 3) Kullanıcı bilgisi ve TimeZone
            var managerId = meetingGroup.CreatedBy;

            var appUser = await _userManager.Users
                .Include(u => u.ManagerParticipants)
                .FirstOrDefaultAsync(u => u.Id == request.AppUserId,
                    cancellationToken);

            if (appUser == null)
            {
                return Result.Fail<MeetingGroupDetailDto>(
                    null, "Kullanıcı bulunamadı.");
            }

            var userTimeZoneId = appUser.TimeZoneId ?? "Turkey Standard Time";
            var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneId);

            // 4) ManagerParticipant kaydı (aktif mi?)
            var managerParticipant = _managerParticipantRepository.Get(
                mp => mp.ManagerId == managerId &&
                      mp.ParticipantId == request.AppUserId);

            if (managerParticipant == null ||
                managerParticipant.IsActive != true ||
                (managerParticipant.ExpiryDateIsActive == true &&
                 managerParticipant.ExpiryDate.HasValue &&
                 managerParticipant.ExpiryDate.Value >= DateTime.Now))
            {
                return Result.Fail<MeetingGroupDetailDto>(
                    null, "Kullanıcı aktif değil veya erişim süresi dolmuş.");
            }

            // 5) TaskAssignments
            var groupId = meetingGroup.Id;
            var activeTaskAssignments = meetingGroup.TaskAssignments?
                .Where(ta => ta.IsActive == 1)
                .ToList()
                ?? new List<TaskAssignment>();

            var assignmentIds = activeTaskAssignments
                .Select(ta => ta.Id)
                .ToList();

            // 6) TaskSubmissions oluştur / kontrol et
            var taskSubmissionsDict = new Dictionary<int, TaskSubmission>();
            foreach (var assignmentId in assignmentIds)
            {
                var taskSubmission = _taskSubmissionRepository.Get(
                    ts => ts.TaskAssignmentId == assignmentId &&
                          ts.CreatedBy == request.AppUserId);

                // Yoksa yeni oluştur
                if (taskSubmission == null)
                {
                    taskSubmission = new TaskSubmission
                    {
                        TaskAssignmentId = assignmentId,
                        CreatedBy = request.AppUserId,
                        // DB'de UTC sakladığınızı varsayıyoruz
                        CreatedTime = DateTime.UtcNow
                    };

                    _taskSubmissionRepository.InsertWithoutCommit(taskSubmission);
                    await _taskSubmissionRepository.CommitAsync(cancellationToken);
                }

                taskSubmissionsDict[assignmentId] = taskSubmission;
            }

            // 7) Dosya sisteminden ilgili veriler
            var pageIds = new List<int> { groupId };
            pageIds.AddRange(assignmentIds);
            pageIds.AddRange(taskSubmissionsDict.Values.Select(ts => ts.Id));

            var allFiles = _fileSystemRepository.GetMany(
                    f => pageIds.Contains(f.PageId) && !f.IsDeleted)
                .ToList();

            // 7a) Grup dosyaları
            var groupFiles = allFiles
                .Where(f => f.PageId == groupId &&
                            f.PageType == FileSystemPageType.GroupTeacher)
                .Select(f => new FileListDto
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    ContentType = f.ContentType,
                    FileSize = f.FileSize,
                    // UTC => local
                    CreatedTime = ConvertFromUtc(f.CreatedTime, userTimeZone) ?? DateTime.UtcNow,
                })
                .ToList();

            // 7b) Assignment dosyaları
            var assignmentFilesDict = allFiles
                .Where(f => assignmentIds.Contains(f.PageId)
                            && f.PageType == FileSystemPageType.TaskAssigment && f.IsDeleted == false)
                .GroupBy(f => f.PageId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => new FileListDto
                    {
                        Id = f.Id,
                        FileName = f.FileName,
                        ContentType = f.ContentType,
                        FileSize = f.FileSize,
                        CreatedTime = ConvertFromUtc(f.CreatedTime, userTimeZone) ?? DateTime.UtcNow,
                    }).ToList()
                );

            // 7c) Submission dosyaları
            var submissionFilesDict = allFiles
                .Where( f => taskSubmissionsDict.Values.Select(ts => ts.Id)
                                .Contains(f.PageId)
                            && f.PageType == FileSystemPageType.TaskSubmission && f.IsDeleted == false)
                .GroupBy(f => f.PageId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => new FileListDto
                    {
                        Id = f.Id,
                        FileName = f.FileName,
                        ContentType = f.ContentType,
                        FileSize = f.FileSize,
                        CreatedTime = ConvertFromUtc(f.CreatedTime, userTimeZone) ?? DateTime.UtcNow,
                    }).ToList()
                );

            // 8) Meetings
            var meetingIds = meetingGroup.MeetingMeetingGroups?
                .Select(mmg => mmg.MeetingId)
                .ToList()
                ?? new List<int>();

            var meetings = _meetingRepository.GetMany(
                m => meetingIds.Contains(m.Id) && m.IsActive == 1,
                m => m.MeetingScheduleDateLists
            ).ToList();

            // Meetings => DTO
            var meetingsDtoList = meetings.Select(m =>
            {
                // StartDate => local
                DateTime? localStartDate = null;
                if (m.StartDate != null)
                {
                    localStartDate = ConvertFromUtc(m.StartDate, userTimeZone);
                }

                // EndDate => local
                DateTime? localEndDate = null;
                if (m.EndDate.HasValue)
                {
                    localEndDate = ConvertFromUtc(m.EndDate.Value, userTimeZone);
                }

                // StartTime => eğer DB'de TimeSpan ama "asıl UTC" => 
                // genelde StartTime bir TimeSpan olarak saklanıyor. 
                // "Local'e" dönüştürmek isterseniz, localStartDate + StartTime yaparsınız.
                // Basit kullanım: m.StartTime'ı TimeSpan olarak döndür, 
                // UI'da "HH:mm" olarak işlenir.
                TimeSpan? localStartTime = null;
                if (m.StartTime.HasValue && localStartDate.HasValue)
                {
                    // localStartDate + m.StartTime => 
                    // tam local DateTime => localTimeOfDay
                    var combinedLocalStart = localStartDate.Value
                        .Date
                        .Add(m.StartTime.Value);
                    localStartTime = combinedLocalStart.TimeOfDay;
                }

                // EndTime => benzer mantık
                TimeSpan? localEndTime = null;
                if (m.EndTime.HasValue && localEndDate.HasValue)
                {
                    var combinedLocalEnd = localEndDate.Value
                        .Date
                        .Add(m.EndTime.Value);
                    localEndTime = combinedLocalEnd.TimeOfDay;
                }

                // MeetingScheduleDateLists => local tarih
                var scheduleListDto = m.MeetingScheduleDateLists?
                    .Select(msdl =>
                    {
                        // msdl.StartDateTimeUtc => local
                        // Bizim senaryomuzda "msdl.Date" 
                        // DB'de "DateOnly" gibi ise
                        // StartDateTimeUtc varsa ConvertFromUtc(...) yapabiliriz.
                        // Aşağıda en basit haliyle msdl.Date'i döndürüyorum.
                        return new MeetingScheduleDateDetailDto
                        {
                            Date = msdl.Date,
                            DidHappen = msdl.DidHappen
                        };
                    })
                    .ToList()
                    ?? new List<MeetingScheduleDateDetailDto>();

                return new MeetingDetailForParticipantDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,

                    // localStartDate => istersen "sadece .Date" 
                    // ya da tam DateTime
                    StartDate = localStartDate ?? DateTime.UtcNow,

                    // local times
                    StartTime = localStartTime,
                    EndTime = localEndTime,

                    DateScheduleList = scheduleListDto
                };
            }).ToList();

            // 9) Görev atamaları => DTO
            var tasksDto = activeTaskAssignments.Select(ta =>
            {
                // DueDate => local
                var localDueDate = (DateTime?)null;
                if (ta.DueDate.HasValue)
                {
                    localDueDate = ConvertFromUtc(ta.DueDate.Value, userTimeZone);
                }

                // LateDueDate => local
                var localLateDueDate = (DateTime?)null;
                if (ta.LateDueDate.HasValue)
                {
                    localLateDueDate = ConvertFromUtc(ta.LateDueDate.Value, userTimeZone);
                }

                // CreatedTime => local
                var localCreatedTime = ConvertFromUtc(ta.CreatedTime, userTimeZone);

                assignmentFilesDict.TryGetValue(ta.Id, out var assignmentFiles);
                taskSubmissionsDict.TryGetValue(ta.Id, out var taskSubmission);

                // Submission files
                var submissionFiles = (taskSubmission != null
                                       && submissionFilesDict.ContainsKey(taskSubmission.Id))
                    ? submissionFilesDict[taskSubmission.Id]
                    : new List<FileListDto>();

                // TaskSubmission => istersen CreatedTime vb. local'e çevir
                var taskSubmissionDto = (taskSubmission != null)
                    ? new TaskSubmissionDetaiForParticipantlDto
                    {
                        Id = taskSubmission.Id,
                        Grade = taskSubmission.Grade,
                        Feedback = taskSubmission.Feedback,
                        ParticipantNote = taskSubmission.ParticipantNote,

                        // Submission creation time => local?
                        // CreatedTime = ConvertFromUtc(taskSubmission.CreatedTime, userTimeZone),

                        SubmissionFiles = submissionFiles
                    }
                    : null;

                return new TaskAssignmentDetailForParticipantDto
                {
                    Id = ta.Id,
                    Title = ta.Title,
                    Description = ta.Description,
                    DueDate = localDueDate,
                    LateDueDate = localLateDueDate,
                    CreatedTime = localCreatedTime,
                    AllowLateSubmissions = ta.AllowLateSubmissions,
                    MaxGrade = ta.MaxGrade,
                    AssignmentFiles = assignmentFiles ?? new List<FileListDto>(),
                    TaskSubmission = taskSubmissionDto
                };
            }).ToList();

            // 10) Nihai DTO
            var meetingGroupDto = new MeetingGroupDetailDto
            {
                Id = meetingGroup.Id,
                Name = meetingGroup.Name,
                Description = meetingGroup.Description,
                GroupFiles = groupFiles,
                Meetings = meetingsDtoList,
                Tasks = tasksDto
            };

            return ResultSingle<MeetingGroupDetailDto>
                .Ok(meetingGroupDto, "Başarılı");
        }

        /// <summary>
        /// DB'de tarihler UTC olarak saklanıyor varsayımıyla 
        /// DateTime'i kullanıcı local zamanına dönüştürür.
        /// </summary>
        private static DateTime? ConvertFromUtc(DateTime utcDateTime, TimeZoneInfo userTimeZone)
        {
            if (utcDateTime.Kind == DateTimeKind.Unspecified)
            {
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, userTimeZone);
        }
    }
}
