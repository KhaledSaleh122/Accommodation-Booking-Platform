namespace Presentation.Responses.Validation
{
    public class ValidationFailureResponse
    {
        public List<ValidationResponse> Errors { get; set; }
        public string Title { get; set; }

        public int Status { get; set; }

        public string TraceId { get; set; }
    }
}
