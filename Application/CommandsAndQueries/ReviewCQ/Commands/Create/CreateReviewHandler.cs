using Application.Dtos.ReviewDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Application.CommandsAndQueries.ReviewCQ.Commands.Create
{
    internal class CreateReviewHandler : IRequestHandler<CreateReviewCommand, ReviewDto>
    {
        private readonly IReviewHotelRepository _reviewRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public CreateReviewHandler(IReviewHotelRepository reviewRepository, IHotelRepository hotelRepository, UserManager<User> userManager)
        {
            _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CreateReviewCommand, Review>();
                cfg.CreateMap<Review, ReviewDto>();
            });
            _mapper = configuration.CreateMapper();
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<ReviewDto> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            var review = _mapper.Map<Review>(request);
            try
            {
                var user = await _userManager.FindByIdAsync(request.userId) ?? throw new NotFoundException("User not found!");
                var hotel = await _hotelRepository.GetByIdAsync(request.hotelId)
                    ?? throw new NotFoundException("Hotel not found!");
                review.HotelId = request.hotelId;
                review.UserId = request.userId;
                var isAlreadyReviewed = await _reviewRepository.DoesUserReviewedAsync(request.hotelId, request.userId);
                if (isAlreadyReviewed)
                    throw new ErrorException("User has already rated this hotel.")
                    {
                        StatusCode = StatusCodes.Status409Conflict
                    };
                var createdReview = await _reviewRepository.AddHotelReviewAsync(review);
                return _mapper.Map<ReviewDto>(createdReview);
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new ErrorException("Error during adding the review on the hotel", exception);
            }
        }
    }
}
