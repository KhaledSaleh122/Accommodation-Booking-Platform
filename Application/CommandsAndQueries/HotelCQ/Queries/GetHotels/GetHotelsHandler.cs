using Application.Dtos.AmenityDtos;
using Application.Dtos.HotelDtos;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using Domain.Params;
using MediatR;

namespace Application.CommandsAndQueries.HotelCQ.Query.GetHotels
{
    internal class GetHotelsHandler : IRequestHandler<GetHotelsQuery, (IEnumerable<HotelDto>, int, int, int)>
    {
        private readonly IMapper _mapper;
        private readonly IHotelRepository _hotelRepository;

        public GetHotelsHandler(IHotelRepository hotelRepository)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Hotel, HotelDto>()
                    .ForMember(dest => dest.Amenities,
                        opt => opt.MapFrom(src => src.HotelAmenity.Select(am => am.Amenity)))
                    .ForMember(dest => dest.City,
                        opt => opt.MapFrom(src => src.City.Name))
                    .ForMember(dest => dest.Country,
                        opt => opt.MapFrom(src => src.City.Country));
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
            decimal minPrice = request.MinPrice >= 0 ? request.MinPrice : 0;
            decimal? maxPrice = request.MaxPrice >= request.MinPrice ? request.MaxPrice : null;
            var checkIn = request.CheckIn ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var checkOut = request.CheckOut ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
            if (checkIn > checkOut || checkIn < DateOnly.FromDateTime(DateTime.UtcNow)) {
                checkIn = DateOnly.FromDateTime(DateTime.UtcNow);
                checkOut = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
            }
            var children = request.Children >= 0 ? request.Children : 0;
            var adult = request.Adult >= 0 ? request.Adult : 2;
            try
            {
                var search = new HotelSearch()
                {
                    Page = request.Page,
                    PageSize = request.PageSize,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    HotelName = request.HotelName,
                    Adult = adult,
                    Children = children,
                    Amenities = request.Amenities,
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    City = request.City,
                    Country = request.Country,
                    HotelType = request.HotelType,
                    Owner = request.Owner 
                };
                var (result, totalRecords) = await _hotelRepository.GetAsync(search);
                var hotels = _mapper.Map<IEnumerable<HotelDto>>(result.Keys);
                for (int i = 0; i < result.Count; i++){
                    hotels.ElementAt(i).Rating = result.ElementAt(i).Value;
                }
                return
                    (
                        hotels,
                        totalRecords,
                        request.Page,
                        request.PageSize
                    );
            }
            catch (Exception exception)
            {

                throw new ErrorException($"Error during Getting hotels.", exception);
            }
        }
    }
}
