using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Tickets.Models
{
    public class OnlyTicketAndMessageDto : IMapFrom<Ticket>
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime? ClosedDate { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public bool WasReadTicket { get; set; }
        public int CreatedBy { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public List<TicketMessageDto> Messages { get; set; }


        public void Mapping(Profile profile)
        {
            profile.CreateMap<Ticket, OnlyTicketAndMessageDto>()
                .ForMember(a => a.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(a => a.Title, opt => opt.MapFrom(s => s.Title))
                .ForMember(a => a.ClosedDate, opt => opt.MapFrom(s => s.ClosedDate))
                .ForMember(a => a.CreatedTime, opt => opt.MapFrom(s => s.CreatedTime))
                .ForMember(a => a.Status, opt => opt.MapFrom(s => s.Status))
                .ForMember(a => a.Priority, opt => opt.MapFrom(s => s.Priority))
                .ForMember(a => a.CreatedBy, opt => opt.MapFrom(s => s.CreatedBy))
                .ForMember(a => a.CategoryId, opt => opt.MapFrom(s => s.CategoryId))
                .ForMember(a => a.WasReadTicket, opt => opt.MapFrom(s => s.WasReadTicket))
                .ForMember(a => a.CategoryName, opt => opt.MapFrom(s => s.CategoryName))
                .ForMember(dest => dest.Messages, opt => opt.MapFrom(src => src.Messages)) // Mesajları TicketDto'ya dahil et
                ;

            ;
        }
    }
}
