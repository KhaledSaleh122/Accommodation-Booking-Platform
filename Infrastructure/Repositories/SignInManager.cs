using Domain.Abstractions;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
namespace Infrastructure.Repositories
{
    public class SignInManager : ISignInManager
    {
        private readonly SignInManager<User> _signInManager;

        public SignInManager(SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<bool> CheckPasswordSignInAsync(User user, string password, bool v)
        {
            return (await _signInManager.CheckPasswordSignInAsync(user, password, v)).Succeeded;
        }
    }
}
