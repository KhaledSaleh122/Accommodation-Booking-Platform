namespace Domain.Entities
{
    public sealed class RoomImage
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string RoomNumber { get; set; }
        public int HotelId { get; set; }
        public Room room { get; set; }
    }
}
