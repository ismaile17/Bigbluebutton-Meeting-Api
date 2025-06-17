using System.Xml.Serialization;

namespace BigBlueButtonAPI.Core
{
    [XmlRoot("response")]
    public class IsMeetingRunningResponse : BaseResponse
    {
        /// <summary>
        /// Whether or not a meeting is running
        /// </summary>
        public bool? running { get; set; }
    }
}