using Application.Validation;
using MediatR.Pipeline;

namespace Booking_API_Project.Configurations
{
    public static class AddPreProcessor
    {
        public static MediatRServiceConfiguration RegisterPreProcessor<TRequest>(this MediatRServiceConfiguration services) where TRequest : notnull {
            services.AddRequestPreProcessor<IRequestPreProcessor<TRequest>, ValidationProcessor<TRequest>>();
            return services;
        }
    }
}
