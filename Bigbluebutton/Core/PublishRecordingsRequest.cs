namespace BigBlueButtonAPI.Core
{
    public class PublishRecordingsRequest:BaseRequest
    {
        /// <summary>
        /// Required.
        /// A record ID for specify the recordings to apply the publish action. It can be a set of record IDs separated by commas.
        /// </summary>
        public string recordID { get; set; }

        /// <summary>
        /// Required.
        /// The value for publish or unpublish the recording(s). 
        /// </summary>
        public bool publish { get; set; }
    }
}