﻿#nullable disable

using Domain.Enums;

namespace Domain.Entities
{
    public sealed class Hotel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Thumbnail { get; set; }

        public string Owner { get; set; }
        public string Address { get; set; }

        public decimal PricePerNight { get; set; }
        public HotelType HotelType { get; set; }
    }
}
