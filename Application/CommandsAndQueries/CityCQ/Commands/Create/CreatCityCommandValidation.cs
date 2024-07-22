using Domain.Abstractions;
using FluentValidation;

namespace Application.CommandsAndQueries.CityCQ.Commands.Create
{
    public class CreatCityCommandValidation : AbstractValidator<CreateCityCommand>
    {
        public CreatCityCommandValidation()
        {
            RuleFor(city => city.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(50);
                
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
