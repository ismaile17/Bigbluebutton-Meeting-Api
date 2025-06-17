namespace Payment.Helper.Results
{
    public class ResultSingle<T> : Result
    {
        public T Value { get; set; }

        public ResultSingle(T value, bool success, string message) : base(success, message)
        {
            Value = value;
        }
    }
}