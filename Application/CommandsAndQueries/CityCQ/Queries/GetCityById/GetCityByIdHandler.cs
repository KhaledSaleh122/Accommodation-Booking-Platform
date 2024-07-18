using Application.Dtos.CityDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.CommandsAndQueries.CityCQ.Query.GetCityById
{
    public class GetCityByIdHandler : IRequestHandler<GetCityByIdQuery, CityDto>
    {
        private readonly IMapper _mapper;
        private readonly ICityRepository _cityRepository;
        public GetCityByIdHandler(ICityRepository cityRepository)
        {

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<City, CityDto>();
            });
            _mapper = configuration.CreateMapper();
            _cityRepository = cityRepository ?? throw new ArgumentNullException(nameof(cityRepository));
        }
        public async Task<CityDto> Handle(GetCityByIdQuery request, CancellationToken cancellationToken)
        {
            City? city;
            try
            {
                city = await _cityRepository.GetByIdAsync(request.CityId);
                if (city is null) throw new NotFoundException("City not found!");
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception)
            {

                throw new ErrorException($"Error during Getting city with id '{request.CityId}'.");
            }
            return _mapper.Map<CityDto>(city);
        }
    }
}
