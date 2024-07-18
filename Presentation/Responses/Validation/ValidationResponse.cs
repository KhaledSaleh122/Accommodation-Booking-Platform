using System.ComponentModel.DataAnnotations;

namespace Presentation.Responses.Validation
{
    public class ValidationResponse
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string ErrorMessage { get; set; }
    }
}
