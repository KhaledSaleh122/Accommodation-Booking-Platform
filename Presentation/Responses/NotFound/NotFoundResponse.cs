using System.ComponentModel.DataAnnotations;

namespace Presentation.Responses.NotFound
{
    public class NotFoundResponse
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public int Status { get; set; }

        [Required]
        public string TraceId { get; set; }
    }
}
