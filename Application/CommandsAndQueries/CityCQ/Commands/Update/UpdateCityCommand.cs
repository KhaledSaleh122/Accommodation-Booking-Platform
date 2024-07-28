﻿using Application.Dtos.CityDtos;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.CommandsAndQueries.CityCQ.Commands.Update
{
    public class UpdateCityCommand : IRequest<CityDto>
    {
        [Required]
        public int id;
        [Required]
        public string Name { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string PostOffice { get; set; }
    }
}
