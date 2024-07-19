using Application.Dtos.CityDtos;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.CommandsAndQueries.CityCQ.Commands.Create
{
    public class CreateCityCommand : IRequest<CityDto>
    {
        public CreateCityCommand() { 
        }
        public CreateCityCommand(string name, string country, string postOffice)
        {
            Name = name;
            Country = country;
            PostOffice = postOffice;
        }

        [Required]
        public string Name { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string PostOffice { get; set; }
    }
}
