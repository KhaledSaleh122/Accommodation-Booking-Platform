using Application.Dtos.HotelDtos;
using Application.Dtos.SpecialOfferDtos;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using System;

namespace Application.CommandsAndQueries.SpecialOfferCQ.Queries.GetTopFeatureDealOffers
{
    internal class GetTopSpecialFeatureOffersHandler : IRequestHandler<GetTopSpecialFeatureOffersQuery, IEnumerable<FeaturedDealsDto>>
    {
        private readonly IMapper _mapper;
        private readonly ISpecialOfferRepository _specialOfferRepository;

        public GetTopSpecialFeatureOffersHandler(ISpecialOfferRepository specialOfferRepository)
        {
            _specialOfferRepository = specialOfferRepository ?? throw new ArgumentNullException(nameof(specialOfferRepository));
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SpecialOffer, FeaturedDealsDto>()
                    .ForMember(dest => dest.DiscountedPrice,
                        opt => opt.MapFrom(
                            src =>
                                src.Hotel.PricePerNight * ( 1 - ( (decimal)src.DiscountPercentage / 100))
                        )
                    )
                    .ForMember(dest => dest.OriginalPrice,
                        opt => opt.MapFrom(
                            src =>
                                src.Hotel.PricePerNight
                        )
                    );
                cfg.CreateMap<Hotel, HotelBaseDto>()
                    .ForMember(dest => dest.City,
                       opt =>
                            opt.MapFrom(src => src.City.Name)
                   )
                   .ForMember(dest => dest.Country,
                       opt =>
                            opt.MapFrom(src => src.City.Country)
                   );
            });
            _mapper = configuration.CreateMapper();
            _specialOfferRepository = specialOfferRepository;
        }
        public async Task<IEnumerable<FeaturedDealsDto>> Handle(
            GetTopSpecialFeatureOffersQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var specialOffer = await _specialOfferRepository.GetTopSpecialFeatureOffers();
                return _mapper.Map<IEnumerable<FeaturedDealsDto>>(specialOffer);
            }
            catch (Exception exception)
            {

                throw new ErrorException($"Error during getting Top Special Feature Offers.", exception);
            }
        }
    }
}
