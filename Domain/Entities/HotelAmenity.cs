﻿namespace Domain.Entities
{
    public sealed class HotelAmenity : BaseEntity
    {
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }

        public int AmenityId { get; set; }
        public Amenity Amenity { get; set; }
    }
}
