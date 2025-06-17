using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace BigBlueButtonAPI.Core
{
    [XmlRoot("response")]
    public class DeleteRecordingsResponse:BaseResponse
    {
        public bool? deleted { get; set; }
    }
}