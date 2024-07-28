using Application.Dtos.SpecialOfferDtos;
using Application.Exceptions;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.CommandsAndQueries.SpecialOfferCQ.Commands
{
    internal class CreateSpecialOfferHandler : IRequestHandler<CreateSpecialOfferCommand, SpecialOfferDto>
    {
        private readonly ISpecialOfferRepository _specialOfferRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IMapper _mapper;

        public CreateSpecialOfferHandler(ISpecialOfferRepository specialOfferRepository, IHotelRepository hotelRepository)
        {
            _specialOfferRepository = specialOfferRepository ?? throw new ArgumentNullException(nameof(specialOfferRepository));
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CreateSpecialOfferCommand, SpecialOffer>();
                cfg.CreateMap<SpecialOffer, SpecialOfferDto>();
            });
            _mapper = configuration.CreateMapper();
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
        }
        public async Task<SpecialOfferDto> Handle(CreateSpecialOfferCommand request, CancellationToken cancellationToken)
        {
            var specialOffer = _mapper.Map<SpecialOffer>(request);
            var id = String.IsNullOrEmpty(request.Id) ? Guid.NewGuid().ToString() : request.Id;
            specialOffer.Id = id;
            try
            {
                var hotel = await _hotelRepository.GetByIdAsync(request.hotelId)
                    ?? throw new NotFoundException("Hotel not found!");
                var specialOffer_ = await _specialOfferRepository.GetByIdAsync(id);
                if (specialOffer_ is not null)
                    throw new ErrorException("A special offer with this id already exists!")
                    {
                        StatusCode = StatusCodes.Status409Conflict
                    };
                var createdSpecialOffer = await _specialOfferRepository.CreateAsync(specialOffer);
                return _mapper.Map<SpecialOfferDto>(createdSpecialOffer);
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {

                throw new ErrorException($"Error during creating new special offer.", exception);
            }
        }
    }
}
