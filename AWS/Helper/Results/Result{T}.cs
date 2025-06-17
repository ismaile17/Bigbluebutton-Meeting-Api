using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Helper.Results
{
    public class Result<T> : Result
    {
        public IQueryable<T> Value { get; set; }

        public Result(IQueryable<T> value, bool success, string message)
                : base(success, message)
        {
            Value = value;
        }
    }
}
