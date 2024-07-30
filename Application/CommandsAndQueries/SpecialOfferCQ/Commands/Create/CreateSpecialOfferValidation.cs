using FluentValidation;

namespace Application.CommandsAndQueries.SpecialOfferCQ.Commands.Create
{
    public class CreateSpecialOfferValidation : AbstractValidator<CreateSpecialOfferCommand>
    {
        public CreateSpecialOfferValidation()
        {
            RuleFor(sp => sp.DiscountPercentage)
                .GreaterThanOrEqualTo(1)
                .LessThanOrEqualTo(100);
            RuleFor(sp => sp.ExpireDate)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
            RuleFor(sp => sp.OfferType)
                .NotEmpty()
                .IsInEnum();
        }
    }
}
