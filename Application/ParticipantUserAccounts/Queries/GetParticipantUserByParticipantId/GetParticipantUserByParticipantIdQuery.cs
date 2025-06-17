using Application.Groups.Model;
using Application.ParticipantAccounts.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.ParticipantUserAccounts.Queries.GetParticipantUserByParticipantId
{
    public class GetParticipantUserByParticipantIdQuery : IRequest<ResultSingle<ParticipantUserDto>>
    {
        public int ParticipantId { get; set; }
        public int AppUserId { get; set; }
    }

    public class GetParticipantUserByParticipantIdQueryHandler : IRequestHandler<GetParticipantUserByParticipantIdQuery, ResultSingle<ParticipantUserDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        

        public GetParticipantUserByParticipantIdQueryHandler(UserManager<AppUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ResultSingle<ParticipantUserDto>> Handle(GetParticipantUserByParticipantIdQuery request, CancellationToken cancellationToken)
        {
            var appUser = await _userManager.Users
            .Include(u => u.ManagerParticipants)
            .Include(u=> u.MeetingGroupUserLists)
                .ThenInclude(u=> u.MeetingGroup)
            .FirstOrDefaultAsync(u => u.Id == request.ParticipantId);

            if (appUser == null)
            {   
                return Result.Fail<ParticipantUserDto>(null, "Belirtilen ParticipantId'ye sahip kullanıcı bulunamadı.");
            }

            var isManager = appUser.ManagerParticipants.Any(mp => mp.ManagerId == request.AppUserId);
            if (!isManager)
            {
                return Result.Fail<ParticipantUserDto>(null, "Belirtilen ParticipantId'ye sahip kullanıcı, belirtilen AppUserId'ye ait bir yönetici değil.");
            }

            // ManagerParticipants içinden sadece belirli bir ManagerId'ye sahip olanları filtrele
            var filteredManagerParticipants = appUser.ManagerParticipants
                .Where(mp => mp.ManagerId == request.AppUserId)
                .ToList();

            // AutoMapper ile AppUser'ı ParticipantUserDto'ya dönüştür
            var participantUserDto = _mapper.Map<ParticipantUserDto>(appUser);

            // Filtrelenmiş ManagerParticipants listesini DTO'ya manuel olarak ata
            participantUserDto.ManagerParticipants = filteredManagerParticipants
                .Select(mp => new ManagerParticipantDto
                {
                    ManagerId = mp.ManagerId,
                    ParticipantId = mp.ParticipantId,
                    NameSurname = mp.NameSurname,
                    PhoneNumber = mp.PhoneNumber,
                    SpecialDescription = mp.SpecialDescription,
                    IsActive = mp.IsActive ?? true,
                    ExpiryDate = mp.ExpiryDate ?? new DateTime(1900, 1, 1),
                    ExpiryDateIsActive = mp.ExpiryDateIsActive ?? false,
                    
                    // Diğer alanlar...
                }).ToList();

            return Result.Ok(participantUserDto, "Başarılı");
        }
    }
}