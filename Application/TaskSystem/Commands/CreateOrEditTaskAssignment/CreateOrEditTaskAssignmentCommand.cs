using Application.Cupon.Models;
using Application.Shared.Results;
using Application.TaskSystem.Model;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;

namespace Application.TaskSystem.Commands.CreateTaskAssignment
{
    public class CreateOrEditTaskAssignmentCommand : IRequest<ResultSingle<TaskAssignmentDto>>
    {
        public int? Id { get; set; } // Var olan görevi düzenlemek için ID
        public int AppUserId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool? AllowLateSubmissions { get; set; } // Geç teslimlere izin verilip verilmediği
        public DateTime? DueDate { get; set; } // Task son teslim tarihi
        public DateTime? LateDueDate { get; set; } // Geç teslim için eklenen son tarih
        public decimal? MaxGrade { get; set; } // Ödevin maksimum puanı
        public int MeetingGroupId { get; set; } // Ödevin atandığı grubun ID'si
        public bool? IsActive { get; set; } // Ödevin atandığı grubun ID'si
    }

    public class CreateTaskAssigmentCommandHandler : IRequestHandler<CreateOrEditTaskAssignmentCommand, ResultSingle<TaskAssignmentDto>>
    {
        private readonly IRepository<TaskAssignment> _taskAssigmentRepository;
        private readonly IMapper _mapper;

        public CreateTaskAssigmentCommandHandler(IRepository<TaskAssignment> taskAssigmentRepository, IMapper mapper)
        {
            _taskAssigmentRepository = taskAssigmentRepository;
            _mapper = mapper;
        }

        public async Task<ResultSingle<TaskAssignmentDto>> Handle(CreateOrEditTaskAssignmentCommand request, CancellationToken cancellationToken)
        {
            TaskAssignment taskAssignment;

            if (request.Id == null)
            {
                // Yeni kayıt oluşturma
                taskAssignment = new TaskAssignment
                {
                    Title = request.Title,
                    Description = request.Description,
                    DueDate = request.DueDate,
                    LateDueDate = request.LateDueDate,
                    MaxGrade = request.MaxGrade,
                    MeetingGroupId = request.MeetingGroupId,
                    CreatedTime = DateTime.UtcNow,
                    CreatedBy = request.AppUserId,
                    IsActive = 0
                };

                _taskAssigmentRepository.InsertWithoutCommit(taskAssignment);
                await _taskAssigmentRepository.CommitAsync(cancellationToken);
            }
            else
            {
                // Var olan kaydı güncelleme
                taskAssignment = _taskAssigmentRepository.GetById(request.Id.Value);

                if (taskAssignment == null)
                {
                    return Result.Fail<TaskAssignmentDto>(null, "Task bulunamadı.");
                }

                taskAssignment.Title = request.Title;
                taskAssignment.Description = request.Description;
                taskAssignment.DueDate = request.DueDate;
                taskAssignment.LateDueDate = request.LateDueDate;
                taskAssignment.MaxGrade = request.MaxGrade;
                taskAssignment.MeetingGroupId = request.MeetingGroupId;
                taskAssignment.UpdatedTime = DateTime.UtcNow;
                taskAssignment.CreatedBy = request.AppUserId;
                taskAssignment.AllowLateSubmissions = request.AllowLateSubmissions;
                taskAssignment.IsActive = (short)(request.IsActive ?? false ? 1 : 0);

                _taskAssigmentRepository.UpdateWithoutCommit(taskAssignment);
                await _taskAssigmentRepository.CommitAsync(cancellationToken);
            }

            var taskAssignmentDto = _mapper.Map<TaskAssignmentDto>(taskAssignment);
            taskAssignmentDto.Id = request.Id == null ? taskAssignment.Id : request.Id.Value;
            return Result.Ok<TaskAssignmentDto>(taskAssignmentDto, $"Kayıt oluşturuldu");
        }
    }
}
