using FluentValidation;
using MediatR.Pipeline;

namespace Application.Validation
{
    public class ValidationProcessor<TRequest> : IRequestPreProcessor<TRequest> where TRequest : notnull
    {
        private readonly IValidator<TRequest> _validator;
        public ValidationProcessor(IValidator<TRequest> validator)
        {
            _validator = validator;
        }
        public async Task Process(TRequest request, CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(request, cancellationToken);
        }
    }
}
