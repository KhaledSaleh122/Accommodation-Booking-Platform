using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Application.CommandsAndQueries.UserCQ.Commands.Create
{
    public class CreateUserValidation : AbstractValidator<CreateUserCommand>
    {
        public CreateUserValidation(UserManager<User> userManager) {
            RuleFor(user => user.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(50)
                .EmailAddress()
                .MustAsync(async (email, token) => {
                    var user = await userManager.FindByEmailAsync(email);
                    return user is null;
                })
                .WithMessage("Email already exists.");
            RuleFor(user => user.Password)
                .NotEmpty()
                .MaximumLength(30)
                .MinimumLength(6);
            RuleFor(user => user.UserName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MinimumLength(1)
                .MaximumLength(50)
                .MustAsync( async(username, token) => {
                   var user = await userManager.FindByNameAsync(username);
                    return user is null;
                })
                .WithMessage("Username already exists.");

        }
    }
}
