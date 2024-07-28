using Application.Dtos.AmenityDtos;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.CommandsAndQueries.AmenityCQ.Commands.Update
{
    public class UpdateAmenityCommand : IRequest<AmenityDto>
    {
        [Required]
        public int id;
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
    }
}
