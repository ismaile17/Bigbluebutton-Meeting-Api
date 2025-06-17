using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class UserSmsCount
    {
        public int UserId { get; set; }
        public virtual AppUser User { get; set; }
        public int SmsCountt { get; set; }

    }
}