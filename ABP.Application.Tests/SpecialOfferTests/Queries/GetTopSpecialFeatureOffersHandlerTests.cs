using Application.CommandsAndQueries.SpecialOfferCQ.Commands.Create;
using Application.CommandsAndQueries.SpecialOfferCQ.Queries.GetTopFeatureDealOffers;
using Application.Dtos.SpecialOfferDtos;
using AutoFixture;
using Domain.Abstractions;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Moq;

namespace ABP.Application.Tests.SpecialOfferTests.Queries
{
    public class GetTopSpecialFeatureOffersHandlerTests
    {
        private readonly Mock<ISpecialOfferRepository> _specialOfferRepositoryMock;
        private readonly GetTopSpecialFeatureOffersHandler _handler;
        private readonly IFixture _fixture;
        private readonly GetTopSpecialFeatureOffersQuery _query;
        public GetTopSpecialFeatureOffersHandlerTests()
        {
            _specialOfferRepositoryMock = new();
            _fixture = new Fixture();
            _query = new GetTopSpecialFeatureOffersQuery();
            _handler = new GetTopSpecialFeatureOffersHandler(
                _specialOfferRepositoryMock.Object);
        }

        [Fact]

        public async Task Handler_Should_ReturnTopSpecialFeatureOffers_WhenSucess() {
            //Arrange
            _specialOfferRepositoryMock.Setup(x => x.GetTopSpecialFeatureOffers()).ReturnsAsync([]);
            //Act
            IEnumerable<FeaturedDealsDto> result = await _handler.Handle(_query, default);
            //Assert
            _specialOfferRepositoryMock.Verify(x => x.GetTopSpecialFeatureOffers(), Times.Once());
            result.Should().NotBeNull();
        }
    }
}
