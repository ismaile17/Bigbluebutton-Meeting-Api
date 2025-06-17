using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Learning
{
    public class AttendeeSession
    {
        public int Id { get; set; }
        public int? AttendeeId { get; set; }
        public DateTime? JoinTime { get; set; }
        public DateTime? LeaveTime { get; set; }
    }
}
