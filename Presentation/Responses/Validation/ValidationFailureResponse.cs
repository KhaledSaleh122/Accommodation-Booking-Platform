using System.ComponentModel.DataAnnotations;

namespace Presentation.Responses.Validation
{
    public class ValidationFailureResponse
    {
        public List<ValidationResponse>? Errors { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public int Status { get; set; }
        [Required]
        public string TraceId { get; set; }
    }
}
