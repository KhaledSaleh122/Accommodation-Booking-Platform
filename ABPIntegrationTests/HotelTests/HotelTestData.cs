using Application.CommandsAndQueries.HotelCQ.Commands.Create;
using Microsoft.AspNetCore.Http;

namespace ABPIntegrationTests.HotelTests
{
    public static class HotelTestData
    {
        public static IEnumerable<object?[]> CreateHotelTestData =>
            new List<object?[]>
            {
                new object?[] { null },
                new object[] { new CreateHotelCommand() {  
                        Thumbnail = new FormFile(new MemoryStream(),0,0, "Thumbnail", "File.exe") 
                    } 
                },
                new object[] { new CreateHotelCommand { Name = "Hotel" } },
                new object[] { new CreateHotelCommand { Description = "Description" } },
                new object[] { new CreateHotelCommand { Name = "Hotel", Description = "Description" } },
                new object[] { new CreateHotelCommand { Name = "Hotel", Description = "Description", Owner = "Owner" } },
                new object[] { new CreateHotelCommand { Name = "Hotel", Description = "Description", Address = "Address" } },
                new object[] { new CreateHotelCommand { Name = "Hotel", Description = "Description", CityId = 2 } },
                new object[] { new CreateHotelCommand { Name = "Hotel", Description = "Description", PricePerNight = 100 } },
                new object[] { new CreateHotelCommand { Name = "Hotel", Description = "Description", HotelType = 1 } },
                new object[] { new CreateHotelCommand { Name = "Hotel", Description = "Description" } },
                new object[] { new CreateHotelCommand { Name = "Hotel", Description = "Description" } },
                new object[] { new CreateHotelCommand { 
                        Name = "Hotel",
                        Description = "Description",
                        Owner = "Owner",
                        Address = "Address",
                        CityId = 1,
                        PricePerNight = 100,
                        HotelType = 1 ,
                        Thumbnail = new FormFile(new MemoryStream(),0,0, "Thumbnail", "File.png"),
                        Images = new List<IFormFile>(){ new FormFile(new MemoryStream(), 0, 0, "Images", "File.exe") }
                    } 
                },
            };
        public static IEnumerable<object[]> NotFoundTestData =>
            new List<object[]>
            {
                new object[] { 99999 },
                new object[] { -1 },
                new object[] { int.MaxValue },
                new object[] { "test" },
            };
    }
}
