﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace BigBlueButtonAPI.Common
{
    [XmlRoot("image")]
    public class PlaybackPreviewImage
    {
        [XmlAttribute]
        public string alt { get; set; }

        [XmlAttribute]
        public int height { get; set; }

        [XmlAttribute]
        public int width { get; set; }

        [XmlText]
        public string url { get; set; }
    }
}