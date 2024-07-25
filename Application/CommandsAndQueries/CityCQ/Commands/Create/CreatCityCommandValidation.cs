using Application.Validation;
using Domain.Abstractions;
using FluentValidation;

namespace Application.CommandsAndQueries.CityCQ.Commands.Create
{
    public class CreatCityCommandValidation : AbstractValidator<CreateCityCommand>
    {
        public CreatCityCommandValidation(IImageService imageService)
        {
            RuleFor(city => city.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(50);
            RuleFor(city => city.Thumbnail).Cascade(CascadeMode.Stop)
                  .NotEmpty()
                  .Custom(
                    (image, context) =>
                        imageService.ValidateImage(image, context, "Thumbnail")
                  );
            RuleFor(city => city.Country)
                .NotEmpty()
                .MaximumLength(50);
            RuleFor(city => city.PostOffice)
                .NotEmpty()
                .MaximumLength(20);
            RuleFor(city => city.PostOffice)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(20);
        }
    }
}
