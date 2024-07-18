using FluentValidation;

namespace Application.CommandsAndQueries.AmenityCQ.Commands.Create
{
    public sealed class CreateAmenityCommandValidation : AbstractValidator<CreateAmenityCommand>
    {
        public CreateAmenityCommandValidation() {
            RuleFor(amenity => amenity.Name)
                .NotEmpty()
                .MaximumLength(60);
            RuleFor(amenity => amenity.Description)
                .NotEmpty()
                .MaximumLength(160);
        }
    }
}
