using Domain.Abstractions;
using Domain.Entities;
using Domain.Enums;
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

        public async Task AddAmenityAsync(HotelAmenity amenityHotel)
        {
            await _dbContext.HotelAmenity.AddAsync(amenityHotel);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<bool> AmenityExistsAsync(int hotelId, int amenityId)
        {
            return await _dbContext.HotelAmenity.AnyAsync(
                x => 
                    x.AmenityId == amenityId && x.HotelId == hotelId
                );
        }

        public async Task BeginTransactionAsync()
        {
            await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_dbContext.Database.CurrentTransaction is null) return;
            await _dbContext.Database.CurrentTransaction.CommitAsync();
        }

        public async Task<Hotel> DeleteAsync(Hotel hotel)
        {
            _dbContext.Hotels.Remove(hotel);
            await _dbContext.SaveChangesAsync();
            return hotel;
        }


        public async Task<(IEnumerable<Hotel>, int)> GetAsync
            (
                int page,
                int pageSize,
                decimal minPrice,
                decimal? maxPrice,
                string? city,
                string? country,
                HotelType[] hotelType,
                string? hotelName,
                string? owner,
                int[] aminites
            )
        {
            var query = _dbContext.Hotels
                                  .Include(o => o.City)
                                  .Include(o => o.HotelAmenity).ThenInclude(o => o.Amenity)
                                  .Where(p => p.PricePerNight >= minPrice);

            if (aminites.Length > 0) 
                query = query.Where
                    (
                        am => am.HotelAmenity.Count > 0 && 
                        aminites.All(p => am.HotelAmenity.Any(o => o.AmenityId == p))
                    );

            if (maxPrice is not null) 
                query = query.Where(p => p.PricePerNight <= maxPrice);

            if (!String.IsNullOrEmpty(city)) 
                query = query.Where(c => c.City.Name.Contains(city));

            if (!String.IsNullOrEmpty(country)) 
                query = query.Where(c => c.City.Country.Contains(country));

            if (hotelType is not null && hotelType.Length > 0 ) 
                query = query.Where(ht => hotelType.Contains(ht.HotelType));
            
            if (!String.IsNullOrEmpty(owner)) 
                query = query.Where(o => o.Owner.Contains(owner));
            
            if (!String.IsNullOrEmpty(hotelName)) 
                query = query.Where(o => o.Name.Contains(hotelName));
           
            int totalRecords = await query.CountAsync();
            return (
                     await query.Take(page * pageSize)
                                .Skip((page - 1) * pageSize)
                                .ToListAsync()
                     , totalRecords
                   );
        }

        public async Task<Hotel?> GetByIdAsync(int hotelId)
        {
            return await _dbContext.Hotels
                .Include(inc => inc.HotelAmenity).ThenInclude(o => o.Amenity)
                .Include(inc => inc.City)
                .Include(inc => inc.Images)
                .Include(inc => inc.Rooms).ThenInclude(o=>o.Images)
                .FirstOrDefaultAsync(hotel => hotel.Id == hotelId);
        }

        public async Task CreateAsync(Hotel hotel)
        {
            await _dbContext.AddAsync(hotel);
            await _dbContext.SaveChangesAsync();
            var city = await _dbContext.Cities.FirstOrDefaultAsync(x => x.Id == hotel.CityId);
            hotel.City = city;
        }

        public async Task RemoveAmenityAsync(HotelAmenity amenityHotel)
        {
            _dbContext.HotelAmenity.Remove(amenityHotel);
            await _dbContext.SaveChangesAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            if (_dbContext.Database.CurrentTransaction is null) return;
            await _dbContext.Database.CurrentTransaction.RollbackAsync();
        }

        public async Task UpdateAsync(Hotel updatedHotel)
        {
            _dbContext.Entry(updatedHotel).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddRoomAsync(Room room)
        {
            await _dbContext.Rooms.AddAsync(room);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Room> DeleteRoomAsync(Room room)
        {
            _dbContext.Rooms.Remove(room);
            await _dbContext.SaveChangesAsync();
            return room;
        }

        public async Task<Room?> GetHotelRoom(int hotelId, string roomNumber)
        {
            return await _dbContext.Rooms
                .Include(o => o.Images)
                .FirstOrDefaultAsync(
                    p =>
                        p.RoomNumber.ToLower() == roomNumber.ToLower() &&
                        p.HotelId == hotelId
            );
        }

        public async Task<bool> RoomNumberExistsAsync(int hotelId, string roomNumber)
        {
            return await _dbContext.Rooms.AnyAsync(
                p =>
                p.RoomNumber.ToLower() == roomNumber.ToLower() &&
                p.HotelId == hotelId
            );
        }

    }
}
