using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Tickets.Models
{
    public class TicketMessageDto : IMapFrom<TicketMessage>
    {
        public int Id { get; set; }
        public string MessageText { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int CreatedByUserId { get; set; }


        public void Mapping(Profile profile)
        {
            profile.CreateMap<TicketMessage, TicketMessageDto>()
                .ForMember(a => a.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(a => a.MessageText, opt => opt.MapFrom(s => s.MessageText))
                .ForMember(a => a.CreatedDate, opt => opt.MapFrom(s => s.CreatedDate))
                .ForMember(a => a.CreatedByUserId, opt => opt.MapFrom(s => s.CreatedByUserId))
                ;
        }
    }
}