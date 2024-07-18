using Application.Dtos.AmenityDtos;
using MediatR;
namespace Application.CommandsAndQueries.AmenityCQ.Commands.Create
{
    public class CreateAmenityCommand(string name, string description) : IRequest<AmenityDto>
    {

        public string Name { get; set; } = name;
        public string Description { get; set; } = description;
    }
}
