using Application.Dtos.AmenityDtos;
using MediatR;
using System.ComponentModel.DataAnnotations;
namespace Application.CommandsAndQueries.AmenityCQ.Commands.Create
{
    public class CreateAmenityCommand : IRequest<AmenityDto>
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
    }
}
