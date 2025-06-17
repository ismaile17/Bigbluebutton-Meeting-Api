using Application.Shared.Results;
using Application.Tickets.Models;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;

namespace Application.Tickets.Commands
{
    public class CreateCategoryOrPriorityOrTicketChangeCommand : IRequest<ResultSingle<TicketDto>>
    {
        public int AppUserId { get; set; }
        public int TicketId { get; set; }
        public int StatusId { get; set; }
        public string Priority { get; set; }
        public int CategoryId { get; set; }
    }

    public class CreatePriorityOrTicketClosedCommandHandler : IRequestHandler<CreateCategoryOrPriorityOrTicketChangeCommand, ResultSingle<TicketDto>>
    {
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IMapper _mapper;

        public CreatePriorityOrTicketClosedCommandHandler(IRepository<Ticket> ticketRepository, IMapper mapper)
        {
            _ticketRepository = ticketRepository;
            _mapper = mapper;
        }

        public async Task<ResultSingle<TicketDto>> Handle(CreateCategoryOrPriorityOrTicketChangeCommand request, CancellationToken cancellationToken)
        {
            // Admin kontrolü
            if (request.AppUserId != 2)
                return Result.Fail<TicketDto>(null, "Admin bulunamadı.");

            // TicketId ile bilet sorgulama
            var ticket = _ticketRepository.Get(t => t.Id == request.TicketId);
            if (ticket == null)
                return Result.Fail<TicketDto>(null, "Ticket bulunamadı.");

            // Priority güncellemesi
            if (!string.IsNullOrEmpty(request.Priority))
                ticket.Priority = request.Priority;

            // CategoryId güncellemesi
            if (request.CategoryId > 0)
                ticket.CategoryName = request.CategoryId switch
                {
                    1 => "Teknik",
                    2 => "Finansal",
                    3 => "Genel",
                    _ => "Belirtilmemiş"
                };

            // StatusId güncellemesi ve statusHistory kaydı
            if (request.StatusId == 0 || request.StatusId == 1)
            {
                ticket.Status = request.StatusId == 0 ? "Açık" : "Kapalı";
                ticket.StatusId = request.StatusId;

                if (request.StatusId == 1)
                {
                    ticket.WasReadTicketAdmin = true;

                }
                else
                {
                    ticket.WasReadTicketAdmin = false;
                }

                ticket.TicketStatusHistories ??= new List<TicketStatusHistory>(); // Null kontrolü ve liste oluşturma
                ticket.TicketStatusHistories.Add(new TicketStatusHistory
                {
                    TicketId = request.TicketId,
                    Status = ticket.Status,
                    ChangedDate = DateTime.UtcNow,
                    ChangedByUserId = 2
                });
            }
            else
            {
                return Result.Fail<TicketDto>(null, "Geçersiz StatusId. Sadece 0 (Açık) veya 1 (Kapalı) değerleri kabul edilir.");
            }

            // Güncelle ve değişiklikleri kaydet
            _ticketRepository.Update(ticket);
            await _ticketRepository.CommitAsync(cancellationToken);

            // DTO'ya mapleyip sonucu döndür
            var ticketDto = _mapper.Map<TicketDto>(ticket);
            return Result.Ok(ticketDto, "Ticket başarıyla güncellendi.");
        }
    }
}
