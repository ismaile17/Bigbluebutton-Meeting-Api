using Application.Accounts.Services;
using Application.Shared.Results;
using Application.Tickets.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Tickets.Queris
{
    public class GetAllOnlyTicketsQuery : IRequest<Result<TicketDto>>
    {
        public int AppUserId { get; set; }
    }

    public class GetAllOnlyTicketsQueryHandler : IRequestHandler<GetAllOnlyTicketsQuery, Result<TicketDto>>
    {
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IMapper _mapper;
        private readonly ITimeZoneService _timeZoneService;

        public GetAllOnlyTicketsQueryHandler(IRepository<Ticket> ticketRepository, IMapper mapper, ITimeZoneService timeZoneService)
        {
            _ticketRepository = ticketRepository;
            _mapper = mapper;
            _timeZoneService = timeZoneService;
        }

        public async Task<Result<TicketDto>> Handle(GetAllOnlyTicketsQuery request, CancellationToken cancellationToken)
        {
            var tickets = _ticketRepository.GetMany(a => a.CreatedBy == request.AppUserId && a.IsActive == 1);

            var userTimeZoneId = await _timeZoneService.GetUserTimeZoneAsync(request.AppUserId);

            var data = await tickets
                .ProjectTo<TicketDto>(_mapper.ConfigurationProvider)
                .OrderByDescending(a => a.Id)
                .ToListAsync(cancellationToken);

            // Kullanıcının zaman dilimine göre tarih ve saatleri dönüştür
            foreach (var ticket in data)
            {
                if (ticket.CreatedTime.HasValue)
                    ticket.CreatedTime = _timeZoneService.ConvertToUserTimeZone(ticket.CreatedTime.Value, userTimeZoneId);

                if (ticket.ClosedDate.HasValue)
                    ticket.ClosedDate = _timeZoneService.ConvertToUserTimeZone(ticket.ClosedDate.Value, userTimeZoneId);

                if (ticket.UpdatedTime.HasValue)
                    ticket.UpdatedTime = _timeZoneService.ConvertToUserTimeZone(ticket.UpdatedTime.Value, userTimeZoneId);
            }

            return Result.Ok(data.AsQueryable(), "Başarılı");
        }
    }

}
