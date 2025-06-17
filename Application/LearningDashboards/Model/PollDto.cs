using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities.Learning;
using System;
using System.Collections.Generic;

namespace Application.LearningDashboards.Model
{
    public class PollDto : IMapFrom<Domain.Entities.Learning.Poll>
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }
        public string? PollId { get; set; }
        public string? Type { get; set; }
        public string? Question { get; set; }
        public bool? Published { get; set; }
        public DateTime? Start { get; set; }
        public List<PollVoteDto>? PollVotes { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Domain.Entities.Learning.Poll, PollDto>()
                .ForMember(a => a.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(a => a.MeetingId, opt => opt.MapFrom(s => s.MeetingId))
                .ForMember(a => a.PollId, opt => opt.MapFrom(s => s.PollId))
                .ForMember(a => a.Type, opt => opt.MapFrom(s => s.Type))
                .ForMember(a => a.Question, opt => opt.MapFrom(s => s.Question))
                .ForMember(a => a.Published, opt => opt.MapFrom(s => s.Published))
                .ForMember(a => a.Start, opt => opt.MapFrom(s => s.Start))
                .ForMember(a => a.PollVotes, opt => opt.MapFrom(s => s.PollVotes))
                ;
        }
    }
}
