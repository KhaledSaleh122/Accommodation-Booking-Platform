using Application.Dtos.SpecialOfferDtos;
using MediatR;

namespace Application.CommandsAndQueries.SpecialOfferCQ.Queries.GetTopFeatureDealOffers
{
    public class GetTopSpecialFeatureOffersQuery : IRequest<IEnumerable<FeaturedDealsDto>>
    {
    }
}
