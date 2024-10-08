﻿using Application.Dtos.UserDtos;
using Application.Execptions;
using Domain.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Application.CommandsAndQueries.UserCQ.Commands.SignInGoogle
{
    internal class SignInGoogleHandler : IRequestHandler<SignInGoogleCommand, UserSignInDto>
    {
        private readonly IUserManager _userManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public SignInGoogleHandler(IUserManager userManager, IConfiguration configuration, ITokenService tokenService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }
        public async Task<UserSignInDto> Handle(SignInGoogleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var username = request.Claims.Where(x => x.Type == ClaimTypes.Name).First().Value;
                var name = string.Empty;
                foreach (var letter in username)
                {
                    if (Char.IsLetterOrDigit(letter)) name += letter;
                }
                var email = request.Claims.Where(x => x.Type == ClaimTypes.Email).First().Value;
                var id = request.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).First().Value;
                var user = new User() { Id = id, Email = email, UserName = name, Thumbnail = "user.jpg" };
                var userAccount = await _userManager.FindByIdAsync(user.Id);
                var userRoles = await _userManager.GetRolesAsync(user);
                if (userAccount is null)
                {
                    var userByEmail = await _userManager.FindByEmailAsync(user.Email);
                    if (userByEmail is not null)
                        throw new ErrorException("User with this email already exist")
                        {
                            StatusCode = StatusCodes.Status409Conflict
                        };
                    var userByName = await _userManager.FindByNameAsync(user.UserName);
                    if (userByName is not null)
                        throw new ErrorException("User with this username already exist")
                        {
                            StatusCode = StatusCodes.Status409Conflict
                        };
                    var (success, errors) = await _userManager.CreateAsync(user);
                    if(!success)
                        throw new ErrorException("An error occurred while creating the account")
                        {
                            StatusCode = StatusCodes.Status500InternalServerError
                        };
                    await _userManager.AddToRoleAsync(user, "User");
                    userRoles = [.. userRoles, "User"];
                }

                var (token, expireDate) = _tokenService.GenerateUserToken(userAccount ?? user, userRoles);
                return new UserSignInDto() { Token = token, Expiration = expireDate };
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception exception)
            {

                throw new ErrorException("Error during sing in using google", exception);
            }
        }
    }
}
