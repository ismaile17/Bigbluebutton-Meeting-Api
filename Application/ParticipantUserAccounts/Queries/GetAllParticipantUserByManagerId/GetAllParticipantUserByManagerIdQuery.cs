using Application.ParticipantAccounts.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.ParticipantUserAccounts.Queries.GetAllParticipantUserByManagerIdQuery
{
    public class GetAllParticipantUserByManagerIdQuery : IRequest<Result<ParticipantUserDto>>
    {
        public int AppUserId { get; set; }
    }

    public class GetParticipantUserByManagerIdQueryHandler : IRequestHandler<GetAllParticipantUserByManagerIdQuery, Result<ParticipantUserDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public GetParticipantUserByManagerIdQueryHandler(UserManager<AppUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<Result<ParticipantUserDto>> Handle(GetAllParticipantUserByManagerIdQuery request, CancellationToken cancellationToken)
        {
            var managerId = request.AppUserId;
            var appUsers = await _userManager.Users
                .Include(u => u.ManagerParticipants)
                //.Include(u=>u.MeetingGroupUserLists)
                //    .ThenInclude(u => u.MeetingGroup)
                .Where(u => u.ManagerParticipants.Any(mp => mp.ManagerId == managerId))
                .ToListAsync(cancellationToken);

            var participantUserDtos = _mapper.Map<List<ParticipantUserDto>>(appUsers);

            // AutoMapper içinde direkt ManagerId filtresi uygulanamayacağı için, dönüşüm sonrası filtreleme yapılıyor
            participantUserDtos.ForEach(dto =>
                dto.ManagerParticipants = dto.ManagerParticipants
                    .Where(mp => mp.ManagerId == request.AppUserId).ToList());

            // AutoMapper kullanarak ParticipantUserDto listesini doldur

            return Result.Ok(participantUserDtos, "Başarılı");
        }
    }
}
