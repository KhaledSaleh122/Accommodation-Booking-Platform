namespace Application.Dtos.HotelDtos
{
    public class HotelMinNoImagesDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string Thumbnail { get; set; }

        public string Owner { get; set; }
        public string Address { get; set; }
        public string HotelType { get; set; }

        public decimal PricePerNight { get; set; }
    }
}
