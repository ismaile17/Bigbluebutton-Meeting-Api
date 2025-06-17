using Application.Accounts.Services;
using Application.Shared.Results;
using Application.TaskSystem.Model;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.TaskSystem.Queries
{
    public class GetAllTaskAssignmentByGroupIdQuery : IRequest<Result<TaskAssignmentDto>>
    {
        public int GroupId { get; set; }
        public int AppUserId { get; set; }
    }

    public class TaskAssignmentByUserIdQueryHandler : IRequestHandler<GetAllTaskAssignmentByGroupIdQuery, Result<TaskAssignmentDto>>
    {
        private readonly IRepository<TaskAssignment> _taskAssignmentRepository;
        private readonly IMapper _mapper;
        private readonly ITimeZoneService _timeZoneService;

        public TaskAssignmentByUserIdQueryHandler(IRepository<TaskAssignment> taskAssignmentRepository, IMapper mapper, ITimeZoneService timeZoneService)
        {
            _taskAssignmentRepository = taskAssignmentRepository;
            _mapper = mapper;
            _timeZoneService = timeZoneService;
        }

        public async Task<Result<TaskAssignmentDto>> Handle(GetAllTaskAssignmentByGroupIdQuery request, CancellationToken cancellationToken)
        {
            // Görevleri sorgula
            var tasks = _taskAssignmentRepository.GetMany(a =>
                a.CreatedBy == request.AppUserId &&
                a.MeetingGroupId == request.GroupId &&
                a.IsActive != -1);

            // Kullanıcının zaman dilimini al
            var userTimeZoneId = await _timeZoneService.GetUserTimeZoneAsync(request.AppUserId);

            // DTO'ya dönüştür ve zaman dönüşümünü uygula
            var data = await tasks
                .ProjectTo<TaskAssignmentDto>(_mapper.ConfigurationProvider)
                .OrderByDescending(a => a.Id)
                .ToListAsync(cancellationToken);

            foreach (var task in data)
            {
                if (task.DueDate.HasValue)
                    task.DueDate = _timeZoneService.ConvertToUserTimeZone(task.DueDate.Value, userTimeZoneId);

                if (task.LateDueDate.HasValue)
                    task.LateDueDate = _timeZoneService.ConvertToUserTimeZone(task.LateDueDate.Value, userTimeZoneId);

                if (task.CreatedTime.HasValue)
                    task.CreatedTime = _timeZoneService.ConvertToUserTimeZone(task.CreatedTime.Value, userTimeZoneId);
            }

            return Result.Ok(data.AsQueryable(), "Başarılı");
        }
    }

}
