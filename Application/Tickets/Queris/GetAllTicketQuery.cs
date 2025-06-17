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
    public class GetAllTicketQuery : IRequest<Result<TicketsAndMessagesDto>>
    {
        public int AppUserId { get; set; }
    }

    public class GetAllTicketQueryHandler : IRequestHandler<GetAllTicketQuery, Result<TicketsAndMessagesDto>>
    {
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IMapper _mapper;
        private readonly ITimeZoneService _timeZoneService;

        public GetAllTicketQueryHandler(IRepository<Ticket> ticketRepository, IMapper mapper, ITimeZoneService timeZoneService)
        {
            _ticketRepository = ticketRepository;
            _mapper = mapper;
            _timeZoneService = timeZoneService;
        }

        public async Task<Result<TicketsAndMessagesDto>> Handle(GetAllTicketQuery request, CancellationToken cancellationToken)
        {
            var ticketsAndMessages = _ticketRepository.GetMany(a => a.CreatedBy == request.AppUserId && a.IsActive == 1);

            var userTimeZoneId = await _timeZoneService.GetUserTimeZoneAsync(request.AppUserId);

            var data = await ticketsAndMessages
                .ProjectTo<TicketsAndMessagesDto>(_mapper.ConfigurationProvider)
                .OrderByDescending(a => a.Id)
                .ToListAsync(cancellationToken);

            // Kullanıcının zaman dilimine göre tarih ve saatleri dönüştür
            foreach (var ticket in data)
            {
                if (ticket.CreatedTime.HasValue)
                    ticket.CreatedTime = _timeZoneService.ConvertToUserTimeZone(ticket.CreatedTime.Value, userTimeZoneId);

                if (ticket.ClosedDate.HasValue)
                    ticket.ClosedDate = _timeZoneService.ConvertToUserTimeZone(ticket.ClosedDate.Value, userTimeZoneId);

                foreach (var message in ticket.Messages)
                {
                    if (message.CreatedDate.HasValue)
                        message.CreatedDate = _timeZoneService.ConvertToUserTimeZone(message.CreatedDate.Value, userTimeZoneId);
                }
            }

            return Result.Ok(data.AsQueryable(), "Başarılı");
        }
    }

}
