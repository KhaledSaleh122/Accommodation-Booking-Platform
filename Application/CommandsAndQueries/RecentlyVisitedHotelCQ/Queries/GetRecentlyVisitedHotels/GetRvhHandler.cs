using Application.Dtos.HotelDtos;
using Application.Dtos.RecentlyVisitedHotelDto;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;

namespace Application.CommandsAndQueries.RecentlyVisitedHotelCQ.Queries.GetRecentlyVisitedHotels
{
    public class GetRvhHandler : IRequestHandler<GetRvhCommand, IEnumerable<RvhDto>>
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
                    .ForMember(dest => dest.Rating,
                        opt => opt.MapFrom(src => src.Reviews.Count > 0 ? src.Reviews.Average(r => r.Rating) : 0)
                    )
                    .ForMember(dest => dest.City,
                        opt => opt.MapFrom(src => src.City.Name))
                    .ForMember(dest => dest.Country,
                        opt => opt.MapFrom(src => src.City.Country));
                cfg.CreateMap<RecentlyVisitedHotel, RvhDto>();
            });
            _mapper = configuration.CreateMapper();
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }
        public async Task<IEnumerable<RvhDto>> Handle(GetRvhCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId)
                ?? throw new NotFoundException("User not found!");
                var recentlyVisited = await _repository.GetAsync(request.UserId);
                return _mapper.Map<IEnumerable<RvhDto>>(recentlyVisited);
            }
            catch (NotFoundException) {
                throw;
            }
            catch (Exception exception)
            {

                throw new ErrorException("Error during getting the recently visited hotels by user", exception);
            }
        }
    }
}
