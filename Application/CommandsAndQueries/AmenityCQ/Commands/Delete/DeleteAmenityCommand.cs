using Application.Dtos.AmenityDtos;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.CommandsAndQueries.AmenityCQ.Commands.Delete
{
    public class DeleteAmenityCommand(uint id) : IRequest<AmenityDto>
    {
        [Required]
        public uint Id { get; set; } = id;
    }
}
