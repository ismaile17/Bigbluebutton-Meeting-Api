using Application.Shared.Mappings;
using AutoMapper;

namespace Application.FileSystem.Models
{
    public class FileListDto : IMapFrom<Domain.Entities.FileSystem>
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public DateTime CreatedTime { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Domain.Entities.FileSystem, FileListDto>()
                .ForMember(a => a.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(a => a.ContentType, opt => opt.MapFrom(s => s.ContentType))
                .ForMember(a => a.FileSize, opt => opt.MapFrom(s => s.FileSize))
                .ForMember(a => a.CreatedTime, opt => opt.MapFrom(s => s.CreatedTime))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName.Length > 25 ? src.FileName.Substring(0, 25) + "..." : src.FileName));
        }
    }
}
