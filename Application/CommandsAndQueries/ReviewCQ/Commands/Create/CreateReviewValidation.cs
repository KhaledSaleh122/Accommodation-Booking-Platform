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
            RuleFor(review => review.Comment)
                .MaximumLength(255)
                .When(x => x.Comment is not null); 
        }
    }
}
