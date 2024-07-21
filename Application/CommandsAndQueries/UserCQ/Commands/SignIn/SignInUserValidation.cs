using FluentValidation;
using System.Data;

namespace Application.CommandsAndQueries.UserCQ.Commands.SignIn
{
    public class SignInUserValidation : AbstractValidator<SignInUserCommand>
    {
        public SignInUserValidation() {
            RuleFor(user => user.Password)
                .NotEmpty();
            RuleFor(user => user.UserName)
                .NotEmpty();
        }
    }
}
