using Application.Dtos.CityDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.CommandsAndQueries.CityCQ.Commands.Delete
{
    public class DeleteCityHandler : IRequestHandler<DeleteCityCommand, CityDto>
    {
        private readonly IMapper _mapper;
        private readonly ICityRepository _repository;

        public DeleteCityHandler(ICityRepository repository)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<City, CityDto>();
                cfg.CreateMap<CityDto, City>();
            });
            _mapper = configuration.CreateMapper();
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<CityDto> Handle(DeleteCityCommand request, CancellationToken cancellationToken)
        {
            City deletedCity;
            try
            {
                var city = await _repository.GetByIdAsync(request.Id) ?? throw new NotFoundException();
                deletedCity = await _repository.DeleteAsync(city);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception)
            {

                throw new ErrorException($"Error during deleting city with id '{request.Id}'.");
            }
            return _mapper.Map<CityDto>(deletedCity);
        }
    }
}
