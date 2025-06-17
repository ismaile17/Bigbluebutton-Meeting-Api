namespace Domain.Entities
{
    public class TaskAssignment:BaseEntity
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; } // Task son teslim tarihi
        public DateTime? LateDueDate { get; set; } // Geç teslim için eklenen son tarih
        public bool? AllowLateSubmissions { get; set; } = false; // Geç teslimlere izin verilip verilmediği
        public decimal? MaxGrade { get; set; } // Ödevin maksimum puanı
        public int MeetingGroupId { get; set; } // Ödevin atandığı grubun ID'si
        public virtual MeetingGroup MeetingGroup { get; set; } // Ödevin atandığı grup
        public virtual ICollection<TaskSubmission> TaskSubmissions { get; set; } = new List<TaskSubmission>(); // Ödeve ait öğrenci teslimleri
    }
}