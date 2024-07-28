using FluentValidation;

namespace Application.CommandsAndQueries.BookingCQ.Commands.Create
{
    public class CreateRoomBookingValidation : AbstractValidator<CreateRoomBookingCommand>
    {
        public CreateRoomBookingValidation() {
            RuleFor(booking => booking.EndDate)
                .Must((x, endDate) => x.StartDate < endDate)
                .WithMessage("End date must be after start date");
            RuleFor(booking => booking.StartDate)
                .Must((startDate) => startDate >= DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Start date must be today or later");
            RuleFor(booking => booking.RoomsNumbers)
                .NotEmpty()
                .ForEach((x) => x.NotEmpty().MaximumLength(20));
            RuleFor(booking => booking.SpecialOfferId)
                .MaximumLength(100);         
        }           
    }
}
