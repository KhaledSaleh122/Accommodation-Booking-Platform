﻿using Application.Dtos.AmenityDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using System.Runtime.CompilerServices;

namespace Application.CommandsAndQueries.AmenityCQ.Commands.Update
{
    public class UpdateAmenityHandler : IRequestHandler<UpdateAmenityCommand>
    {
        private readonly IMapper _mapper;
        private readonly IAmenityRepository _repository;

        public UpdateAmenityHandler(IAmenityRepository repository)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Amenity, AmenityDto>();
                cfg.CreateMap<UpdateAmenityCommand, Amenity>();
            });
            _mapper = configuration.CreateMapper();
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        public async Task Handle(UpdateAmenityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var amenity = await _repository.GetByIdAsync(request.id) ??
                    throw new NotFoundException($"Amenity not found!");
                var updatedAmenity = _mapper.Map<Amenity>(request);
                await _repository.UpdateAsync(updatedAmenity);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw new ErrorException($"Error during updaing amenity with id '{request.id}'.");
            }
        }
    }
}
