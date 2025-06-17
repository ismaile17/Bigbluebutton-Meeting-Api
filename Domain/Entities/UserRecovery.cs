using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class UserRecovery : BaseEntity
    {
        public int AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; }

        public string Key { get; set; }
    }
}