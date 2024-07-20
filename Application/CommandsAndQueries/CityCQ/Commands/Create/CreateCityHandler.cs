using Application.Dtos.CityDtos;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using System;

namespace Application.CommandsAndQueries.CityCQ.Commands.Create
{
    public class CreateCityHandler : IRequestHandler<CreateCityCommand, CityDto>
    {
        private readonly ICityRepository _cityRepository;
        private readonly IMapper _mapper;

        public CreateCityHandler(ICityRepository cityRepository)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<City, CityDto>();
                cfg.CreateMap<CreateCityCommand, City>();
            });
            _mapper = configuration.CreateMapper();
            _cityRepository = cityRepository ?? throw new ArgumentNullException(nameof(cityRepository));
        }

        public async Task<CityDto> Handle(CreateCityCommand request, CancellationToken cancellationToken)
        {
            var city = _mapper.Map<City>(request);
            try
            {
                await _cityRepository.CreateAsync(city);
            }
            catch (Exception exception)
            {
                throw new ErrorException($"Error during creating new city.", exception);
            }
            var cityDto = _mapper.Map<CityDto>(city);
            return cityDto;
        }
    }
}
