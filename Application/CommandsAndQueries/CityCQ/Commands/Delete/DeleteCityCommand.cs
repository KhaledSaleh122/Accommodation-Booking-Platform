using Application.Dtos.CityDtos;
using MediatR;

namespace Application.CommandsAndQueries.CityCQ.Commands.Delete
{
    public class DeleteCityCommand(uint id) : IRequest<CityDto>
    {
        public uint Id { get; set; } = id;
    }
}
