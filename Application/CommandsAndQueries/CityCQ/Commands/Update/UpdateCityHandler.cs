using Application.Dtos.CityDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using System;

namespace Application.CommandsAndQueries.CityCQ.Commands.Update
{
    public class UpdateCityHandler : IRequestHandler<UpdateCityCommand>
    {
        private readonly IMapper _mapper;
        private readonly ICityRepository _repository;

        public UpdateCityHandler(ICityRepository repository)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<City, CityDto>();
                cfg.CreateMap<UpdateCityCommand, City>();
            });
            _mapper = configuration.CreateMapper();
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        public async Task Handle(UpdateCityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var city = await _repository.GetByIdAsync(request.id) ?? throw new NotFoundException($"Not Found");
                var updatedCity = _mapper.Map<City>(request);
                await _repository.UpdateAsync(updatedCity);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new ErrorException($"Error during updaing city with id '{request.id}'.", exception);
            }
        }
    }
}
