using Domain.Abstractions;
using Domain.Entities;
using Domain.Enums;
using Domain.Params;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class HotelRepository : IHotelRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public HotelRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }



        public async Task<Hotel> DeleteAsync(Hotel hotel)
        {
            _dbContext.Hotels.Remove(hotel);
            await _dbContext.SaveChangesAsync();
            return hotel;
        }


        public async Task<(IDictionary<Hotel, double>, int)> GetAsync(HotelSearch search)
        {
            var query = _dbContext.Hotels
                                  .Include(o => o.City)
                                  .Include(o => o.HotelAmenity).ThenInclude(o => o.Amenity)
                                  .Where(p => p.PricePerNight >= search.MinPrice)
                                  .Where(h =>
                                    !h.Rooms.Any(r => r.BookingRooms.Any(br =>
                                        br.Booking.StartDate < search.CheckOut &&
                                        br.Booking.EndDate > search.CheckIn
                                    ))
                                  )
                                  .Where(
                                    h => h.Rooms.Any(
                                        r => 
                                            r.ChildrenCapacity >= search.Children && 
                                            r.AdultCapacity >= search.Adult
                                        )
                                  );

            if (search.Amenities is not null && search.Amenities.Length > 0) {
                var amenityIds = search.Amenities.ToList();
                query = query.Where(am => am.HotelAmenity.Any(o => amenityIds.Contains(o.AmenityId)));
            }

            if (search.MaxPrice is not null)
                query = query.Where(p => p.PricePerNight <= search.MaxPrice);

            if (!String.IsNullOrEmpty(search.City))
                query = query.Where(c => c.City.Name.Contains(search.City));

            if (!String.IsNullOrEmpty(search.Country))
                query = query.Where(c => c.City.Country.Contains(search.Country));

            if (search.HotelType is not null && search.HotelType.Length > 0)
                query = query.Where(ht => search.HotelType.Contains(ht.HotelType));

            if (!String.IsNullOrEmpty(search.Owner))
                query = query.Where(o => o.Owner.Contains(search.Owner));

            if (!String.IsNullOrEmpty(search.HotelName))
                query = query.Where(o => o.Name.Contains(search.HotelName));

            int totalRecords = await query.CountAsync();
            var result = await query
                .Select(h => new
                {
                    Hotel = h,
                    AvgReviewScore = h.Reviews.Count != 0 ? h.Reviews.Average(r => r.Rating) : 0
                })
                .Skip((search.Page - 1) * search.PageSize)
                .Take(search.PageSize)
                .ToDictionaryAsync(k => k.Hotel, v => v.AvgReviewScore);
            return (result, totalRecords);
        }

        public async Task<(Hotel, double)?> GetByIdAsync(int hotelId)
        {
            var result = await _dbContext.Hotels
                .Include(inc => inc.HotelAmenity).ThenInclude(o => o.Amenity)
                .Include(inc => inc.City)
                .Include(inc => inc.Images)
                .Include(inc => inc.Rooms).ThenInclude(o => o.Images)
                .Select(h => new
                {
                    Hotel = h,
                    Reviews = h.Reviews.Take(10).ToList(),
                    AvgReviewScore = h.Reviews.Count != 0 ? h.Reviews.Average(r => r.Rating) : 0
                })
                .FirstOrDefaultAsync(h => h.Hotel.Id == hotelId);
            if (result is not null)
                result.Hotel.Reviews = result.Reviews;

            return result is null ? null : (result.Hotel,result.AvgReviewScore);
        }

        public async Task CreateAsync(Hotel hotel)
        {
            await _dbContext.AddAsync(hotel);
            await _dbContext.SaveChangesAsync();
            var city = await _dbContext.Cities.FirstOrDefaultAsync(x => x.Id == hotel.CityId);
            hotel.City = city;
        }

        public async Task UpdateAsync(Hotel updatedHotel)
        {
            _dbContext.Entry(updatedHotel).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }


        public async Task<Hotel?> FindRoomsAsync(int hotelId, IEnumerable<string> roomsIds)
        {
            var result = await _dbContext.Hotels
                .Where(h => h.Id == hotelId)
                .Select(x => new
                {
                    Hotel = x,
                    Rooms = x.Rooms.Where(r => roomsIds.Contains(r.RoomNumber)).ToList()
                }).FirstOrDefaultAsync();
            if(result is not null)
                result.Hotel.Rooms = result.Rooms;
            return result?.Hotel;
        }
    }
}
