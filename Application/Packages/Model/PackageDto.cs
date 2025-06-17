using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Packages.Model
{
    public class PackageDto : IMapFrom<Package>
    {
        public int? ParentID { get; set; }
        public int? Duration { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }
        public int Price { get; set; }
        public string PriceCurrency { get; set; }
        public string SmsCountGift { get; set; }
        public int ValidityTotalDay { get; set; }
        public bool CloudRecording { get; set; }
        public int CloudRecordingGbSize { get; set; }
        public int SessionHours { get; set; }
        public bool SutdyRooms { get; set; }
        public string Logo { get; set; }
        public int PackageParentType { get; set; }
        public double? PackageDiscountPrice { get; set; }
        public DateTime? NewEndDate { get; set; }
      
        public int? PackageFinishDay { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Package, PackageDto>()
                .ForMember(a=>a.Id, opt => opt.MapFrom(s=>s.Id))
                .ForMember(a=>a.Name, opt => opt.MapFrom(s=>s.Name))
                .ForMember(a=>a.Description, opt => opt.MapFrom(s=>s.Description))
                .ForMember(a=>a.Detail, opt => opt.MapFrom(s=>s.Detail))
                .ForMember(a=>a.Price, opt => opt.MapFrom(s=>s.Price))
                .ForMember(a=>a.PriceCurrency, opt => opt.MapFrom(s=>s.PriceCurrency))
                .ForMember(a=>a.SmsCountGift, opt => opt.MapFrom(s=>s.SmsCountGift))
                .ForMember(a=>a.ValidityTotalDay, opt => opt.MapFrom(s=>s.ValidityTotalDay))
                .ForMember(a=>a.CloudRecording, opt => opt.MapFrom(s=>s.CloudRecording))
                .ForMember(a=>a.CloudRecordingGbSize, opt => opt.MapFrom(s=>s.CloudRecordingGbSize))
                .ForMember(a=>a.SessionHours, opt => opt.MapFrom(s=>s.SessionHours))
                .ForMember(a=>a.SutdyRooms, opt => opt.MapFrom(s=>s.SutdyRooms))
                .ForMember(a=>a.Logo, opt => opt.MapFrom(s=>s.Logo))
                .ForMember(a=>a.ParentID, opt => opt.MapFrom(s=>s.ParentID))
                .ForMember(a=>a.Duration, opt => opt.MapFrom(s=>s.Duration))
                .ForMember(a=>a.PackageParentType, opt => opt.MapFrom(s=>s.PackageParentType));
        }

    }
}
