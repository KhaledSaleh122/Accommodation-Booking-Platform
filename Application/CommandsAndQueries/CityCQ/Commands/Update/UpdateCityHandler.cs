using Application.Dtos.CityDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;

namespace Application.CommandsAndQueries.CityCQ.Commands.Update
{
    internal class UpdateCityHandler : IRequestHandler<UpdateCityCommand, CityDto>
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
        public async Task<CityDto> Handle(UpdateCityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var city = await _repository.GetByIdAsync(request.id) ?? throw new NotFoundException($"Not Found");
                var isCityExist = await _repository.DoesCityExistInCountryAsync(
                    request.Name, request.Country
                );
                if (isCityExist)
                    throw new ErrorException("A city with this name already exists in the country.")
                    {
                        StatusCode = StatusCodes.Status409Conflict
                    };
                var isPostExist = await _repository.DoesPostOfficeExistsAsync(request.PostOffice);
                if (isPostExist)
                    throw new ErrorException("This post office exists in a city.")
                    {
                        StatusCode = StatusCodes.Status409Conflict
                    };
                var updatedCity = _mapper.Map<City>(request);
                await _repository.UpdateAsync(updatedCity);
                return _mapper.Map<CityDto>(updatedCity);
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
