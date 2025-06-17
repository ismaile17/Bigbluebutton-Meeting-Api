using Application.ParticipantUserAccounts.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Application.ParticipantUserAccounts.Queries.GetSelectedGroupByUserId
{
    public class GetSelectedParticipantByUserIdQuery : IRequest<Result<SelectedParticipantDto>>
    {
        public int AppUserId { get; set; }
    }

    public class GetSelectedParticipantByUserIdQueryHandler : IRequestHandler<GetSelectedParticipantByUserIdQuery, Result<SelectedParticipantDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public GetSelectedParticipantByUserIdQueryHandler(UserManager<AppUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<Result<SelectedParticipantDto>> Handle(GetSelectedParticipantByUserIdQuery request, CancellationToken cancellationToken)
        {
            var managerId = request.AppUserId;
            var currentDate = DateTime.UtcNow;

            var managerParticipants = await _userManager.Users
                .Where(u => u.ManagerParticipants.Any(mp => mp.ManagerId == managerId))
                .SelectMany(u => u.ManagerParticipants)
                .Where(mp => mp.ManagerId == managerId && mp.IsActive == true &&
                             (mp.ExpiryDateIsActive != true || (mp.ExpiryDateIsActive == true && mp.ExpiryDate > currentDate)))
                .Select(mp => new SelectedParticipantDto
                {
                    Id = mp.ParticipantId,
                    FullName = mp.NameSurname,
                    Email = mp.Participant.Email,
                    PhoneNumber = mp.PhoneNumber
                })
                .ToListAsync(cancellationToken);

            return Result.Ok(managerParticipants, "Başarılı");
        }
    }
}
