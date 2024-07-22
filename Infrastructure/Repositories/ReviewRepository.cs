﻿using Domain.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ReviewRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<Review> AddHotelReview(Review review)
        {
            _dbContext.Reviews.Add(review);
            await _dbContext.SaveChangesAsync();
            return review;
        }

        public async Task<Review> DeleteHotelReview(Review review)
        {
            _dbContext.Reviews.Remove(review);
            await _dbContext.SaveChangesAsync();
            return review;
        }

        public async Task<bool> DoesUserReviewed(int hotelId, string userId)
        {
            return await _dbContext.Reviews.AnyAsync(review => review.UserId == userId && review.HotelId == hotelId);
        }

        public async Task<Review?> GetReview(int hotelId, string userId)
        {
            return await _dbContext.Reviews.FirstOrDefaultAsync(review =>
                review.HotelId == hotelId &&
                review.UserId == userId
            );
        }
    }
}
