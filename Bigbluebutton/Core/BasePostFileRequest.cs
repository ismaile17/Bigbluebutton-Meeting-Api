using BigBlueButtonAPI.Common;
using System;
using System.Collections.Generic;
using System.Linq;


namespace BigBlueButtonAPI.Core
{
    public class BasePostFileRequest: BaseRequest
    {
        public FileContentData file { get; set; }
    }
}