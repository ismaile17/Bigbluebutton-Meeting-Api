using Application.Accounts.Services;
using Application.Shared.Results;
using Application.Tickets.Models;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Tickets.Queris
{
    public class GetOnlyTicketAndMessageQuery : IRequest<ResultSingle<OnlyTicketAndMessageDto>>
    {
        public int AppUserId { get; set; }
        public int TicketId { get; set; }
    }

    public class GetOnlyTicketAndMessageQueryHandler : IRequestHandler<GetOnlyTicketAndMessageQuery, ResultSingle<OnlyTicketAndMessageDto>>
    {
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IMapper _mapper;
        private readonly ITimeZoneService _timeZoneService;

        public GetOnlyTicketAndMessageQueryHandler(IRepository<Ticket> ticketRepository, IMapper mapper, ITimeZoneService timeZoneService)
        {
            _ticketRepository = ticketRepository;
            _mapper = mapper;
            _timeZoneService = timeZoneService;
        }

        public async Task<ResultSingle<OnlyTicketAndMessageDto>> Handle(GetOnlyTicketAndMessageQuery request, CancellationToken cancellationToken)
        {
            // Tek bir ticket ve mesajlarını sorgulama
            var ticket = await _ticketRepository.GetMany(t =>
                    t.Id == request.TicketId &&
                    t.IsActive == 1)
                .Include(t => t.Messages) // Mesajları dahil et
                .FirstOrDefaultAsync(cancellationToken); // İlk uygun kaydı al

            if (ticket == null)
            {
                return Result.Fail<OnlyTicketAndMessageDto>(null, "Ticket bulunamadı.");
            }

            if (ticket.CreatedBy != request.AppUserId && request.AppUserId != 2)
            {
                return Result.Fail<OnlyTicketAndMessageDto>(null, "Yetkili değilsiniz bulunamadı.");
            }

            // Mesajların `WasRead` alanını güncelle
            if (request.AppUserId == ticket.CreatedBy)
            {
                ticket.WasReadTicket = true;
                ticket.WasReadTicketAdmin = false;
            }
            else
            {
                ticket.WasReadTicket = false;
            }

            // Değişiklikleri veritabanına yansıt
            _ticketRepository.UpdateWithoutCommit(ticket);
            var result = await _ticketRepository.CommitAsync(cancellationToken);
            if (result == -1)
            {
                return Result.Fail<OnlyTicketAndMessageDto>(null, "Mesaj durumu güncellenemedi.");
            }

            // Kullanıcının zaman dilimini al
            var userTimeZoneId = await _timeZoneService.GetUserTimeZoneAsync(request.AppUserId);

            // DTO'ya mapleme
            var ticketDto = _mapper.Map<OnlyTicketAndMessageDto>(ticket);

            // Zaman dönüşümünü uygula
            if (ticketDto.CreatedTime.HasValue)
            {
                ticketDto.CreatedTime = _timeZoneService.ConvertToUserTimeZone(ticketDto.CreatedTime.Value, userTimeZoneId);
            }

            if (ticketDto.ClosedDate.HasValue)
            {
                ticketDto.ClosedDate = _timeZoneService.ConvertToUserTimeZone(ticketDto.ClosedDate.Value, userTimeZoneId);
            }

            foreach (var message in ticketDto.Messages)
            {
                if (message.CreatedDate.HasValue)
                {
                    message.CreatedDate = _timeZoneService.ConvertToUserTimeZone(message.CreatedDate.Value, userTimeZoneId);
                }
            }

            return Result.Ok(ticketDto, "Başarılı");
        }
    }

}
