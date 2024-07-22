using Application.Dtos.ReviewDtos;
using MediatR;

namespace Application.CommandsAndQueries.ReviewCQ.Commands.Create
{
    public class CreateReviewCommand : IRequest<ReviewDto>
    {
        public int hotelId;
        public string userId;
        public string? Comment { get; set; }
        public int Rating { get; set; }
    }
}
