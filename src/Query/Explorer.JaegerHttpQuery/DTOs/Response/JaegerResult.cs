namespace Explorer.JaegerHttpQuery.DTOs
{
    public class JaegerResult<T>
    {
        public JaegerResult(T data)
        {
            Data = data;
        }

        public JaegerResult()
        {
                
        }

        public JaegerResultError Error { get; set; }
        public T Data { get; set; }
    }

    public class JaegerResultError
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }
}