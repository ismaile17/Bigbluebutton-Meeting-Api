using System;
using System.Collections.Generic;
using System.Linq;

namespace BigBlueButtonAPI.Core
{
    public class GetMeetingInfoRequest: BaseRequest
    {
        /// <summary>
        /// Required.
        /// The meeting ID that identifies the meeting you are attempting to check on.
        /// </summary>
        public string meetingID { get; set; }
    }
}