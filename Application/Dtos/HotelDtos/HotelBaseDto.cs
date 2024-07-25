namespace Application.Dtos.HotelDtos
{
    public class HotelBaseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Thumbnail { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}
