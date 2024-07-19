using Application.CommandsAndQueries.CityCQ.Commands.Create;
using Application.CommandsAndQueries.CityCQ.Commands.Update;

public static class CityTestData
{
    public static IEnumerable<object?[]> CreateCityTestData =>
        new List<object?[]>
        {
            new object?[] { null },
            new object[] { new CreateCityCommand { Name = "", Country = "",PostOffice = "" } },
            new object[] { new CreateCityCommand { } },
            new object[] { new CreateCityCommand
                { 
                    Name =  new string('a',51),
                    Country = new string('a', 51),
                    PostOffice = new string ('a',21)
                } 
            },
            new object[] { new CreateCityCommand { 
                Name = "Test City",
                Country = "Test Country",
                PostOffice = "500" } 
            },
            new object[] { new CreateCityCommand {
                Name = "Test",
                Country = "Test",
                PostOffice = "100" }
            },
        };

    public static IEnumerable<object[]> NotFoundTestData =>
        new List<object[]>
        {
            new object[] { "99999" },
            new object[] { "-1" },
            // Add more test cases as needed
        };

    public static IEnumerable<object[]> UpdateCityTestData =>
        new List<object[]>
        {
            new object[] { new UpdateCityCommand { Name = "", Country = "",PostOffice = "" } },
            new object[] { new UpdateCityCommand { } },
            new object[] { new UpdateCityCommand
                {
                    Name =  new string('a',51),
                    Country = new string('a', 51),
                    PostOffice = new string ('a',21)
                }
            },
            new object[] { new UpdateCityCommand {
                Name = "Test City",
                Country = "Test Country",
                PostOffice = "500" }
            },
            new object[] { new UpdateCityCommand {
                Name = "Test",
                Country = "Test",
                PostOffice = "100" }
            },
        };
}
