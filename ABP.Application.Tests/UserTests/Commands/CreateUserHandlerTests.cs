using Application.CommandsAndQueries.UserCQ.Commands.Create;
using Application.Dtos.UserDtos;
using Application.Execptions;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Net.Mail;
using System.Text;

namespace ABP.Application.Tests.UserTests.Commands
{
    public class CreateUserHandlerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IImageService> _imageRepositoryMock;
        private readonly Mock<ITransactionService> _transactionServiceMock;
        private readonly CreateUserCommand _command;
        private readonly CreateUserHandler _handler;
        private readonly IFixture _fixture;

        public CreateUserHandlerTests()
        {
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);
            _imageRepositoryMock = new();
            _transactionServiceMock = new();
            var bytes = Encoding.UTF8.GetBytes("This is a dummy file");
            IFormFile testFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "dummy", "dummy.png");
            _fixture = new Fixture();
            _command = new CreateUserCommand()
            {
                Email = _fixture.Create<MailAddress>().Address,
                Password = _fixture.Create<Guid>().ToString(),
                Thumbnail = testFile,
                UserName = _fixture.Create<string>()
            };
            _handler = new CreateUserHandler(
                _userManagerMock.Object, _transactionServiceMock.Object, _imageRepositoryMock.Object);
        }

        [Fact]
        public async Task Handler_Should_ReturnCreatedUser_WhenSuccess()
        {
            //Arrange
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            //Act
            UserDto result = await _handler.Handle(_command, default);
            //Assert
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handler_Should_ThrowsErrorException_WhenUserWithTheEmailExists()
        {
            //Arrange
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(new User());
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
            await result.Should().ThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task Handler_Should_ThrowsErrorException_WhenUserWithTheUsernameExists()
        {
            //Arrange
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new User());
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
            await result.Should().ThrowAsync<ErrorException>();
        }
        [Fact]
        public async Task Handler_Should_ThrowsValidationException_WhenCreateUserFail()
        {
            //Arrange
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed([]));
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
            await result.Should().ThrowAsync<ValidationException>();
        }
        [Fact]
        public async Task Handler_Should_AddUserToRole_WhenCreateUserSucess()
        {
            //Arrange
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            //Act
            UserDto result = await _handler.Handle(_command, default);
            //Assert
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handler_Should_ThrowsErrorException_WhenFail()
        {
            //Arrange
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ThrowsAsync(new Exception());
            //Act
            Func<Task> result = async () => await _handler.Handle(_command, default);
            //Assert
            await result.Should().ThrowAsync<ErrorException>();
        }
    }
}
