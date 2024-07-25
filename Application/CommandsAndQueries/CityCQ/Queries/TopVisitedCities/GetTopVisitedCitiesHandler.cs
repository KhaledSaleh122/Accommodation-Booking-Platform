using Application.Dtos.CityDtos;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.CommandsAndQueries.CityCQ.Queries.TopVisitedCities
{
    public class GetTopVisitedCitiesHandler : IRequestHandler<GetTopVisitedCitiesCommand, IEnumerable<CityTopDto>>
    {
        private readonly ICityRepository _cityRepository;
        private readonly IMapper _mapper;

        public GetTopVisitedCitiesHandler(ICityRepository cityRepository)
        {
            _cityRepository = cityRepository ?? throw new ArgumentNullException(nameof(cityRepository));
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<City, CityTopDto>();
            });
            _mapper = configuration.CreateMapper();
        }

        public async Task<IEnumerable<CityTopDto>> Handle(
            GetTopVisitedCitiesCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var cities = await _cityRepository.TopVisitedCities();
                return _mapper.Map<IEnumerable<CityTopDto>>(cities);

            }
            catch (Exception exception)
            {

                throw new ErrorException($"Error during getting top visited cities.", exception);
            }

        }
    }
}
