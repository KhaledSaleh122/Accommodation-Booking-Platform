using Domain.Enums;

namespace Domain.Params
{
    public class HotelSearch
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public decimal MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public HotelType[] HotelType { get; set; }
        public string? HotelName { get; set; }
        public string? Owner { get; set; }
        public int[] Aminites { get; set; }
        public DateOnly CheckIn { get; set; }
        public DateOnly CheckOut { get; set; }
        public int Children { get; set; }
        public int Adult { get; set; }
    }
}
