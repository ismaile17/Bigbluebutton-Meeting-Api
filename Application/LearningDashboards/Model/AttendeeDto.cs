using Application.Shared.Mappings;
using AutoMapper;
using Domain.Entities.Learning;
using System;
using System.Collections.Generic;

namespace Application.LearningDashboards.Model
{
    public class AttendeeDto : IMapFrom<Domain.Entities.Learning.Attendee>
    {
        public int Id { get; set; }
        public int? MeetingId { get; set; }
        public string? ExtUserId { get; set; }
        public string? Name { get; set; }
        public bool? Moderator { get; set; }
        public int? Duration { get; set; }
        public string? RecentTalkingTime { get; set; }
        public int? EngagementChats { get; set; }
        public int? EngagementTalks { get; set; }
        public int? EngagementRaisehand { get; set; }
        public int? EngagementEmojis { get; set; }
        public int? EngagementPollVotes { get; set; }
        public int? EngagementTalkTime { get; set; }
        public List<AttendeeSessionDto>? Sessions { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Domain.Entities.Learning.Attendee, AttendeeDto>()
                .ForMember(a => a.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(a => a.MeetingId, opt => opt.MapFrom(s => s.MeetingId))
                .ForMember(a => a.ExtUserId, opt => opt.MapFrom(s => s.ExtUserId))
                .ForMember(a => a.Name, opt => opt.MapFrom(s => s.Name))
                .ForMember(a => a.Moderator, opt => opt.MapFrom(s => s.Moderator))
                .ForMember(a => a.Duration, opt => opt.MapFrom(s => s.Duration))
                .ForMember(a => a.RecentTalkingTime, opt => opt.MapFrom(s => s.RecentTalkingTime))
                .ForMember(a => a.EngagementChats, opt => opt.MapFrom(s => s.EngagementChats))
                .ForMember(a => a.EngagementTalks, opt => opt.MapFrom(s => s.EngagementTalks))
                .ForMember(a => a.EngagementRaisehand, opt => opt.MapFrom(s => s.EngagementRaisehand))
                .ForMember(a => a.EngagementEmojis, opt => opt.MapFrom(s => s.EngagementEmojis))
                .ForMember(a => a.EngagementPollVotes, opt => opt.MapFrom(s => s.EngagementPollVotes))
                .ForMember(a => a.EngagementTalkTime, opt => opt.MapFrom(s => s.EngagementTalkTime))
                .ForMember(a => a.Sessions, opt => opt.MapFrom(s => s.Sessions))
                ;
        }
    }
}
