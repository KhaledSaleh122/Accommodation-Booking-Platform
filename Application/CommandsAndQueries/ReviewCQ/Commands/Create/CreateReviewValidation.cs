using Domain.Abstractions;
using FluentValidation;

namespace Application.CommandsAndQueries.ReviewCQ.Commands.Create
{
    public class CreateReviewValidation : AbstractValidator<CreateReviewCommand>
    {
        public CreateReviewValidation()
        {
            RuleFor(review => review.Rating)
                .Cascade(CascadeMode.Stop)
                .InclusiveBetween(1, 5);
        }
    }
}
