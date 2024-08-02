using Application.CommandsAndQueries.CityCQ.Commands.Create;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text;

namespace ABP.Presentation.IntegrationTests
{
    public class GlobalTestData
    {
        public static MultipartFormDataContent GetMultiPartFormDataFromCommand<T>(T _command) where T : class
        {
            var content = new MultipartFormDataContent();
            foreach (var property in _command.GetType().GetProperties())
            {
                var value = property.GetValue(_command);
                if (value is IFormFile formFile)
                {
                    var fileContent = new StreamContent(formFile.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(formFile.ContentType);
                    content.Add(fileContent, property.Name, formFile.FileName);
                }
                else if (value is not null)
                {
                    content.Add(new StringContent(value.ToString()!), property.Name);
                }
            }
            return content;
        }

        public static IFormFile GetFormFile() {
            IFormFile testFile = new FormFile(
                    baseStream: new MemoryStream(Encoding.UTF8.GetBytes("Dummy image content")),
                    baseStreamOffset: 0,
                    length: 20,
                    name: "dummyFile",
                    fileName: "dummy.png")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
            return testFile;
        }

        public static IFormFile GetInvaildFormFile()
        {
            IFormFile testFile = new FormFile(
            baseStream: new MemoryStream(Encoding.UTF8.GetBytes("Dummy image content")),
            baseStreamOffset: 0,
            length: 20,
            name: "dummyFile",
            fileName: "dummy.gif")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/gif"
                };
            return testFile;
        }
    }
}
