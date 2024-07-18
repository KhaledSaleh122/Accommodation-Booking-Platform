using Application.Dtos.CityDtos;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.CommandsAndQueries.CityCQ.Query.GetCities
{
    public class GetCitiesHandler : IRequestHandler<GetCitiesQuery, (IEnumerable<CityDto>, uint, uint, uint)>
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

        public async Task<(IEnumerable<CityDto>, uint, uint, uint)> Handle(GetCitiesQuery request, CancellationToken cancellationToken)
        {
            uint page = request.Page > 0 ? request.Page : 1;
            uint pageSize = request.PageSize > 0 && request.PageSize <= 100 ? request.PageSize : 10;
            IEnumerable<City> cities;
            uint totalRecords;
            try
            {
                (cities, totalRecords) = await _cityRepository.GetAsync(page, pageSize, request.Country,request.City);
            }
            catch (Exception)
            {

                throw new ErrorException($"Error during Getting cities.");
            }

            return (_mapper.Map<IEnumerable<CityDto>>(cities), totalRecords, page, pageSize);
        }
    }
}
