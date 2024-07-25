using Domain.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ReviewRepository : IReviewHotelRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ReviewRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<Review> AddHotelReviewAsync(Review review)
        {
            _dbContext.Reviews.Add(review);
            await _dbContext.SaveChangesAsync();
            return review;
        }

        public async Task<Review> DeleteHotelReviewAsync(Review review)
        {
            _dbContext.Reviews.Remove(review);
            await _dbContext.SaveChangesAsync();
            return review;
        }

        public async Task<bool> DoesUserReviewedAsync(int hotelId, string userId)
        {
            return await _dbContext.Reviews.AnyAsync(review => review.UserId == userId && review.HotelId == hotelId);
        }

        public async Task<Review?> GetReviewAsync(int hotelId, string userId)
        {
            return await _dbContext.Reviews.FirstOrDefaultAsync(review =>
                review.HotelId == hotelId &&
                review.UserId == userId
            );
        }
    }
}
