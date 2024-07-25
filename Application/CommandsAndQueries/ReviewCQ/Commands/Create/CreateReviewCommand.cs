using Application.Dtos.ReviewDtos;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.CommandsAndQueries.ReviewCQ.Commands.Create
{
    public class CreateReviewCommand : IRequest<ReviewDto>
    {
        public int hotelId;
        public string userId;
        [Required]
        public string? Comment { get; set; }
        [Required]
        public int Rating { get; set; }
    }
}
