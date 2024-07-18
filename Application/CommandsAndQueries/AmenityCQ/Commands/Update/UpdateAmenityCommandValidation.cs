using FluentValidation;

namespace Application.CommandsAndQueries.AmenityCQ.Commands.Update
{
    public class UpdateAmenityCommandValidation : AbstractValidator<UpdateAmenityCommand>
    {
        public UpdateAmenityCommandValidation() {
            RuleFor(amenity => amenity.Name)
                .NotEmpty()
                .MaximumLength(60);
            RuleFor(amenity => amenity.Description)
                .NotEmpty()
                .MaximumLength(160);
        }
    }
}
