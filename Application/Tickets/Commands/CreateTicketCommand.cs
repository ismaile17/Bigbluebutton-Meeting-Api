using Application.Groups.Model;
using Application.Shared.Results;
using Application.Tickets.Models;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Migrations;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Tickets.Commands
{
    public class CreateTicketCommand : IRequest<ResultSingle<TicketDto>>
    {
        public int AppUserId { get; set; }
        public string Title { get; set; }
        public string Priority { get; set; }
        public int? CategoryId { get; set; }
        public string Message { get; set; }
    }

    public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, ResultSingle<TicketDto>>
    {
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IMapper _mapper;
        private readonly TelegramService _telegramService;

        public CreateTicketCommandHandler(IRepository<Ticket> ticketRepository, IMapper mapper, TelegramService telegramService)
        {
            _ticketRepository = ticketRepository;
            _mapper = mapper;
            _telegramService = telegramService;
        }

        public async Task<ResultSingle<TicketDto>> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
        {
            // CategoryName'yi CategoryId'ye göre ayarlama
            string categoryName = request.CategoryId switch
            {
                1 => "Teknik",
                2 => "Finansal",
                3 => "Genel",
                _ => "Belirtilmemiş" // Eğer başka bir değer gelirse varsayılan değer
            };

            // 1. Ticket oluşturma
            var ticket = new Ticket
            {
                Title = request.Title,
                CreatedTime = DateTime.UtcNow,
                Status = "Açık", // Default olarak Açık
                Priority = request.Priority,
                CreatedBy = request.AppUserId,
                CategoryId = request.CategoryId,
                CategoryName = categoryName,
                IsActive = 1,
                AssignedUserId = 2, // Varsayılan atanmış kullanıcı
                Messages = new List<TicketMessage>(), // Mesajlar listesini başlatıyoruz
                TicketStatusHistories = new List<TicketStatusHistory>() // Statü güncellemeleri listesini başlatıyoruz
            };

            // 2. İlk mesajı ekleme
            var initialMessage = new TicketMessage
            {
                MessageText = request.Message,
                CreatedByUserId = request.AppUserId, // Mesajı oluşturan kullanıcı
                Ticket = ticket // Ticket ile ilişkilendiriliyor
            };

            ticket.Messages.Add(initialMessage); // Tek bir mesaj ekleniyor

            // 3. İlk status güncellemesi ekleme
            var statusHistory = new TicketStatusHistory
            {
                Status = "Açık", // Default olarak Açık
                ChangedDate = DateTime.UtcNow,
                ChangedByUserId = request.AppUserId // Statü güncelleyen kullanıcı
            };

            ticket.TicketStatusHistories.Add(statusHistory); // Tek bir status ekleniyor

            // 4. Ticket ve ilişkili alt tabloları kaydetme
            _ticketRepository.InsertWithoutCommit(ticket);

            // 5. Tüm değişiklikleri veritabanına kaydetme (Commit işlemi)
            var result = await _ticketRepository.CommitAsync(cancellationToken);
            if (result == -1)
            {
                return Result.Fail<TicketDto>(null, "Kayıt edilemedi.");
            }

            _ = _telegramService.SendMessageAsync($"NEW TICKET:\r\n Kişi: {request.AppUserId} \r\n Kategori: {ticket.CategoryName} \r\n Başlık: {request.Title} \r\n Mesaj: {request.Message}");


            // 6. Mapping ve dönüş işlemi
            var data = _mapper.Map<TicketDto>(ticket);
            return Result.Ok<TicketDto>(data, "Ticket başarıyla oluşturuldu.");
        }
    }
}
