// Application/ParticipantHomepageAndDetailPages/Queries/GetActiveGroupsForUserQuery.cs

using Application.ParticipantHomepageAndDetailPages.Models;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ParticipantHomepageAndDetailPages.Queries
{
    public class GetActiveGroupsForUserQuery : IRequest<Result<ActiveGroupDto>>
    {
        public int AppUserId { get; set; }
    }

    public class GetActiveGroupsForUserQueryHandler : IRequestHandler<GetActiveGroupsForUserQuery, Result<ActiveGroupDto>>
    {
        private readonly IRepository<ManagerParticipant> _managerParticipantRepository;
        private readonly IRepository<MeetingGroup> _meetingGroupRepository;
        private readonly IMapper _mapper;

        public GetActiveGroupsForUserQueryHandler(
            IRepository<ManagerParticipant> managerParticipantRepository,
            IRepository<MeetingGroup> meetingGroupRepository,
            IMapper mapper)
        {
            _managerParticipantRepository = managerParticipantRepository;
            _meetingGroupRepository = meetingGroupRepository;
            _mapper = mapper;
        }

        public async Task<Result<ActiveGroupDto>> Handle(GetActiveGroupsForUserQuery request, CancellationToken cancellationToken)
        {
            // 1. Kullanıcının aktif olduğu ManagerParticipant kayıtlarını al
            var activeManagerParticipants = await _managerParticipantRepository.GetMany(mp =>
                mp.ParticipantId == request.AppUserId &&
                mp.IsActive == true &&
                (
                    mp.ExpiryDateIsActive != true ||
                    (mp.ExpiryDate.HasValue && mp.ExpiryDate.Value >= DateTime.Now)
                ))
                .ToListAsync(cancellationToken);

            // 2. ManagerId'leri al
            var managerIds = activeManagerParticipants.Select(mp => mp.ManagerId).Distinct().ToList();

            // 3. Bu ManagerId'lere ait MeetingGroup'ları al ve kullanıcının UserList'inde olup olmadığını kontrol et
            var activeGroups = await _meetingGroupRepository.GetMany(mg =>
                managerIds.Contains(mg.UserId))
                .Include(mg => mg.MeetingGroupUserLists)
                .Include(mg => mg.TaskAssignments)
                .Where(mg => mg.MeetingGroupUserLists.Any(mgul => mgul.AppUserId == request.AppUserId))
                .ToListAsync(cancellationToken);

            // 4. DTO'ya manuel olarak dönüştür
            var activeGroupDtos = activeGroups.Select(mg => new ActiveGroupDto
            {
                Id = mg.Id,
                Name = mg.Name,
                Description = mg.Description,
                ActiveAssignmentCount = mg.TaskAssignments.Count(ta =>
                    (
                        (ta.AllowLateSubmissions == true && ta.LateDueDate != null && ta.LateDueDate >= DateTime.UtcNow)
                        ||
                        (ta.AllowLateSubmissions != true && ta.DueDate >= DateTime.UtcNow)
                    )
                ),
                // Diğer alanlar...
            }).ToList();

            // 5. Sonucu döndür
            return Result.Ok(activeGroupDtos.AsQueryable(), "Başarılı");
        }

    }
}
