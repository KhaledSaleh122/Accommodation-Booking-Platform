#nullable disable

namespace Domain.Entities
{
    public sealed class Review
    {
        public int Rating { get; set; }
        public string Comment { get; set; }
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
