using Application.Validation;
using MediatR.Pipeline;

namespace Accommodation_Booking_Platform.Configurations
{
    public static class AddPreProcessor
    {
        public static MediatRServiceConfiguration RegisterPreProcessor<TRequest>(this MediatRServiceConfiguration services) where TRequest : notnull {
            services.AddRequestPreProcessor<IRequestPreProcessor<TRequest>, ValidationProcessor<TRequest>>();
            return services;
        }
    }
}
