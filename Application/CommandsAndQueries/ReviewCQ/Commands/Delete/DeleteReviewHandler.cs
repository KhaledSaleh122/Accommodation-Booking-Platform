using Application.Dtos.ReviewDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.CommandsAndQueries.ReviewCQ.Commands.Delete
{
    internal class DeleteReviewHandler : IRequestHandler<DeleteReviewCommand, ReviewDto>
    {
        private readonly IReviewHotelRepository _reviewRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public DeleteReviewHandler(
            IReviewHotelRepository reviewRepository,
            IHotelRepository hotelRepository,
            UserManager<User> userManager)
        {
            _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<DeleteReviewCommand, Review>();
                cfg.CreateMap<Review, ReviewDto>();
            });
            _mapper = configuration.CreateMapper();
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }
        public async Task<ReviewDto> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var hotel = await _hotelRepository.GetByIdAsync(request.HotelId)
                                ?? throw new NotFoundException("Hotel not found!");
                var user = await _userManager.FindByIdAsync(request.UserId)
                                ?? throw new NotFoundException("User not found!");
                var review = await _reviewRepository.GetReviewAsync(request.HotelId, request.UserId)
                                ?? throw new NotFoundException("Review not found!");
                var deletedReview = await _reviewRepository.DeleteHotelReviewAsync(review);
                return _mapper.Map<ReviewDto>(deletedReview);
            }
            catch (NotFoundException)
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
