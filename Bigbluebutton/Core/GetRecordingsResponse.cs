using BigBlueButtonAPI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace BigBlueButtonAPI.Core
{
    [XmlRoot("response")]
    public class GetRecordingsResponse : BaseResponse
    {
        [XmlArrayItem("recording")]
        public List<Recording> recordings { get; set; }
    }
}