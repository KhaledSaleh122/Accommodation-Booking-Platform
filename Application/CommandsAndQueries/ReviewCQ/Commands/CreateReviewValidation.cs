using Domain.Abstractions;
using FluentValidation;

namespace Application.CommandsAndQueries.ReviewCQ.Commands
{
    public class CreateReviewValidation : AbstractValidator<CreateReviewCommand>
    {
        public CreateReviewValidation() {
            RuleFor(review => review.Rating)
                .Cascade(CascadeMode.Stop)
                .ExclusiveBetween(1, 5);
        }
    }
}
