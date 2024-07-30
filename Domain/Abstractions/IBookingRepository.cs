using Domain.Entities;

namespace Domain.Abstractions
{
    public interface IBookingRepository
    {
        Task<Booking> CreateRoomBookingAsync(Booking booking);
        Task<(IEnumerable<Booking>,int)> GetAsync(
            string userId, int page, int pageSize, DateOnly? startDate, DateOnly? endDate);
        Task<Booking?> GetByIdAsync(string userId, int bookingId);
        Task<Booking?> GetByPaymentIntentIdAsync(string paymentIntentId);
    }
}
