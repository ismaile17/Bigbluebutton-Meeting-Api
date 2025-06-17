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
    public class GetAllOpenTicketsQueryAdmin : IRequest<Result<TicketDto>>
    {
        public int AppUserId { get; set; }
    }

    public class GetAllTicketsQueryAdminHandler : IRequestHandler<GetAllOpenTicketsQueryAdmin, Result<TicketDto>>
    {
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IMapper _mapper;
        private readonly ITimeZoneService _timeZoneService;

        public GetAllTicketsQueryAdminHandler(IRepository<Ticket> ticketRepository, IMapper mapper, ITimeZoneService timeZoneService)
        {
            _ticketRepository = ticketRepository;
            _mapper = mapper;
            _timeZoneService = timeZoneService;
        }

        public async Task<Result<TicketDto>> Handle(GetAllOpenTicketsQueryAdmin request, CancellationToken cancellationToken)
        {
            // Açık olan biletleri sorgula
            var ticketsAndMessages = _ticketRepository.GetMany(a => a.StatusId == 0 && a.IsActive == 1);

            // Kullanıcının zaman dilimini al
            var userTimeZoneId = await _timeZoneService.GetUserTimeZoneAsync(request.AppUserId);

            // DTO'ya dönüştür
            var data = await ticketsAndMessages
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
