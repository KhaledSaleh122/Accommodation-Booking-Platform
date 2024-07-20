namespace Presentation.Responses.ServerErrors
{
    public class ServerErrorResponse
    {
        public string Title { get; set; }

        public int Status { get; set; }

        public string TraceId { get; set; }
    }
}
