using System.Xml.Serialization;

namespace BigBlueButtonAPI.Core
{
    [XmlRoot("response")]
    public class SetConfigXMLResponse:BaseResponse
    {
        public string configToken { get; set; }
    }
}