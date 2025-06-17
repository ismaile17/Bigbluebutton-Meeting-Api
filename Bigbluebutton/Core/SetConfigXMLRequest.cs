namespace BigBlueButtonAPI.Core
{
    public class SetConfigXMLRequest:BaseRequest
    {
        /// <summary>
        /// A meetingID to an active meeting
        /// </summary>
        public string meetingID { get; set; }


        /// <summary>
        /// The content of a valid config.xml file.
        /// </summary>
        public string configXML { get; set; }
    }
}