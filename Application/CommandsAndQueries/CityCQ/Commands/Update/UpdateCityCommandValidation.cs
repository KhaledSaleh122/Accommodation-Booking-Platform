using Domain.Abstractions;
using FluentValidation;

namespace Application.CommandsAndQueries.CityCQ.Commands.Update
{
    public class UpdateCityCommandValidation : AbstractValidator<UpdateCityCommand>
    {
        public UpdateCityCommandValidation(ICityRepository cityRepository) {
            RuleFor(city => city.Name)
                .NotEmpty()
                .MaximumLength(50);
            RuleFor(city => city.Country)
                .NotEmpty()
                .MaximumLength(50);
            RuleFor(city => city)
                .CustomAsync(async (command, context ,token) =>
                {
                    var isExist =  await cityRepository.DoesCityExistInCountryAsync(
                        command.Name!, 
                        command.Country!);
                    if (isExist) {
                        context.AddFailure("Name", "A city with this name already exists in the country.");
                    }
                }).When(
                city => 
                    !String.IsNullOrEmpty(city.Name) && city.Name.Length <= 50 &&
                    !String.IsNullOrEmpty(city.Country) && city.Country.Length <= 50
                );
            RuleFor(city => city.PostOffice).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(20)
                .MustAsync(
                    async (postOffice, CancellationToken) =>
                    !(await cityRepository.DoesPostOfficeExistsAsync(postOffice))
                )
                .WithMessage("This post office exists in a city.");
        }
    }
}
