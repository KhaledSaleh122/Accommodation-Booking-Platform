
using Domain.Entities;

namespace Domain.Abstractions
{
    public interface ISpecialOfferRepository
    {
        Task<SpecialOffer> CreateAsync(SpecialOffer specialOffer);
        Task<SpecialOffer?> GetByIdAsync(string id);
        Task<IEnumerable<SpecialOffer>> GetTopSpecialFeatureOffers();
    }
}
