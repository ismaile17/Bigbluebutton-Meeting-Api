using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.TaskSystem.Model
{
    public class TaskSubmissionDto : IMapFrom<TaskSubmission>
    {
        public decimal? Grade { get; set; } // Öğretmenin verdiği not
        public string? Feedback { get; set; } // Öğretmenin geri bildirimi
        public string? ParticipantNote { get; set; } // Öğretmenin geri bildirimi
        public int TaskAssignmentId { get; set; } // Teslimin ait olduğu ödevin ID'si
        public int? CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }


        public void Mapping(Profile profile)
        {
            profile.CreateMap<Domain.Entities.TaskSubmission, TaskSubmissionDto>()
                .ForMember(a => a.Grade, opt => opt.MapFrom(s => s.Grade))
                .ForMember(a => a.Feedback, opt => opt.MapFrom(s => s.Feedback))
                .ForMember(a => a.TaskAssignmentId, opt => opt.MapFrom(s => s.TaskAssignmentId))
                .ForMember(a => a.CreatedBy, opt => opt.MapFrom(s => s.CreatedBy))
                .ForMember(a => a.CreatedTime, opt => opt.MapFrom(s => s.CreatedTime))
                ;
        }
    }
}
