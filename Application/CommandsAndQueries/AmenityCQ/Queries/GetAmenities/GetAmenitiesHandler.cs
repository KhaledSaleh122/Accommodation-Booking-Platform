using Application.Dtos.AmenityDtos;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.CommandsAndQueries.AmenityCQ.Query.GetAmenities
{
    public class GetAmenitiesHandler : IRequestHandler<GetAmenitiesQuery, (IEnumerable<AmenityDto>, uint, uint, uint)>
    {
        private readonly IMapper _mapper;
        private readonly IAmenityRepository _amenityRepository;

        public GetAmenitiesHandler(IAmenityRepository amenityRepository)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Amenity, AmenityDto>();
            });
            _mapper = configuration.CreateMapper();
            _amenityRepository = amenityRepository ?? throw new ArgumentNullException(nameof(amenityRepository));
        }

        public async Task<(IEnumerable<AmenityDto>, uint, uint, uint)> Handle(GetAmenitiesQuery request, CancellationToken cancellationToken)
        {
            uint page = request.Page > 0 ? request.Page : 1;
            uint pageSize = request.PageSize > 0 && request.PageSize <= 100 ? request.PageSize : 10;
            IEnumerable<Amenity> amenities;
            uint totalRecords;
            try
            {
                (amenities, totalRecords) = await _amenityRepository.GetAsync(page, pageSize);
            }
            catch (Exception)
            {

                throw new ErrorException($"Error during Getting amenities.");
            }

            return (_mapper.Map<IEnumerable<AmenityDto>>(amenities),totalRecords,page,pageSize);
        }
    }
}
