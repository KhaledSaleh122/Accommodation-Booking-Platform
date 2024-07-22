using Application.Dtos.ReviewDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.CommandsAndQueries.ReviewCQ.Commands
{
    public class CreateReviewHandler : IRequestHandler<CreateReviewCommand, ReviewDto>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IMapper _mapper;

        public CreateReviewHandler(IReviewRepository reviewRepository, IHotelRepository hotelRepository)
        {
            _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CreateReviewCommand, Review>();
                cfg.CreateMap<Review, ReviewDto>();
            });
            _mapper = configuration.CreateMapper();
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
        }

        public async Task<ReviewDto> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            var review = _mapper.Map<Review>(request);
            try
            {
                var hotel = await _hotelRepository.GetByIdAsync(request.hotelId)
                    ?? throw new NotFoundException("Hotel not found!");
                var createdReview = await _reviewRepository.AddHotelReview(review);
                return _mapper.Map<ReviewDto>(createdReview);
            }
            catch (NotFoundException) {
                throw;
            }
            catch (Exception exception)
            {
                throw new ErrorException("Error during adding the review on the hotel",exception);
            }
        }
    }
}
