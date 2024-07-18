using Application.Dtos.CityDtos;
using MediatR;

namespace Application.CommandsAndQueries.CityCQ.Commands.Create
{
    public class CreateCityCommand(string name, string country, string postOffice) : IRequest<CityDto>
    {
        public string Name { get; set; } = name;
        public string Country { get; set; } = country;

        public string PostOffice { get; set; } = postOffice;
    }
}
