using Application.Validation;
using Domain.Abstractions;
using FluentValidation;
using FluentValidation.Validators;

namespace Application.CommandsAndQueries.UserCQ.Commands.Create
{
    public class CreateUserValidation : AbstractValidator<CreateUserCommand>
    {

        public CreateUserValidation(IImageService imageService)
        {
            RuleFor(user => user.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(50)
                .EmailAddress(EmailValidationMode.AspNetCoreCompatible);
            RuleFor(user => user.Password)
                .NotEmpty()
                .MaximumLength(55)
                .MinimumLength(6);
            RuleFor(user => user.UserName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MinimumLength(1)
                .MaximumLength(50);
            RuleFor(user => user.Thumbnail).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Custom(
                (image, context) =>
                    imageService.ValidateImage(image, context, "Thumbnail")
                );

        }
    }
}

