﻿namespace Domain.Entities
{
    public class HotelImage
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }
    }
}
