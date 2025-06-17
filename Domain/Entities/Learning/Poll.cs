using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Learning
{
    public class Poll
    {
        public int Id { get; set; }
        public int? MeetingId { get; set; }
        public string? PollId { get; set; }
        public string? Type { get; set; }
        public string? Question { get; set; }
        public bool? Published { get; set; }
        public DateTime? Start { get; set; }
        public ICollection<PollVote>? PollVotes { get; set; }

    }
}
