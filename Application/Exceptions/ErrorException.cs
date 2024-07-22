namespace Application.Execptions
{
    public class ErrorException : Exception
    {
        public int? StatusCode { get; set; }
        public ErrorException() { }

        public ErrorException(string message)
            : base(message) { }

        public ErrorException(string message, Exception inner)
            : base(message, inner) { }

    }
}
