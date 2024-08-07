using ABPIntegrationTests;
using Application.CommandsAndQueries.BookingCQ.Commands.Create;
using Application.Dtos.BookingDtos;
using AutoFixture;
using Domain.Entities;
using FluentAssertions;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ABP.Presentation.IntegrationTests.BookingControllerTests
{
    public class CreateRoomBookingTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly string _userToken;
        private readonly CreateRoomBookingCommand _command;
        private readonly string? _adminToken;

        public CreateRoomBookingTests(ABPWebApplicationFactory factory)
        {
            factory.DatabaseName = Guid.NewGuid().ToString();
            _client = factory.CreateClient();
            _fixture = new Fixture();
            var scope = factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            _configuration = serviceProvider.GetRequiredService<IConfiguration>();
            _userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            _dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _fixture.Customize<DateOnly>(composer => composer.FromFactory<int, int, int>((year, month, day) =>
            {
                var now = DateTime.Now;
                return new DateOnly(now.Year, now.Month, now.Day);
            }));
            _fixture.Customize<Hotel>(x => x
                .Without(h => h.Reviews)
                .Without(h => h.RecentlyVisitedHotels)
                .Without(h => h.Rooms)
                .Without(h => h.SpecialOffers)
            );
            factory.SetupDbContext(_dbContext).GetAwaiter().GetResult();
            JwtTokenHelper jwtTokenHelper = new(_configuration, _userManager);
            _userToken = jwtTokenHelper.GetJwtTokenAsync("User").GetAwaiter().GetResult();
            _adminToken = jwtTokenHelper.GetJwtTokenAsync("Admin").GetAwaiter().GetResult();
            _command = _fixture.Build<CreateRoomBookingCommand>()
                .With(x => x.StartDate, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
                .With(x => x.EndDate, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)))
                .Without(x => x.SpecialOfferId)
                .Create();
        }

        [Fact]
        public async Task CreateRoomBooking_Should_ReturnCreated_WhenSuccess()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();
            var room = _fixture.Build<Room>()
                .With(x => x.RoomNumber, _fixture.Create<string>()[0..20])
                .With(x => x.HotelId, hotel.Id)
                .Without(x => x.BookingRooms)
                .Without(x => x.Hotel)
                .Create();

            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.Rooms.AddAsync(room);
            await _dbContext.SaveChangesAsync();

            _command.HotelId = hotel.Id;
            _command.RoomsNumbers = [room.RoomNumber];
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/bookings", _command);
            var bookingRequestDto = response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<BookingRequestDto>()
                : null;

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            bookingRequestDto.Should().NotBeNull();
            bookingRequestDto?.ClientSecret.Should().NotBeNullOrEmpty();
            bookingRequestDto?.PaymentIntentId.Should().NotBeNullOrEmpty();
            bookingRequestDto?.Booking.Should().NotBeNull();
            bookingRequestDto?.Booking.Rooms.Should().Contain(_command.RoomsNumbers);
        }

        [Fact]
        public async Task CreateRoomBooking_Should_ReturnConflict_WhenRoomNotAvailable()
        {
            // Arrange
            var hotel = _fixture.Create<Hotel>();
            var room = _fixture.Build<Room>()
                .With(x => x.RoomNumber, _fixture.Create<string>()[0..20])
                .With(x => x.HotelId, hotel.Id)
                .Without(x => x.BookingRooms)
                .Without(x => x.Hotel)
                .Create();
            var booking = _fixture.Build<Booking>()
                .With(x => x.Id, 1)
                .With(x => x.StartDate, _command.StartDate)
                .With(x => x.EndDate, _command.EndDate)
                .With(x => x.BookingRooms, [new BookingRoom() { HotelId = hotel.Id, RoomNumber = room.RoomNumber, BookingId = 1 }])
                .Without(x => x.SpecialOffer)
                .Without(x => x.SpecialOfferId)
                .Create();

            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.Rooms.AddAsync(room);
            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            _command.HotelId = hotel.Id;
            _command.RoomsNumbers = [room.RoomNumber];
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/bookings", _command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task CreateRoomBooking_Should_ReturnBadRequest_WhenCommandIsNull()
        {
            //Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);

            // Act
            var response = await _client.PostAsync("/api/users/bookings", null);


            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateRoomBooking_Should_ReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/users/bookings", _command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateHotelReview_Should_ReturnForbidden_WhenUserIsNotAuthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            // Act
            var response = await _client.PostAsJsonAsync("/api/hotels/1/reviews", _command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
