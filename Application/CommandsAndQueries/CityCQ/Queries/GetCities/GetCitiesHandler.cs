using Application.Dtos.CityDtos;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using System;

namespace Application.CommandsAndQueries.CityCQ.Query.GetCities
{
    public class GetCitiesHandler : IRequestHandler<GetCitiesQuery, (IEnumerable<CityDto>, int, int, int)>
    {
        private readonly IMapper _mapper;
        private readonly ICityRepository _cityRepository;

        public GetCitiesHandler(ICityRepository cityRepository)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<City, CityDto>();
            });
            _mapper = configuration.CreateMapper();
            _cityRepository = cityRepository;
        }

        public async Task<(IEnumerable<CityDto>, int, int, int)> Handle(GetCitiesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (cities, totalRecords) = await _cityRepository.GetAsync
                    (
                        request.Page, 
                        request.PageSize, 
                        request.Country,
                        request.City
                    );
                return 
                    (
                        _mapper.Map<IEnumerable<CityDto>>(cities), 
                        totalRecords,
                        request.Page, 
                        request.PageSize
                    );
            }
            catch (Exception exception)
            {

                throw new ErrorException($"Error during Getting cities.", exception);
            }
        }
    }
}
