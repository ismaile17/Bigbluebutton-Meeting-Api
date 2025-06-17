using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;


namespace Application.GeneralAdmin.GAServer.Model
{
    public class BBBServerDto : IMapFrom<BBBServer>
    {
        public int? Id { get; set; }
        public bool? MainServer { get; set; } = false;
        public string? ServerName { get; set; }
        public string? ServerDetail { get; set; }
        public string? Notes { get; set; }
        public string? BuyPrice { get; set; }
        public string? BuyCompany { get; set; }
        public string? ServerLocation { get; set; }
        public string? IpAdress { get; set; }
        public string? ServerApiUrl { get; set; }
        public string? SharedSecret { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<BBBServer, BBBServerDto>()
                .ForMember(a => a.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(a => a.MainServer, opt => opt.MapFrom(s => s.MainServer))
                .ForMember(a => a.ServerName, opt => opt.MapFrom(s => s.ServerName))
                .ForMember(a => a.ServerDetail, opt => opt.MapFrom(s => s.ServerDetail))
                .ForMember(a => a.Notes, opt => opt.MapFrom(s => s.Notes))
                .ForMember(a => a.BuyPrice, opt => opt.MapFrom(s => s.BuyPrice))
                .ForMember(a => a.BuyCompany, opt => opt.MapFrom(s => s.BuyCompany))
                .ForMember(a => a.ServerLocation, opt => opt.MapFrom(s => s.ServerLocation))
                .ForMember(a => a.IpAdress, opt => opt.MapFrom(s => s.IpAdress))
                .ForMember(a => a.ServerApiUrl, opt => opt.MapFrom(s => s.ServerApiUrl))
                .ForMember(a => a.SharedSecret, opt => opt.MapFrom(s => s.SharedSecret));
        }
    }
}
