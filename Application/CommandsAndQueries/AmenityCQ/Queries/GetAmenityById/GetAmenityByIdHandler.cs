using Application.Dtos.AmenityDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using System;

namespace Application.CommandsAndQueries.AmenityCQ.Query.GetAmenityById
{
    internal class GetAmenityByIdHandler : IRequestHandler<GetAmenityByIdQuery, AmenityDto?>
    {
        private readonly IMapper _mapper;
        private readonly IAmenityRepository _amenityRepository;
        public GetAmenityByIdHandler(IAmenityRepository amenityRepository)
        {

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Amenity, AmenityDto>();
            });
            _mapper = configuration.CreateMapper();
            _amenityRepository = amenityRepository ?? throw new ArgumentNullException(nameof(amenityRepository));
        }
        public async Task<AmenityDto?> Handle(GetAmenityByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var amenity = await _amenityRepository.GetByIdAsync(request.AmenityId)
                     ?? throw new NotFoundException("Amenity not found!");
                return _mapper.Map<AmenityDto>(amenity);
            }
            catch (NotFoundException) {
                throw;
            }
            catch (Exception exception)
            {
                throw new ErrorException($"Error during Getting amenity with id '{request.AmenityId}'.", exception);
            }
        }
    }
}
