using System;
using System.Collections.Generic;
using System.Linq;

namespace BigBlueButtonAPI.Core
{
    /// <summary>
    /// It contains the config data for the BigBlueButton API.
    ///   http://docs.bigbluebutton.org/dev/api.html
    /// </summary>
    public class BigBlueButtonAPISettings
    {
        /// <summary>
        /// The BigBlueButton server API endpoint (usually the server’s hostname followed by <b>/bigbluebutton/api/</b>, for example: http://yourserver.com/bigbluebutton/api/ ).
        /// </summary>
        public string ServerAPIUrl { get; set; }

        /// <summary>
        /// The shared secret code that is needed for the BigBlueButton server API. 
        /// You can retrieve it using the command in your BigBlueButton server:
        ///     $ bbb-conf --secret
        /// </summary>
        public string SharedSecret { get; set; }
    }
}