using Application.Validation;
using Domain.Abstractions;
using FluentValidation;

namespace Application.CommandsAndQueries.CityCQ.Commands.Update
{
    public class UpdateCityCommandValidation : AbstractValidator<UpdateCityCommand>
    {
        public UpdateCityCommandValidation(IImageService imageService) {
            RuleFor(city => city.Name)
                .NotEmpty()
                .MaximumLength(50);
            RuleFor(city => city.Country)
                .NotEmpty()
                .MaximumLength(50);
            RuleFor(city => city.PostOffice).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(20);
            RuleFor(city => city.Thumbnail).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Custom(
                (image, context) =>
                    imageService.ValidateImage(image, context, "Thumbnail")
                );
        }
    }
}
