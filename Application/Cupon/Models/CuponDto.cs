using Application.Packages.Model;
using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Cupon.Models
{
    public class CuponDto : IMapFrom<Domain.Entities.Cupon>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string DiscountType { get; set; } // "Percentage", "Fixed", "MinimumOrderPercentage", "MinimumOrderFixed"
        public decimal DiscountValue { get; set; }
        public decimal? MinimumOrderValue { get; set; }
        public int UsageLimit { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int UsedCount { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Domain.Entities.Cupon, CuponDto>()
                .ForMember(a => a.Code, opt => opt.MapFrom(s => s.Code))
                .ForMember(a => a.Name, opt => opt.MapFrom(s => s.Name))
                .ForMember(a => a.DiscountType, opt => opt.MapFrom(s => s.DiscountType))
                .ForMember(a => a.DiscountValue, opt => opt.MapFrom(s => s.DiscountValue))
                .ForMember(a => a.MinimumOrderValue, opt => opt.MapFrom(s => s.MinimumOrderValue))
                .ForMember(a => a.UsageLimit, opt => opt.MapFrom(s => s.UsageLimit))
                .ForMember(a => a.ExpiryDate, opt => opt.MapFrom(s => s.ExpiryDate))
                .ForMember(a => a.UsedCount, opt => opt.MapFrom(s => s.UsedCount))
                ;
        }

    }

}
