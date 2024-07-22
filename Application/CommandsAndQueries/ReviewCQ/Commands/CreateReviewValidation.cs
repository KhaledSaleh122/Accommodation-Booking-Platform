using FluentValidation;

namespace Application.CommandsAndQueries.ReviewCQ.Commands
{
    public class CreateReviewValidation : AbstractValidator<CreateReviewCommand>
    {
        public CreateReviewValidation() {
            RuleFor(review => review.Rating)
                .NotEmpty()
                .ExclusiveBetween(1,5);
        }
    }
}
