using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.BookingDtos
{
    public class CreditCardInformationDto
    {
        [Required]
        public string CardNumber { get; set; }
        [Required]
        public string CardHolderName { get; set; }
        [Required]
        public int ExpiryYear { get; set; }
        [Required]
        public int ExpiryMonth { get; set; }
        [Required]
        public string CardCodeCVC { get; set; }
    }
}
