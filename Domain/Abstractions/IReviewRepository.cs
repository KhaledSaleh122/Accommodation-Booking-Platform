using Domain.Entities;

namespace Domain.Abstractions
{
    public interface IReviewRepository
    {
        public Task<Review> AddHotelReview(Review review);
        public Task<Review> DeleteHotelReview(Review review);
    }
}
