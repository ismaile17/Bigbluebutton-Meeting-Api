using Application.Emails.Commands.SendEmail;
using Application.Shared.Results;
using Application.Tickets.Models;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Tickets.Commands
{
    public class CreateTicketMessageCommand : IRequest<ResultSingle<TicketDto>>
    {
        public int AppUserId { get; set; }
        public int TicketId { get; set; }
        public string Message { get; set; }
    }

    public class CreateTicketMessageCommandHandler : IRequestHandler<CreateTicketMessageCommand, ResultSingle<TicketDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IMapper _mapper;
        private readonly IMediator mediator;
        private readonly TelegramService _telegramService;

        public CreateTicketMessageCommandHandler(UserManager<AppUser> userManager, IRepository<Ticket> ticketRepository, IMapper mapper, IMediator mediator, TelegramService telegramService)
        {
            _userManager = userManager;
            _ticketRepository = ticketRepository;
            _mapper = mapper;
            this.mediator = mediator;
            _telegramService = telegramService;
        }

        public async Task<ResultSingle<TicketDto>> Handle(CreateTicketMessageCommand request, CancellationToken cancellationToken)
        {
            var ticket = _ticketRepository.Get(t => t.Id == request.TicketId, t => t.Messages);
            ticket.UpdatedTimeForMessage = DateTime.UtcNow;
            ticket.UpdatedTime = DateTime.Now;
            if (ticket == null)
            {
                return Result.Fail<TicketDto>(null, "Ticket bulunamadı.");
            }


            if (ticket.StatusId == 1) // Kapalı ticket ID'sini belirle
            {
                if (ticket.TicketStatusHistories == null)
                {
                    ticket.TicketStatusHistories = new List<TicketStatusHistory>();
                }
                // StatusHistory'ye kayıt ekleme
                var statusHistory = new TicketStatusHistory
                {
                    TicketId = request.TicketId,
                    Status = "Açık", // Açık ticket durumu ID'si
                    ChangedDate = DateTime.UtcNow,
                    ChangedByUserId = request.AppUserId
                };

                // Status history'yi ekle
                ticket.TicketStatusHistories.Add(statusHistory);

                // Ticket'ın durumunu güncelle
                ticket.StatusId = 0;
                ticket.Status = "Açık"; // Varsa duruma uygun string
            }

            // 5. Mesajı atan kişiye göre doğru e-postayı belirleyip gönder
            string recipientEmail = string.Empty;
            string recipientFullName = string.Empty;
            int createdId = 0;

            if (request.AppUserId == ticket.CreatedBy)
            {
                // Mesajı oluşturan kişi CreatedBy ise, AssignedUser'a mail gönder
                var assignedUser = await _userManager.FindByIdAsync(ticket.AssignedUserId.ToString());
                if (assignedUser != null)
                {
                    recipientEmail = RemoveParticipantSuffix(assignedUser.Email);
                    recipientFullName = assignedUser.FullName; // İstediğiniz şekilde fullname alınabilir
                    createdId = assignedUser.Id; // İstediğiniz şekilde fullname alınabilir
                }
            }
            else if (request.AppUserId == ticket.AssignedUserId)
            {
                // Mesajı oluşturan kişi AssignedUser ise, CreatedBy'e mail gönder
                var createdByUser = await _userManager.FindByIdAsync(ticket.CreatedBy.ToString());
                if (createdByUser != null)
                {
                    recipientEmail = RemoveParticipantSuffix(createdByUser.Email);
                    recipientFullName = createdByUser.FullName; // İstediğiniz şekilde fullname alınabilir
                    createdId = createdByUser.Id; // İstediğiniz şekilde fullname alınabilir

                }
            }

            bool wasRead = false;

            if(createdId == ticket.CreatedBy)
            {
                wasRead = true;
                ticket.WasReadTicketAdmin = true;
            }
            else
            {
                ticket.WasReadTicket = false;
                ticket.WasReadTicketAdmin = false;
            }

            // 2. Yeni mesajı oluştur
            var newMessage = new TicketMessage
            {
                TicketId = request.TicketId,
                MessageText = request.Message,
                CreatedByUserId = request.AppUserId,
                CreatedDate = DateTime.UtcNow,
                WasRead = wasRead
            };

            ticket.Messages.Add(newMessage);

            _ticketRepository.UpdateWithoutCommit(ticket);  
            var result = await _ticketRepository.CommitAsync(cancellationToken);
            if (result == -1)
            {
                return Result.Fail<TicketDto>(null, "Mesaj kaydedilemedi.");
            }



            //if (!string.IsNullOrEmpty(recipientEmail))
            //{
            //    // E-posta gönderme işlemi
            //    await mediator.Send(new SendEmailCommand
            //    {
            //        Type = 1,
            //        Email = recipientEmail,
            //        FullName = recipientFullName,
            //        Subject = "Yeni Bir Ticket Yanıtınız Var!",
            //        ButtonText = "Mesajları Görüntüle",
            //        Url = "https://tomeets.com",
            //        Body = "Yeni bir ticket yanıtı aldınız. Detaylar için lütfen tıklayın."
            //    });
            //}

            if(request.AppUserId == ticket.CreatedBy)
                _ = _telegramService.SendMessageAsync($"TICKET MESSAGE:\r\n Kişi: {request.AppUserId} \r\n Kategori: {ticket.CategoryName} \r\n Başlık: {ticket.Title} \r\n Mesaj: {request.Message}");


            var ticketDto = _mapper.Map<TicketDto>(ticket);
            return Result.Ok<TicketDto>(ticketDto, "Mesaj başarıyla eklendi.");
        }

        private string RemoveParticipantSuffix(string email)
        {
            // E-posta adresinin sonunda _PARTICIPANT varsa kaldır
            if (email.EndsWith("_PARTICIPANT", StringComparison.OrdinalIgnoreCase))
            {
                return email.Substring(0, email.Length - "_PARTICIPANT".Length);
            }
            return email;
        }
    }

}