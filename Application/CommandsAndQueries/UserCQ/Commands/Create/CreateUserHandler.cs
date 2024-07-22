using Application.Dtos.UserDtos;
using Application.Execptions;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Application.CommandsAndQueries.UserCQ.Commands.Create
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand, UserDto>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        public CreateUserHandler(UserManager<User> userManager)
        {
            this._userManager = userManager;
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserDto>();
            });
            _mapper = configuration.CreateMapper();
        }
        public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {

            var user = new User()
            {
                UserName = request.UserName,
                Email = request.Email,
            };
            try
            {
                var userWithThisEmail = await _userManager.FindByEmailAsync(request.Email);
                if (userWithThisEmail is not null)
                    throw new ErrorException("Email already exists.")
                    {
                        StatusCode = StatusCodes.Status409Conflict
                    };
                var userWithThisUsername = await _userManager.FindByNameAsync(request.UserName);
                if (userWithThisUsername is not null)
                    throw new ErrorException("Username already exists.")
                    {
                        StatusCode = StatusCodes.Status409Conflict
                    };
                var result = await _userManager.CreateAsync(user, request.Password);
                await _userManager.AddToRoleAsync(user, "User");
                if (!result.Succeeded)
                {
                    throw new ValidationException(
                        $"Error during creating new User.",
                        result.Errors.Select(x =>
                        {
                            string? name = null;
                            if (x.Code.Contains("UserName", StringComparison.CurrentCultureIgnoreCase))
                                name = "Username";
                            else if (x.Code.Contains("Password", StringComparison.CurrentCultureIgnoreCase))
                                name = "Password";
                            else if (x.Code.Contains("Email", StringComparison.CurrentCultureIgnoreCase))
                                name = "Email";

                            return new ValidationFailure() { PropertyName = name ?? x.Code, ErrorMessage = x.Description };
                        })
                   );
                }

                return _mapper.Map<UserDto>(user);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception exception)
            {

                throw new ErrorException("Error during create the user account", exception);
            }
        }
    }
}
