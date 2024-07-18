using Application.Dtos.AmenityDtos;
using MediatR;

namespace Application.CommandsAndQueries.AmenityCQ.Commands.Delete
{
    public class DeleteAmenityCommand(int id) : IRequest<AmenityDto>
    {
        public int Id { get; set; } = id;
    }
}
