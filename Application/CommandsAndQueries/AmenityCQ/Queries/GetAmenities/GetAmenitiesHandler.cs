using Application.Dtos.AmenityDtos;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.CommandsAndQueries.AmenityCQ.Query.GetAmenities
{
    internal class GetAmenitiesHandler : IRequestHandler<GetAmenitiesQuery,
        (IEnumerable<AmenityDto> Amenities, int TotalRecords, int Page, int PageSize)>
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

        public async Task<(IEnumerable<AmenityDto> Amenities, int TotalRecords, int Page, int PageSize)> Handle(
            GetAmenitiesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (amenities, totalRecords) = await _amenityRepository.GetAsync(request.Page, request.PageSize);
                return
                    (
                        _mapper.Map<IEnumerable<AmenityDto>>(amenities),
                        totalRecords,
                        request.Page,
                        request.PageSize
                    );
            }
            catch (Exception exception)
            {

                throw new ErrorException($"Error during Getting amenities.", exception);
            }
        }
    }
}
