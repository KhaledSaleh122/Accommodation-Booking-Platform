using Application.CommandsAndQueries.CityCQ.Commands.Update;
using Domain.Enums;
using FluentValidation;

namespace Application.CommandsAndQueries.HotelCQ.Commands.Update
{
    public class UpdateHotelCommandValidation : AbstractValidator<UpdateHotelCommand>
    {
        public UpdateHotelCommandValidation()
        {
            RuleFor(hotel => hotel.Name).MaximumLength(50);
            RuleFor(hotel => hotel.Owner).MaximumLength(50);
            RuleFor(hotel => hotel.Address).MaximumLength(100);
            RuleFor(hotel => hotel.Description).MaximumLength(160);
            RuleFor(hotel => hotel.PricePerNight).GreaterThanOrEqualTo(0);
            RuleFor(hotel => hotel.HotelType)
                .Must(
                    ht =>
                        Enum.IsDefined
                        (
                            typeof(HotelType),
                            ht!
                        )
                )
                .When(hotel => hotel.HotelType is not null)
                .WithMessage("The hotel type is wrong.");
        }
    }
}
