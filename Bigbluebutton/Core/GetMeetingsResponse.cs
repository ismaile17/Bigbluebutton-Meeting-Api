using BigBlueButtonAPI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace BigBlueButtonAPI.Core
{
    [XmlRoot("response")]
    public class GetMeetingsResponse : BaseResponse
    {
        [XmlArrayItem("meeting")]
        public List<MeetingInfo> meetings { get; set; }
    }
}