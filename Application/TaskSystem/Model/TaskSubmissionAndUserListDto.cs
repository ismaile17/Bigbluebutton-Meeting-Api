using Application.FileSystem.Models;
using System;
using System.Collections.Generic;

namespace Application.TaskSystem.Model
{
    /// <summary>
    /// Tüm gerekli bilgileri içeren ana DTO.
    /// </summary>
    public class TaskSubmissionAndUserListDto
    {
        public int TaskAssignmentId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? LateDueDate { get; set; }
        public bool AllowLateSubmissions { get; set; }
        public decimal? MaxGrade { get; set; }
        public List<UserTaskSubmissionDto> Users { get; set; } = new List<UserTaskSubmissionDto>();
    }

    /// <summary>
    /// Kullanıcı bilgilerini ve gönderim durumlarını içeren DTO.
    /// </summary>
    public class UserTaskSubmissionDto
    {
        public TaskUserDto User { get; set; }
        public bool HasSubmitted { get; set; }
        public string? Feedback { get; set; }
        public decimal? Grade { get; set; }
        public string? ParticipantNote { get; set; }
        public List<FileListDto>? Files { get; set; } // Dosya listesi eklendi

    }

    /// <summary>
    /// Kullanıcı bilgilerini taşıyan DTO.
    /// </summary>
    public class TaskUserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        // Diğer gerekli kullanıcı alanlarını buraya ekleyebilirsiniz
    }
}
