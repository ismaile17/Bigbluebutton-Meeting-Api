using System.Xml.Serialization;

namespace BigBlueButtonAPI.Core
{
    [XmlRoot("response")]
    public class PutRecordingTextTrackResponse:BaseResponse
    {
        public string recordID { get; set; }
    }
}