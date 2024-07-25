﻿using Application.Dtos.AmenityDtos;
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
            try
            {
                var (result, totalRecords) = await _hotelRepository.GetAsync
                    (
                        request.Page, 
                        request.PageSize,
                        minPrice, 
                        maxPrice,
                        request.City,
                        request.Country,
                        request.HotelType,
                        request.HotelName,
                        request.Owner,
                        request.Aminites
                    );
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
