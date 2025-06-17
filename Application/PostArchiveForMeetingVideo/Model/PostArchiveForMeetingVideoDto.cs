using Application.Meetings.Model;
using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.PostArchiveForMeetingVideo.Model
{
    public class PostArchiveForMeetingVideoDto : IMapFrom<Domain.Entities.PostArchiveForMeetingVideo>
    {
        public string? MeetingId { get; set; }


        public void Mapping(Profile profile)
        {
            profile.CreateMap<Domain.Entities.PostArchiveForMeetingVideo, PostArchiveForMeetingVideoDto>()
                .ForMember(a => a.MeetingId, opt => opt.MapFrom(s => s.MeetingId));
        }
    }
}
