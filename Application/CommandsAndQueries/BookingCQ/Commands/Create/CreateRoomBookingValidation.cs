using FluentValidation;

namespace Application.CommandsAndQueries.BookingCQ.Commands.Create
{
    public class CreateRoomBookingValidation : AbstractValidator<CreateRoomBookingCommand>
    {
        public CreateRoomBookingValidation() {
            RuleFor(booking => booking.StartDate)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Start date must be today or later");
            RuleFor(booking => booking.EndDate)
                .Must((booking,endDate) => endDate > booking.StartDate)
                .WithMessage("End date must be after start date");
        }           
    }
}
