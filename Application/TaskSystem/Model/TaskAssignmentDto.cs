using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.TaskSystem.Model
{
    public class TaskAssignmentDto : IMapFrom<TaskAssignment>
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; } // Task son teslim tarihi
        public DateTime? LateDueDate { get; set; } // Geç teslim için eklenen son tarih
        public DateTime? CreatedTime { get; set; } // Geç teslim için eklenen son tarih
        public bool? AllowLateSubmissions { get; set; } = false; // Geç teslimlere izin verilip verilmediği
        public decimal? MaxGrade { get; set; } // Ödevin maksimum puanı
        public int MeetingGroupId { get; set; } // Ödevin atandığı grubun ID'si
        public short? IsActive { get; set; }


        public void Mapping(Profile profile)
        {
            profile.CreateMap<Domain.Entities.TaskAssignment, TaskAssignmentDto>()
                .ForMember(a => a.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(a => a.Title, opt => opt.MapFrom(s => s.Title))
                .ForMember(a => a.Description, opt => opt.MapFrom(s => s.Description))
                .ForMember(a => a.DueDate, opt => opt.MapFrom(s => s.DueDate))
                .ForMember(a => a.LateDueDate, opt => opt.MapFrom(s => s.LateDueDate))
                .ForMember(a => a.AllowLateSubmissions, opt => opt.MapFrom(s => s.AllowLateSubmissions ?? false))
                .ForMember(a => a.MaxGrade, opt => opt.MapFrom(s => s.MaxGrade))
                .ForMember(a => a.MeetingGroupId, opt => opt.MapFrom(s => s.MeetingGroupId))
                .ForMember(a => a.IsActive, opt => opt.MapFrom(s => s.IsActive))
                .ForMember(a => a.CreatedTime, opt => opt.MapFrom(s => s.CreatedTime))
                ;
        }
    }
}
