using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class MeetingGuestEmailList
    {
        public int MeetingId { get; set; }
        public Meeting Meeting { get; set; }
        public string Email { get; set; }
    }
}
