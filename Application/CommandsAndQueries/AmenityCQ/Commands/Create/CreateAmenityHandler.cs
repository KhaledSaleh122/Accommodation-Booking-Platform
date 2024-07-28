using Application.Dtos.AmenityDtos;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using System;

namespace Application.CommandsAndQueries.AmenityCQ.Commands.Create
{
    internal class CreateAmenityHandler : IRequestHandler<CreateAmenityCommand, AmenityDto>
    {
        private readonly IAmenityRepository _amenityRepository;
        private readonly IMapper _mapper;

        public CreateAmenityHandler(IAmenityRepository amenityRepository)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Amenity, AmenityDto>();
                cfg.CreateMap<CreateAmenityCommand, Amenity>();
            });
            _mapper = configuration.CreateMapper();
            _amenityRepository = amenityRepository ?? throw new ArgumentNullException(nameof(amenityRepository));
        }

        public async Task<AmenityDto> Handle(CreateAmenityCommand request, CancellationToken cancellationToken)
        {
            var amenity = _mapper.Map<Amenity>(request);
            try
            {
                await _amenityRepository.CreateAsync(amenity);
            }
            catch (Exception exception)
            {
                throw new ErrorException($"Error during creating new amenity.", exception);
            }
            var amenityDto = _mapper.Map<AmenityDto>(amenity);
            return amenityDto;
        }
    }
}
