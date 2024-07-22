using Application.Dtos.ReviewDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System;

namespace Application.CommandsAndQueries.ReviewCQ.Commands.Delete
{
    public class DeleteReviewHandler : IRequestHandler<DeleteReviewCommand, ReviewDto>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IMapper _mapper;

        public DeleteReviewHandler(IReviewRepository reviewRepository, IHotelRepository hotelRepository)
        {
            _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<DeleteReviewCommand, Review>();
                cfg.CreateMap<Review, ReviewDto>();
            });
            _mapper = configuration.CreateMapper();
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
        }
        public async Task<ReviewDto> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
        {
            try
			{
                var hotel = await _hotelRepository.GetByIdAsync(request.HotelId)
                                ?? throw new NotFoundException("Hotel not found!");
                var review = await _reviewRepository.GetReview(request.HotelId, request.UserId)
                                ?? throw new NotFoundException("Review not found!");
                var deletedReview = await _reviewRepository.DeleteHotelReview(review);
                return _mapper.Map<ReviewDto>(deletedReview);
            } catch (NotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {

                throw new ErrorException("Error during delete the review on the hotel", exception);
            }
        }
    }
}
