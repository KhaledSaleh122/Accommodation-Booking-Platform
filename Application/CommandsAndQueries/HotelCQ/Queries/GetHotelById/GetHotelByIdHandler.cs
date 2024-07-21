using Application.Dtos.AmenityDtos;
using Application.Dtos.HotelDtos;
using Application.Dtos.RoomDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.CommandsAndQueries.HotelCQ.Query.GetHotelById
{
    public class GetHotelByIdHandler : IRequestHandler<GetHotelByIdQuery, HotelFullDto?>
    {
        private readonly IMapper _mapper;
        private readonly IHotelRepository _hotelRepository;
        public GetHotelByIdHandler(IHotelRepository hotelRepository)
        {

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Hotel, HotelFullDto>()
                    .ForMember(dest => dest.Images,
                    opt =>
                        opt.MapFrom(
                            src => src.Images.Select(x => x.Path).ToList()
                        )
                    )
                   .ForMember(dest => dest.Amenities,
                   opt =>
                        opt.MapFrom(
                            src => src.HotelAmenity.Select(am => am.Amenity)
                        )
                   )
                   .ForMember(dest => dest.City,
                   opt =>
                        opt.MapFrom(src => src.City.Name)
                   )
                   .ForMember(dest => dest.Country,
                   opt =>
                        opt.MapFrom(src => src.City.Country)
                   );
                cfg.CreateMap<Amenity, AmenityDto>();
                cfg.CreateMap<Room, RoomDto>()
                    .ForMember(dest => dest.Images, opt =>
                        opt.MapFrom(
                          src => src.Images.Select(x => x.Path).ToList()
                        )
                    );
            });
            _mapper = configuration.CreateMapper();
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
        }
        public async Task<HotelFullDto?> Handle(
                GetHotelByIdQuery request,
                CancellationToken cancellationToken
            )
        {
            try
            {
                var hotel = await _hotelRepository.GetByIdAsync(request.HotelId) ??
                    throw new NotFoundException("Hotel not found!");
                return _mapper.Map<HotelFullDto>(hotel);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {

                throw new ErrorException($"Error during Getting hotel with id '{request.HotelId}'.", exception);
            }
        }
    }
}
