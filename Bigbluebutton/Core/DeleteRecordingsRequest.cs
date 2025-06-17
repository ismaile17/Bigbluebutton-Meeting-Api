using System;
using System.Collections.Generic;
using System.Linq;

namespace BigBlueButtonAPI.Core
{
    public class DeleteRecordingsRequest:BaseRequest
    {
        /// <summary>
        /// A record ID for specify the recordings to delete. It can be a set of record IDs separated by commas.
        /// </summary>
        public string recordID { get; set; }
    }
}