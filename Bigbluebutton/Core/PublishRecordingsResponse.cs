using System.Xml.Serialization;

namespace BigBlueButtonAPI.Core
{
    [XmlRoot("response")]
    public class PublishRecordingsResponse:BaseResponse
    {
        public bool? published { get; set; }
    }
}