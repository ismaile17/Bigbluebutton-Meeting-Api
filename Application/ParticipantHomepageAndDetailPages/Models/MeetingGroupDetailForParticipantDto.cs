using Application.FileSystem.Models;

namespace Application.MeetingGroupDetailForParticipant.Models
{
    public class MeetingGroupDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public List<MeetingDetailForParticipantDto> Meetings { get; set; } = new List<MeetingDetailForParticipantDto>();
        public List<TaskAssignmentDetailForParticipantDto> Tasks { get; set; } = new List<TaskAssignmentDetailForParticipantDto>();
        public List<FileListDto> GroupFiles { get; set; } = new List<FileListDto>(); // Yeni eklenen alan

    }

    public class MeetingDetailForParticipantDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public List<MeetingScheduleDateDetailDto> DateScheduleList { get; set; } = new List<MeetingScheduleDateDetailDto>();
    }


    public class MeetingScheduleDateDetailDto
    {
        public DateOnly Date { get; set; }
        public bool DidHappen { get; set; }
    }



    public class TaskAssignmentDetailForParticipantDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LateDueDate { get; set; }
        public bool? AllowLateSubmissions { get; set; }
        public decimal? MaxGrade { get; set; }
        public TaskSubmissionDetaiForParticipantlDto TaskSubmission { get; set; }
        public List<FileListDto> AssignmentFiles { get; set; } = new List<FileListDto>(); // Yeni eklenen alan

    }


    public class TaskSubmissionDetaiForParticipantlDto
    {
        public int Id { get; set; }
        public decimal? Grade { get; set; }
        public string? Feedback { get; set; }
        public string? ParticipantNote { get; set; }
        public List<FileListDto> SubmissionFiles { get; set; } = new List<FileListDto>(); // Yeni eklenen alan

    }


}
