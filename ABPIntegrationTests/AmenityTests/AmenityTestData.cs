using Application.CommandsAndQueries.AmenityCQ.Commands.Create;
using Application.CommandsAndQueries.AmenityCQ.Commands.Update;

namespace ABPIntegrationTests.AmenityTests
{
    public static class AmenityTestData
    {
        public static IEnumerable<object?[]> GetCreateAmenityCommandData()
        {
            yield return new object?[] { null };
            yield return new object[] { new CreateAmenityCommand { Name = "test" } };
            yield return new object[] { new CreateAmenityCommand { Description = "test" } };
            yield return new object[] { new CreateAmenityCommand { Description = "test" } };
        }

        public static IEnumerable<object?[]> GetUpdateAmenityCommandTestData()
        {
            yield return new object[] { new UpdateAmenityCommand { Name = "test" } };
            yield return new object[] { new UpdateAmenityCommand { Description = "test" } };
            yield return new object[] { new UpdateAmenityCommand { Description = "test" } };
        }

        public static IEnumerable<object[]> GetNotFoundTestData()
        {
            yield return new object[] { "wrongId" };
            yield return new object[] { "-2" };
            yield return new object[] { "0" };
            yield return new object[] { "999" };
        }
    }
}
