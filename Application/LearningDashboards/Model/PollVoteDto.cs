using Application.Shared.Mappings;
using AutoMapper;

namespace Application.LearningDashboards.Model
{
    public class PollVoteDto : IMapFrom<Domain.Entities.Learning.PollVote>
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public string? UserId { get; set; }
        public string? Vote { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Domain.Entities.Learning.PollVote, PollVoteDto>()
                .ForMember(a => a.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(a => a.PollId, opt => opt.MapFrom(s => s.PollId))
                .ForMember(a => a.UserId, opt => opt.MapFrom(s => s.UserId))
                .ForMember(a => a.Vote, opt => opt.MapFrom(s => s.Vote));
        }
    }
}
