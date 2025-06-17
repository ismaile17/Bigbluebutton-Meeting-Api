using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UserReportAndDetailDatas.Model.UserCalendarPage
{
    public class UserCalendarPageDto
    {
        public int? Id { get; set; }
        public int MeetingType { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDateAndTime { get; set; }
        public DateTime? EndDateAndTime { get; set; }

    }
}
