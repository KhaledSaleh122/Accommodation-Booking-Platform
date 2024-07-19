namespace Domain.Entities
{
#nullable disable
    public sealed class Amenity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<HotelAmenity> HotelAmenity { get; set; }
    }
}
