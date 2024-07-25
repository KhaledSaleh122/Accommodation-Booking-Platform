using Application.CommandsAndQueries.CityCQ.Commands.Update;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.CommandsAndQueries.HotelCQ.Commands.Update
{
    public class UpdateHotelHandler : IRequestHandler<UpdateHotelCommand>
    {
        private readonly IMapper _mapper;
        private readonly IHotelRepository _repository;

        public UpdateHotelHandler(IHotelRepository repository)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UpdateHotelCommand, Hotel>()
                    .ForMember(dest => dest.Address, 
                        opt => opt.Condition((src, dest, srcMember) => srcMember != null))
                    .ForMember(dest => dest.Description, 
                        opt => opt.Condition((src, dest, srcMember) => srcMember != null))
                    .ForMember(dest => dest.Name, 
                        opt => opt.Condition((src, dest, srcMember) => srcMember != null))
                    .ForMember(dest => dest.Owner, 
                        opt => opt.Condition((src, dest, srcMember) => srcMember != null))
                    .ForMember(dest => dest.PricePerNight, 
                        opt => opt.Condition((src, dest, srcMember) => src.PricePerNight != null))
                    .ForMember(dest => dest.HotelType, 
                        opt =>
                            {
                                opt.PreCondition((src, dest, srcMember) => src.HotelType != null);
                                opt.MapFrom(src => (HotelType)src.HotelType!);
                            }
                    );
            });
            _mapper = configuration.CreateMapper();
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        public async Task Handle(UpdateHotelCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var (hotel, avgReviews) = await _repository.GetByIdAsync(request.hotelId) ??
                    throw new NotFoundException($"Hotel not found!");
                _mapper.Map(request, hotel);
                await _repository.UpdateAsync(hotel);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new ErrorException($"Error during updaing hotel with id '{request.hotelId}'.", exception);
            }
        }
    }
}
