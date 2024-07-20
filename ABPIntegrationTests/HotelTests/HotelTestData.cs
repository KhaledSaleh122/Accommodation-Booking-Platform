using Application.CommandsAndQueries.CityCQ.Commands.Update;
using Application.CommandsAndQueries.HotelCQ.Commands.Create;
using Microsoft.AspNetCore.Http;

namespace ABPIntegrationTests.HotelTests
{
    public static class HotelTestData
    {
        public static IEnumerable<object?[]> CreateHotelTestData =>
            [
                [
                    new CreateHotelCommand
                    {
                        Name = "Valid Name",
                        Description = "Valid Description",
                        Images = Enumerable.Repeat(new FormFile(new MemoryStream(), 0, 0, "Images", "image.jpg"), 21).ToList<IFormFile>(), // More than 20 images
                        Thumbnail = new FormFile(new MemoryStream(), 0, 0, "Thumbnail", "thumbnail.jpg"),
                        Owner = "Valid Owner",
                        Address = "Valid Address",
                        HotelType = 1,
                        CityId = 1,
                        PricePerNight = 100
                    }
                ],
                [null],
                // Invalid Name - exceeds maximum length of 50 or is empty
                [
                    new CreateHotelCommand
                    {
                        Name = "",
                        Description = "Valid Description",
                        Images = new List<IFormFile> { new FormFile(new MemoryStream(), 0, 0, "Images", "image.jpg") },
                        Thumbnail = new FormFile(new MemoryStream(), 0, 0, "Thumbnail", "thumbnail.jpg"),
                        Owner = "Valid Owner",
                        Address = "Valid Address",
                        HotelType = 1,
                        CityId = 1,
                        PricePerNight = 100
                    }
                ],
                [
                    new CreateHotelCommand
                    {
                        Name = new string('A', 51),
                        Description = "Valid Description",
                        Images = new List<IFormFile> { new FormFile(new MemoryStream(), 0, 0, "Images", "image.jpg") },
                        Thumbnail = new FormFile(new MemoryStream(), 0, 0, "Thumbnail", "thumbnail.jpg"),
                        Owner = "Valid Owner",
                        Address = "Valid Address",
                        HotelType = 1,
                        CityId = 1,
                        PricePerNight = 100
                    }
                ],
                // Invalid Owner - exceeds maximum length of 50 or is empty
                [
                    new CreateHotelCommand
                    {
                        Name = "Valid Name",
                        Description = "Valid Description",
                        Images = new List<IFormFile> { new FormFile(new MemoryStream(), 0, 0, "Images", "image.jpg") },
                        Thumbnail = new FormFile(new MemoryStream(), 0, 0, "Thumbnail", "thumbnail.jpg"),
                        Owner = "",
                        Address = "Valid Address",
                        HotelType = 1,
                        CityId = 1,
                        PricePerNight = 100
                    }
                ],
                [
                    new CreateHotelCommand
                    {
                        Name = "Valid Name",
                        Description = "Valid Description",
                        Images = new List<IFormFile> { new FormFile(new MemoryStream(), 0, 0, "Images", "image.jpg") },
                        Thumbnail = new FormFile(new MemoryStream(), 0, 0, "Thumbnail", "thumbnail.jpg"),
                        Owner = new string('B', 51),
                        Address = "Valid Address",
                        HotelType = 1,
                        CityId = 1,
                        PricePerNight = 100
                    }
                ],
                // Invalid Address - exceeds maximum length of 100 or is empty
                [
                    new CreateHotelCommand
                    {
                        Name = "Valid Name",
                        Description = "Valid Description",
                        Images = new List<IFormFile> { new FormFile(new MemoryStream(), 0, 0, "Images", "image.jpg") },
                        Thumbnail = new FormFile(new MemoryStream(), 0, 0, "Thumbnail", "thumbnail.jpg"),
                        Owner = "Valid Owner",
                        Address = "",
                        HotelType = 1,
                        CityId = 1,
                        PricePerNight = 100
                    }
                ],
                [
                    new CreateHotelCommand
                    {
                        Name = "Valid Name",
                        Description = "Valid Description",
                        Images = new List<IFormFile> { new FormFile(new MemoryStream(), 0, 0, "Images", "image.jpg") },
                        Thumbnail = new FormFile(new MemoryStream(), 0, 0, "Thumbnail", "thumbnail.jpg"),
                        Owner = "Valid Owner",
                        Address = new string('C', 101),
                        HotelType = 1,
                        CityId = 1,
                        PricePerNight = 100
                    }
                ],
                // Invalid Description - exceeds maximum length of 160 or is empty
                [
                    new CreateHotelCommand
                    {
                        Name = "Valid Name",
                        Description = "",
                        Images = new List<IFormFile> { new FormFile(new MemoryStream(), 0, 0, "Images", "image.jpg") },
                        Thumbnail = new FormFile(new MemoryStream(), 0, 0, "Thumbnail", "thumbnail.jpg"),
                        Owner = "Valid Owner",
                        Address = "Valid Address",
                        HotelType = 1,
                        CityId = 1,
                        PricePerNight = 100
                    }
                ],
                [
                    new CreateHotelCommand
                    {
                        Name = "Valid Name",
                        Description = new string('D', 161),
                        Images = new List<IFormFile> { new FormFile(new MemoryStream(), 0, 0, "Images", "image.jpg") },
                        Thumbnail = new FormFile(new MemoryStream(), 0, 0, "Thumbnail", "thumbnail.jpg"),
                        Owner = "Valid Owner",
                        Address = "Valid Address",
                        HotelType = 1,
                        CityId = 1,
                        PricePerNight = 100
                    }
                ],
                // Invalid Images - empty or more than 20
                [
                    new CreateHotelCommand
                    {
                        Name = "Valid Name",
                        Description = "Valid Description",
                        Images = new List<IFormFile>(), // No images
                        Thumbnail = new FormFile(new MemoryStream(), 0, 0, null, "thumbnail.jpg"),
                        Owner = "Valid Owner",
                        Address = "Valid Address",
                        HotelType = 1,
                        CityId = 1,
                        PricePerNight = 100
                    }
                ],
                // Invalid Thumbnail - empty
                [
                    new CreateHotelCommand
                    {
                        Name = "Valid Name",
                        Description = "Valid Description",
                        Images = new List<IFormFile> { new FormFile(new MemoryStream(), 0, 0, "Images", "image.jpg") },
                        Owner = "Valid Owner",
                        Address = "Valid Address",
                        HotelType = 1,
                        CityId = 1,
                        PricePerNight = 100
                    }
                ],
                // Invalid CityId - city not found
                [
                    new CreateHotelCommand
                    {
                        Name = "Valid Name",
                        Description = "Valid Description",
                        Images = new List<IFormFile> { new FormFile(new MemoryStream(), 0, 0, "Images", "image.jpg") },
                        Thumbnail = new FormFile(new MemoryStream(), 0, 0, "Thumbnail", "thumbnail.jpg"),
                        Owner = "Valid Owner",
                        Address = "Valid Address",
                        HotelType = 1,
                        CityId = 9999, // Assuming 9999 is not a valid CityId
                        PricePerNight = 100
                    }
                ],
                // Invalid PricePerNight - less than 0
                [
                    new CreateHotelCommand
                    {
                        Name = "Valid Name",
                        Description = "Valid Description",
                        Images = new List<IFormFile> { new FormFile(new MemoryStream(), 0, 0, "Images", "image.jpg") },
                        Thumbnail = new FormFile(new MemoryStream(), 0, 0, "Thumbnail", "thumbnail.jpg"),
                        Owner = "Valid Owner",
                        Address = "Valid Address",
                        HotelType = 1,
                        CityId = 1,
                        PricePerNight = -1 // Negative price
                    }
                ],
                // Invalid HotelType - not defined in HotelType enum
                [
                    new CreateHotelCommand
                    {
                        Name = "Valid Name",
                        Description = "Valid Description",
                        Images = new List<IFormFile> { new FormFile(new MemoryStream(), 0, 0, "Images", "image.jpg") },
                        Thumbnail = new FormFile(new MemoryStream(), 0, 0, "Thumbnail", "thumbnail.jpg"),
                        Owner = "Valid Owner",
                        Address = "Valid Address",
                        HotelType = -2,
                        CityId = 1,
                        PricePerNight = 100
                    }
                ],
                // Invalid Image extention 
                [
                    new CreateHotelCommand
                    {
                        Name = "Valid Name",
                        Description = "Valid Description",
                        Images = new List<IFormFile> { new FormFile(new MemoryStream(), 0, 0, "Images", "image.exe") },
                        Thumbnail = new FormFile(new MemoryStream(), 0, 0, "Thumbnail", "thumbnail.jpg"),
                        Owner = "Valid Owner",
                        Address = "Valid Address",
                        HotelType = -2,
                        CityId = 1,
                        PricePerNight = 100
                    }
                ],
                // Invalid Thumbnail extention 
                [
                    new CreateHotelCommand
                    {
                        Name = "Valid Name",
                        Description = "Valid Description",
                        Images = new List<IFormFile> { new FormFile(new MemoryStream(), 0, 0, "Images", "image.png") },
                        Thumbnail = new FormFile(new MemoryStream(), 0, 0, "Thumbnail", "thumbnail.exe"),
                        Owner = "Valid Owner",
                        Address = "Valid Address",
                        HotelType = -2,
                        CityId = 1,
                        PricePerNight = 100
                    }
                ]
            ];

        public static IEnumerable<object[]> UpdateHotelInvalidTestData =>
            [
                // Invalid Name - exceeds maximum length of 50
                [
                    new UpdateHotelCommand
                    {
                        Name = new string('A', 51),
                        Description = "Valid Description",
                        Owner = "Valid Owner",
                        Address = "Valid Address",
                        PricePerNight = 100,
                        HotelType = 1
                    }
                ],
                // Invalid Owner - exceeds maximum length of 50
                [
                    new UpdateHotelCommand
                    {
                        Name = "Valid Name",
                        Description = "Valid Description",
                        Owner = new string('B', 51),
                        Address = "Valid Address",
                        PricePerNight = 100,
                        HotelType = 1
                    }
                ],
                // Invalid Address - exceeds maximum length of 100
                [
                    new UpdateHotelCommand
                    {
                        Name = "Valid Name",
                        Description = "Valid Description",
                        Owner = "Valid Owner",
                        Address = new string('C', 101),
                        PricePerNight = 100,
                        HotelType = 1
                    }
                ],
                // Invalid Description - exceeds maximum length of 160
                [
                    new UpdateHotelCommand
                    {
                        Name = "Valid Name",
                        Description = new string('D', 161),
                        Owner = "Valid Owner",
                        Address = "Valid Address",
                        PricePerNight = 100,
                        HotelType = 1
                    }
                ],
                // Invalid PricePerNight - less than 0
                [
                    new UpdateHotelCommand
                    {
                        Name = "Valid Name",
                        Description = "Valid Description",
                        Owner = "Valid Owner",
                        Address = "Valid Address",
                        PricePerNight = -1,
                        HotelType = 1
                    }
                ],
                // Invalid HotelType - not defined in HotelType enum
                [
                    new UpdateHotelCommand
                    {
                        Name = "Valid Name",
                        Description = "Valid Description",
                        Owner = "Valid Owner",
                        Address = "Valid Address",
                        PricePerNight = 100,
                        HotelType = -1
                    }
                ]
            ];

        public static IEnumerable<object[]> PatchUpdateTestData =>
            [
                [
                    new UpdateHotelCommand
                    {
                        Name = "Update Name"
                    }
                ],
                [
                    new UpdateHotelCommand
                    {
                        Description = "Update Description"
                    }
                ],
                [
                    new UpdateHotelCommand
                    {
                        Owner = "Update Owner"
                    }
                ],
                [
                    new UpdateHotelCommand
                    {
                        Address = "Update Address"
                    }
                ],
                [
                    new UpdateHotelCommand
                    {
                        PricePerNight = 32200
                    }
                ],
                [
                    new UpdateHotelCommand
                    {
                        HotelType = 3
                    }
                ]
            ];
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
