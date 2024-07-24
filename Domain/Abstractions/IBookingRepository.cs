using Domain.Entities;

namespace Domain.Abstractions
{
    public interface IBookingRepository
    {
        Task<Booking> CreateRoomBookingAsync(Booking booking);
    }
}
