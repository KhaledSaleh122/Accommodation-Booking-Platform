using Domain.Abstractions;
using FluentValidation;

namespace Application.CommandsAndQueries.HotelAmenityCQ.Commands.Create
{
    public class AddAmenityToHotelCommandValidation : AbstractValidator<AddAmenityToHotelCommand>
    {
        private readonly IHotelRepository _hotelRepository;
        public AddAmenityToHotelCommandValidation(IHotelRepository hotelRepository)
        {
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
            RuleFor(m => m).CustomAsync
                (
                    async (command, context, token) =>
                    {
                        var isExist = await _hotelRepository.AmenityExistsAsync(command.HotelId, command.AmenityId);
                        if (isExist)
                        {
                            context.AddFailure("AmenityId", "The hotel already has this amenity.");
                        }
                    }
                );
        }
    }
}
