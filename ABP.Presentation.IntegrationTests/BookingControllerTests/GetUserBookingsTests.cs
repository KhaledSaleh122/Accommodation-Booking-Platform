using ABPIntegrationTests;
using Application.Dtos.BookingDtos;
using AutoFixture;
using Domain.Entities;
using FluentAssertions;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Responses.Pagination;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ABP.Presentation.IntegrationTests.BookingControllerTests
{
    public class GetUserBookingsTests : IClassFixture<ABPWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly string _userToken;
        private readonly string? _adminToken;

        public GetUserBookingsTests(ABPWebApplicationFactory factory)
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
        }

        [Fact]
        public async Task GetUserBookings_Should_ReturnOk_WhenSuccess()
        {
            //Arrange
            var bookingRoom = _fixture.Build<BookingRoom>()
                    .Without(x => x.Booking)
                    .Create();
            var userBooking = _fixture.Build<Booking>()
                .With(x => x.BookingRooms, [bookingRoom])
                .With(x => x.UserId, "UserId")
                .Without(x => x.User)
                .Create();

            await _dbContext.Bookings.AddAsync(userBooking);
            await _dbContext.SaveChangesAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);
            //Act
            var response = await _client.GetAsync($"api/users/{"UserId"}/bookings?page=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<BookingDto>>>();
            result.Should().NotBeNull();
            result?.Results.Should().HaveCount(1);
            result?.TotalRecords.Should().Be(1);
            result?.Page.Should().Be(1);
            result?.PageSize.Should().Be(10);
            result?.TotalPages.Should().Be(1);
        }

        [Fact]
        public async Task GetUserBookings_Should_ReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Act
            var response = await _client.GetAsync($"api/users/{"UserId"}/bookings?page=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetUserBookings_Should_ReturnForbidden_WhenUserAccessingOtherUsersBookings()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);

            // Act
            var response = await _client.GetAsync($"api/users/{"User1Id"}/bookings?page=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetUserBookings_Should_ReturnOk_WhenAdminAccessingAnyUsersBookings()
        {
            //Arrange
            var bookingRoom = _fixture.Build<BookingRoom>()
                    .Without(x => x.Booking)
                    .Create();
            var userBooking = _fixture.Build<Booking>()
                .With(x => x.BookingRooms, [bookingRoom])
                .With(x => x.UserId, "UserId")
                .Without(x => x.User)
                .Create();

            await _dbContext.Bookings.AddAsync(userBooking);
            await _dbContext.SaveChangesAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            //Act
            var response = await _client.GetAsync($"api/users/{"UserId"}/bookings?page=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<BookingDto>>>();
            result.Should().NotBeNull();
            result?.Results.Should().HaveCount(1);
            result?.TotalRecords.Should().Be(1);
            result?.Page.Should().Be(1);
            result?.PageSize.Should().Be(10);
            result?.TotalPages.Should().Be(1);
        }
        [Fact]
        public async Task GetUserBookings_Should_ReturnOk_WithPagination()
        {
            //Arrange

            var hotel = _fixture.Create<Hotel>();
            var room = _fixture.Build<Room>()
                .With(x => x.RoomNumber, _fixture.Create<string>()[0..20])
                .With(x => x.HotelId, hotel.Id)
                .Without(x => x.BookingRooms)
                .Without(x => x.Hotel)
                .Without(x => x.Images)
                .Create();
            List<Booking> userBookings = [];
            var count = 1;
            for (int i = 0; i < 10; i++)
            {
                var booking = new Booking()
                {
                    Id = i + 1,
                    StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(count++)),
                    EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(count++)),
                    BookingRooms = [new BookingRoom() { HotelId = hotel.Id, RoomNumber = room.RoomNumber }],
                    PaymentIntentId = _fixture.Create<string>(),
                    UserId = "UserId",
                    DiscountedTotalPrice = _fixture.Create<int>(),
                    OriginalTotalPrice = _fixture.Create<int>(),
                };
                userBookings.Add(booking);
            }
            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.Rooms.AddAsync(room);
            await _dbContext.Bookings.AddRangeAsync(userBookings);
            await _dbContext.SaveChangesAsync();
            var roombookings = await _dbContext.BookingRooms.ToListAsync();
            var bookings = await _dbContext.Bookings.Include(o => o.BookingRooms).ToListAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            //Act
            var response = await _client.GetAsync($"api/users/{"UserId"}/bookings?page=2&pageSize=5");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ResultWithPaginationResponse<IEnumerable<BookingDto>>>();
            result.Should().NotBeNull();
            result?.Results.Should().HaveCount(5);
            result?.TotalRecords.Should().Be(10);
            result?.Page.Should().Be(2);
            result?.PageSize.Should().Be(5);
            result?.TotalPages.Should().Be(2);
        }
    }
}
