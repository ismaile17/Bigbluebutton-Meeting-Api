using Application.Meetings.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Meetings.Queries.GetMeetingByParticipantUserId
{
    public class GetMeetingByParticipantUserIdQuery : IRequest<Result<MeetingDto>>
    {
        public int AppUserId { get; set; }
    }

    public class GetMeetingByParticipantUserIdQueryHandler : IRequestHandler<GetMeetingByParticipantUserIdQuery, Result<MeetingDto>>
    {
        private readonly IRepository<Meeting> _meetingRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public GetMeetingByParticipantUserIdQueryHandler(IRepository<Meeting> meetingRepository, IMapper mapper, UserManager<AppUser> userManager)
        {
            _meetingRepository = meetingRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<Result<MeetingDto>> Handle(GetMeetingByParticipantUserIdQuery request, CancellationToken cancellationToken)
        {
            // AppUserId'ye sahip katılımcının yöneticilerini bul
            var appUserId = request.AppUserId;
            var appUser = await _userManager.Users
                .Include(u => u.ManagerParticipants)
                .SingleOrDefaultAsync(u => u.Id == appUserId, cancellationToken);

            if (appUser == null)
            {
                return Result.Fail<MeetingDto>("Belirtilen AppUserId'ye sahip kullanıcı bulunamadı.");
            }

            var managerIds = appUser.ManagerParticipants
                .Where(mp => mp.ParticipantId == appUserId && mp.IsActive ==true && (mp.ExpiryDate == null || mp.ExpiryDate > DateTime.Now)) // Sadece istenen katılımcıya ait yöneticileri seç
                .Select(mp => mp.ManagerId)
                .ToList();

            // Yöneticilerin oluşturduğu toplantıları bul
            var meetings = await _meetingRepository
                .GetMany(m => managerIds.Contains(m.UserId) && m.IsActive == 1)
                .Include(m => m.MeetingMeetingGroups)
                    .ThenInclude(mm => mm.MeetingGroup)
                .ToListAsync(cancellationToken);

            // Bulunan toplantıları MeetingDto'ya dönüştür
            var meetingDtos = _mapper.Map<List<MeetingDto>>(meetings);

            return Result.Ok(meetingDtos, "Başarılı");
        }
    }
}
