using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.TransferPaymentAndInvoice.Model
{
    public class MoneyTransferFormDto : IMapFrom<MoneyTransferForm>
    {
        public int AppUserId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PhoneNumber { get; set; }
        public string? AdminNote { get; set; }
        public int PackageId { get; set; }
        public int MoneyTransferFormId { get; set; }
        public bool? Success { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<MoneyTransferForm, MoneyTransferFormDto>()
                .ForMember(a => a.AppUserId, opt => opt.MapFrom(s => s.CreatedBy))
                .ForMember(a => a.Name, opt => opt.MapFrom(s => s.Name))
                .ForMember(a => a.Surname, opt => opt.MapFrom(s => s.Surname))
                .ForMember(a => a.PhoneNumber, opt => opt.MapFrom(s => s.PhoneNumber))
                .ForMember(a => a.AdminNote, opt => opt.MapFrom(s => s.AdminNote))
                .ForMember(a => a.PackageId, opt => opt.MapFrom(s => s.PackageId))
                .ForMember(a => a.MoneyTransferFormId, opt => opt.MapFrom(s => s.Id))
                .ForMember(a => a.Success, opt => opt.MapFrom(s => s.Success))
                ;
        }
    }
}
