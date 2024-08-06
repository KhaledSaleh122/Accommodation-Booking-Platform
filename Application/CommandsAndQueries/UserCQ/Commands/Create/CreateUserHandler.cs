using Application.Dtos.UserDtos;
using Application.Execptions;
using AutoMapper;
using Domain.Abstractions;
using Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Application.CommandsAndQueries.UserCQ.Commands.Create
{
    internal class CreateUserHandler : IRequestHandler<CreateUserCommand, UserDto>
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly ITransactionService _transactionService;
        private readonly IImageService _imageRepository;
        public CreateUserHandler(
            UserManager<User> userManager,
            ITransactionService transactionService,
            IImageService imageRepository,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserDto>();
            });
            _mapper = configuration.CreateMapper();
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }
        public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var storePath = "UsersThumbnails";
            var thumnailName = Guid.NewGuid().ToString();
            var user = new User()
            {
                UserName = request.UserName,
                Email = request.Email,
                Thumbnail = $"{storePath}/{thumnailName}{Path.GetExtension(request.Thumbnail.FileName)}"
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
                await _transactionService.BeginTransactionAsync();
                var result = await _userManager.CreateAsync(user, request.Password);
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
                await _userManager.AddToRoleAsync(user, "User");
                _imageRepository.UploadFile(request.Thumbnail, $"{storePath}", thumnailName);
                await _transactionService.CommitTransactionAsync();
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
                await _transactionService.RollbackTransactionAsync();
                _imageRepository.DeleteFile(user.Thumbnail, true);
                throw new ErrorException("Error during create the user account", exception);
            }
        }
    }
}
