using Domain.Enums;

namespace Application.Dtos.RoomDtos
{
    public class RoomDto
    {
        public string RoomNumber { get; set; }
        public string Status { get; set; }
        public int AdultCapacity { get; set; }
        public int ChildrenCapacity { get; set; }
        public string Thumbnail { get; set; }
        public List<string> Images { get; set; }
    }
}
