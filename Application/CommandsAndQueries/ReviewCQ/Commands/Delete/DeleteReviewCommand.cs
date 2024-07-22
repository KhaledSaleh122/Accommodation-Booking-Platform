using Application.Dtos.ReviewDtos;
using MediatR;

namespace Application.CommandsAndQueries.ReviewCQ.Commands.Delete
{
    public class DeleteReviewCommand : IRequest<ReviewDto>
    {
        public int HotelId { get; set; }
        public string UserId { get; set; }
    }
}
