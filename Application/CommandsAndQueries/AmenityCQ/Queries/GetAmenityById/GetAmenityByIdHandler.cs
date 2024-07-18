using Application.Dtos.AmenityDtos;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.CommandsAndQueries.AmenityCQ.Query.GetAmenityById
{
    public class GetAmenityByIdHandler : IRequestHandler<GetAmenityByIdQuery, AmenityDto?>
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
            Amenity? amenity;
            try
            {
                amenity = await _amenityRepository.GetByIdAsync(request.AmenityId);
            }
            catch (Exception)
            {

                throw new ErrorException($"Error during Getting amenity with id '{request.AmenityId}'.");
            }
            if (amenity is null) return null;
            return _mapper.Map<AmenityDto>(amenity);
        }
    }
}
