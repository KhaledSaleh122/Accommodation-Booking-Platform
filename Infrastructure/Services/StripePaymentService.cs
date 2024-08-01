using Domain.Abstractions;
using Stripe;

namespace Infrastructure.Services
{
    public class StripePaymentService : IPaymentService<PaymentIntent, PaymentIntentCreateOptions>
    {
        private readonly PaymentIntentService _paymentIntentService;

        public StripePaymentService(PaymentIntentService paymentIntentService)
        {
            _paymentIntentService = paymentIntentService ?? throw new ArgumentNullException(nameof(paymentIntentService));
        }

        public Task<PaymentIntent> CreateAsync(PaymentIntentCreateOptions options, CancellationToken cancellationToken)
        {
            return _paymentIntentService.CreateAsync(options, null, cancellationToken);
        }

        public Task<PaymentIntent> GetAsync(string id, CancellationToken cancellationToken)
        {
            return _paymentIntentService.GetAsync(id,null,null,cancellationToken);
        }
    }
}
