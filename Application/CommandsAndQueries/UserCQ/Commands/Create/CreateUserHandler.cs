using Application.Dtos.UserDtos;
using Application.Execptions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;

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
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded) {
                throw new ErrorException($"Error during creating new User.");
            }

            return _mapper.Map<UserDto>(user);
        }
    }
}
