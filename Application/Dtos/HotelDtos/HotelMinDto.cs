namespace Application.Dtos.HotelDtos
{
    public class HotelMinDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<string> Images { get; set; }
        public string Thumbnail { get; set; }

        public string Owner { get; set; }
        public string Address { get; set; }
        public string HotelType { get; set; }

        public decimal PricePerNight { get; set; }
    }
}
