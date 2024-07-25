using Domain.Entities;

namespace Domain.Abstractions
{
    public interface IReviewHotelRepository
    {
        public Task<Review> AddHotelReviewAsync(Review review);
        public Task<Review> DeleteHotelReviewAsync(Review review);
        public Task<bool> DoesUserReviewedAsync(int hotelId, string userId);
        Task<Review?> GetReviewAsync(int hotelId, string userId);
    }
}
