using Application.Dtos.CityDtos;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.CommandsAndQueries.CityCQ.Commands.Create
{
    public class CreateCityCommand(string name, string country, string postOffice) : IRequest<CityDto>
    {
        [Required]
        public string Name { get; set; } = name;
        [Required]
        public string Country { get; set; } = country;
        [Required]
        public string PostOffice { get; set; } = postOffice;
    }
}
