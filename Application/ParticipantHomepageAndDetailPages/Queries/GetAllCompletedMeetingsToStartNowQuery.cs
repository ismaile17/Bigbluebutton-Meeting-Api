using Application.ParticipantHomepageAndDetailPages.Models;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Enum;
using System.Linq;

namespace Application.ParticipantHomepageAndDetailPages.Queries
{
    public class GetAllCompletedMeetingsToStartNowQuery : IRequest<Result<CompletedMeetingsToStartNowDto>>
    {
        public int AppUserId { get; set; }
    }

    public class GetAllCompletedMeetingsToStartNowQueryHandler : IRequestHandler<GetAllCompletedMeetingsToStartNowQuery, Result<CompletedMeetingsToStartNowDto>>
    {
        private readonly IRepository<Domain.Entities.CompletedMeeting> _completedRepository;
        private readonly IRepository<Meeting> _meetingRepository;
        private readonly IRepository<ManagerParticipant> _managerParticipantRepository;
        private readonly IMapper _mapper;

        public GetAllCompletedMeetingsToStartNowQueryHandler(
            IRepository<Domain.Entities.CompletedMeeting> completedRepository,
            IRepository<Meeting> meetingRepository,
            IRepository<ManagerParticipant> managerParticipantRepository,
            IMapper mapper)
        {
            _completedRepository = completedRepository;
            _meetingRepository = meetingRepository;
            _managerParticipantRepository = managerParticipantRepository;
            _mapper = mapper;
        }

        public async Task<Result<CompletedMeetingsToStartNowDto>> Handle(GetAllCompletedMeetingsToStartNowQuery request, CancellationToken cancellationToken)
        {
            // 1. Kullanıcının aktif olduğu ManagerParticipant kayıtlarını al
            var activeManagerParticipants = await _managerParticipantRepository.GetMany(mp =>
                mp.ParticipantId == request.AppUserId &&
                mp.IsActive == true &&
                (
                    mp.ExpiryDateIsActive != true ||
                    (mp.ExpiryDate != null && mp.ExpiryDate >= DateTime.UtcNow)
                ))
                .ToListAsync(cancellationToken);

            var managerIds = activeManagerParticipants.Select(mp => mp.ManagerId).Distinct().ToList();

            // 2. Kullanıcının dahil olduğu ve aktif yöneticilerin oluşturduğu MeetingGroup'ların MeetingId'lerini al
            var meetingIds = await _meetingRepository.GetAll
                .Include(m => m.MeetingMeetingGroups)
                    .ThenInclude(mmg => mmg.MeetingGroup)
                        .ThenInclude(mg => mg.MeetingGroupUserLists)
                .Where(m => m.MeetingMeetingGroups.Any(mmg =>
                    mmg.MeetingGroup.CreatedBy.HasValue && // CreatedBy null değilse
                    managerIds.Contains(mmg.MeetingGroup.CreatedBy.Value) && // CreatedBy'ın değeri managerIds'de varsa
                    mmg.MeetingGroup.MeetingGroupUserLists.Any(mgul => mgul.AppUserId == request.AppUserId)
                ))
                .Select(m => m.Id)
                .ToListAsync(cancellationToken);


            // 3. Tamamlanan toplantıları al
            var completedMeetings = await _completedRepository.GetMany(cm =>
                cm.CreateStartOrFinish == CompletedMeetingCreateStartOrFinish.Start &&
                meetingIds.Contains(cm.MeetingId))
                .ToListAsync(cancellationToken);

            // 4. DTO'ya dönüştür ve sonucu döndür
            var result = completedMeetings.Select(cm => new CompletedMeetingsToStartNowDto
            {
                MeetingId = cm.MeetingId,
                MeetingName = cm.Name,
                MeetingDescription = cm.Description,
                CompletedMeetingGuid = cm.CompletedGuid.ToString(),
                GuestPolicy = cm.GuestPolicy,
                CreateStartOrFinish = cm.CreateStartOrFinish
            })
            .OrderByDescending(dto => dto.MeetingId)
            .AsQueryable();

            return Result.Ok(result, "Başarılı");
        }
    }

}
