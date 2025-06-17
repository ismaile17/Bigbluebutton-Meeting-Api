using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace BigBlueButtonAPI.Common
{
    public class Playback
    {
        public string type { get; set; }
        public string url { get; set; }
        public int processingTime { get; set; }
        public int length { get; set; }
        public int size { get; set; }

        [XmlElement("preview")]
        public PlaybackPreviewImages previewImages { get; set; }
    }
}