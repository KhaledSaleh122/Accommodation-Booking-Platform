using Application.Dtos.AmenityDtos;
using Application.Dtos.HotelDtos;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.CommandsAndQueries.HotelCQ.Query.GetHotels
{
    public class GetHotelsHandler : IRequestHandler<GetHotelsQuery, (IEnumerable<HotelDto>, int, int, int)>
    {
        private readonly IMapper _mapper;
        private readonly IHotelRepository _hotelRepository;

        public GetHotelsHandler(IHotelRepository hotelRepository)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Hotel, HotelDto>()
                    .ForMember(m => m.Amenities, m => m.MapFrom(src => src.HotelAmenity.Select(am => am.Amenity)))
                    .ForMember(m => m.City, m => m.MapFrom(src => src.City.Name))
                    .ForMember(m => m.Country, m => m.MapFrom(src => src.City.Country));
                cfg.CreateMap<Amenity, AmenityDto>();
            });
            _mapper = configuration.CreateMapper();
            _hotelRepository = hotelRepository;
        }

        public async Task<(IEnumerable<HotelDto>, int, int, int)> Handle(
                GetHotelsQuery request,
                CancellationToken cancellationToken
            )
        {
            int page = request.Page > 0 ? request.Page : 1;
            int pageSize = request.PageSize > 0 && request.PageSize <= 100 ? request.PageSize : 10; 
            decimal minPrice = request.MinPrice >= 0 ? request.MinPrice : 0;
            decimal? maxPrice = request.MaxPrice >= request.MinPrice ? request.MaxPrice : null;
            IEnumerable<Hotel> hotels;
            int totalRecords;
            try
            {
                (hotels, totalRecords) = await _hotelRepository.GetAsync
                    (
                        page, 
                        pageSize,
                        minPrice, 
                        maxPrice,
                        request.City,
                        request.Country,
                        request.HotelType,
                        request.HotelName,
                        request.Owner,
                        request.Aminites
                    );
            }
            catch (Exception exception)
            {

                throw new ErrorException($"Error during Getting hotels.", exception);
            }

            return (_mapper.Map<IEnumerable<HotelDto>>(hotels), totalRecords, page, pageSize);
        }
    }
}
