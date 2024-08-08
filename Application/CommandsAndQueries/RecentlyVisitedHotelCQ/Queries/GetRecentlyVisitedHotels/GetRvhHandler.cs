using Application.Dtos.HotelDtos;
using Application.Dtos.RecentlyVisitedHotelDto;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.CommandsAndQueries.RecentlyVisitedHotelCQ.Queries.GetRecentlyVisitedHotels
{
    internal class GetRvhHandler : IRequestHandler<GetRvhQuery, IEnumerable<RvhDto>>
    {
        private readonly IRecentlyVisitedHotelRepository _repository;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public GetRvhHandler(IRecentlyVisitedHotelRepository repository, UserManager<User> userManager)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Hotel, HotelMinWithRatingDto>()
                    .ForMember(dest => dest.City,
                        opt => opt.MapFrom(src => src.City.Name))
                    .ForMember(dest => dest.Country,
                        opt => opt.MapFrom(src => src.City.Country));
                cfg.CreateMap<RecentlyVisitedHotel, RvhDto>();
            });
            _mapper = configuration.CreateMapper();
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }
        public async Task<IEnumerable<RvhDto>> Handle(GetRvhQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId)
                ?? throw new NotFoundException("User not found!");
                var result = await _repository.GetAsync(request.UserId);
                var recentlyVisitedDto = _mapper.Map<IEnumerable<RvhDto>>(result.Keys);
                for (var i = 0; i < result.Count; i++)
                {
                    recentlyVisitedDto.ElementAt(i).hotel.Rating = result.ElementAt(i).Value;
                }
                return recentlyVisitedDto;

            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {

                throw new ErrorException("Error during getting the recently visited hotels by user", exception);
            }
        }
    }
}
