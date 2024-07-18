using Application.Dtos.AmenityDtos;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.CommandsAndQueries.AmenityCQ.Commands.Delete
{
    public class DeleteAmenityCommand(int id) : IRequest<AmenityDto>
    {
        [Required]
        public int Id { get; set; } = id;
    }
}
