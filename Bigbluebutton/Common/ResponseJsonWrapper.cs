using System;
using System.Collections.Generic;
using System.Linq;

namespace BigBlueButtonAPI.Common
{
    public class ResponseJsonWrapper<T>
    {
        public T response { get; set; }
    }
}