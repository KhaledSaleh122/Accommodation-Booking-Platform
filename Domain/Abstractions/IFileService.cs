
using Microsoft.AspNetCore.Http;

namespace Domain.Abstractions
{
    public interface IFileService
    {
        public string UploadFile(IFormFile formFile, string fileStorePath,string fileName);

        public bool ValidateFile(IFormFile formFile);
        public void DeleteFile(string filePath, bool isFileExist = false);
    }
}
