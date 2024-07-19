using Application.CommandsAndQueries.AmenityCQ.Commands.Create;
using Application.CommandsAndQueries.AmenityCQ.Commands.Update;

namespace ABPIntegrationTests.AmenityTests
{
    public static class AmenityTestData
    {
        public static IEnumerable<object?[]> CreateAmenityTestData()
        {
            yield return new object?[] { null };
            yield return new object[] { new CreateAmenityCommand { Name = "test" } };
            yield return new object[] { new CreateAmenityCommand { Description = "test" } };
            yield return new object[] { new CreateAmenityCommand { Description = "", Name = "test" } };
            yield return new object[] { new CreateAmenityCommand { Description = "test", Name = "" } };
            yield return new object[] { new CreateAmenityCommand 
                { 
                    Description = new string('a',161),
                    Name = new string('a',61) 
                } 
            };
        }

        public static IEnumerable<object?[]> UpdateAmenityTestData()
        {
            yield return new object[] { new UpdateAmenityCommand { Name = "test" } };
            yield return new object[] { new UpdateAmenityCommand { Description = "test" } };
            yield return new object[] { new UpdateAmenityCommand { Description = "", Name = "test" } };
            yield return new object[] { new UpdateAmenityCommand { Description = "test", Name = "" } };
            yield return new object[] { new UpdateAmenityCommand 
                { 
                    Description = new string('a', 161), 
                    Name = new string('a', 61) 
                } 
            };
        }

        public static IEnumerable<object[]> NotFoundTestData()
        {
            yield return new object[] { "wrongId" };
            yield return new object[] { "-2" };
            yield return new object[] { "0" };
            yield return new object[] { "999" };
        }
    }
}
