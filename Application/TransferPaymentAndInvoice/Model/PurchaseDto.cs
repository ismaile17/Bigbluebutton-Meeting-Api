using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;

public class PurchaseDto : IMapFrom<Purchase>
{
    public int Id { get; set; }
    public int AppUserId { get; set; }
    public int PackageId { get; set; }
    public int StatusType { get; set; }
    public string PurchaseType { get; set; }
    public double Price { get; set; }
    public int? CouponId { get; set; }
    public DateTime CreatedTime { get; set; }

    // Yeni alanlar
    public double? DiscountValue { get; set; } // Kuponun indirim değeri
    public int? Duration { get; set; } 
    public string? PackageName { get; set; } // Paket adı
    public string? PackageDescription { get; set; } // Paket açıklaması
    public string? Code { get; set; } // Paket açıklaması
    public string? Name { get; set; } // Paket açıklaması
    public string? Email { get; set; } // Paket açıklaması
    public string? InvoiceType { get; set; } // Paket açıklaması
    public string? InvoiceNameSurname { get; set; } // Paket açıklaması
    public string? PhoneNumber { get; set; } // Paket açıklaması
    public string? InvoiceAddress { get; set; } // Paket açıklaması
    public string? InvoiceNumber { get; set; } // Paket açıklaması
    public string? InvoiceCountry { get; set; } // Paket açıklaması
    public string? InvoiceVD { get; set; } // Paket açıklaması

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Purchase, PurchaseDto>()
            .ForMember(a => a.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(a => a.AppUserId, opt => opt.MapFrom(s => s.AppUserId))
            .ForMember(a => a.PackageId, opt => opt.MapFrom(s => s.PackageId))
            .ForMember(a => a.StatusType, opt => opt.MapFrom(s => s.StatusType))
            .ForMember(a => a.PurchaseType, opt => opt.MapFrom(s => s.PurchaseType))
            .ForMember(a => a.Price, opt => opt.MapFrom(s => s.Price))
            .ForMember(a => a.CouponId, opt => opt.MapFrom(s => s.CouponId))
            .ForMember(a => a.CreatedTime, opt => opt.MapFrom(s => s.CreatedTime))
            .ForMember(a => a.DiscountValue, opt => opt.MapFrom(s => s.Coupon != null ? (double?)s.Coupon.DiscountValue : null))
            .ForMember(a => a.Code, opt => opt.MapFrom(s => s.Coupon.Code))
            .ForMember(a => a.Duration, opt => opt.MapFrom(s => s.Package.Duration))
            .ForMember(a => a.PackageName, opt => opt.MapFrom(s => s.Package.Name))
            .ForMember(a => a.PackageDescription, opt => opt.MapFrom(s => s.Package.Description))
            ;
    }
}
