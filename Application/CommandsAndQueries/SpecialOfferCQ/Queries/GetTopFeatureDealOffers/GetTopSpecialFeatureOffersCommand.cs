using Application.Dtos.SpecialOfferDtos;
using MediatR;

namespace Application.CommandsAndQueries.SpecialOfferCQ.Queries.GetTopFeatureDealOffers
{
    public class GetTopSpecialFeatureOffersCommand : IRequest<IEnumerable<FeaturedDealsDto>>
    {
    }
}
