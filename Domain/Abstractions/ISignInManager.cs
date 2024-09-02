using Domain.Entities;

namespace Domain.Abstractions
{
    public interface ISignInManager
    {
        Task<bool> CheckPasswordSignInAsync(User user, string password, bool v);
    }
}
