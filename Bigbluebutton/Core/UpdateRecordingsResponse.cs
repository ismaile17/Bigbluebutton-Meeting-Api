using System.Xml.Serialization;

namespace BigBlueButtonAPI.Core
{
    [XmlRoot("response")]
    public class UpdateRecordingsResponse:BaseResponse
    {
        public bool? updated { get; set; }
    }
}