namespace BigBlueButtonAPI.Core
{
    public class IsMeetingRunningRequest: BaseRequest
    {
        /// <summary>
        /// Required.
        /// The meeting ID that identifies the meeting you are attempting to check on.
        /// </summary>
        public string meetingID { get; set; }
    }
}