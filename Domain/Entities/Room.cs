﻿using Domain.Enums;

namespace Domain.Entities
{
#nullable disable
    public class Room
    {
        public string RoomNumber { get; set; }
        public RoomStatus Status { get; set; }
        public int AdultCapacity { get; set; }
        public int ChildrenCapacity { get; set; }
        public string Thumbnail { get; set; }
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }
    }
}
