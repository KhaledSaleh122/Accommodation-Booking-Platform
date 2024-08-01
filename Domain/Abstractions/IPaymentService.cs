using Microsoft.Extensions.Options;

namespace Domain.Abstractions
{
    public interface IPaymentService<TReturn,Toptions>
    {
        public Task<TReturn> GetAsync(string id, CancellationToken cancellationToken);
        public Task<TReturn> CreateAsync(Toptions options, CancellationToken cancellationToken);
    }
}
