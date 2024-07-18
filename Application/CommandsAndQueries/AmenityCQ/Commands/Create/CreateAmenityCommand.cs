using Application.Dtos.AmenityDtos;
using MediatR;
using System.ComponentModel.DataAnnotations;
namespace Application.CommandsAndQueries.AmenityCQ.Commands.Create
{
    public class CreateAmenityCommand(string name, string description) : IRequest<AmenityDto>
    {
        [Required]
        public string Name { get; set; } = name;
        [Required]
        public string Description { get; set; } = description;
    }
}
