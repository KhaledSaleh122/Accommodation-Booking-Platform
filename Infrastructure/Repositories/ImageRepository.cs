using Domain.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Repositories
{
    public class ImageRepository : IImageService
    {
        public void DeleteFile(string filePath,bool isFileExist = false)
        {
            var directoryPath = $"{Directory.GetCurrentDirectory()}\\Public\\";
            var fullPath = Path.Combine(directoryPath, filePath);
            if (isFileExist && !File.Exists(fullPath)) return;
            File.Delete(fullPath);
        }

        public string UploadFile(IFormFile formFile, string fileStorePath, string fileName)
        {
            var extention = Path.GetExtension(formFile.FileName);
            var directoryPath = $"{Directory.GetCurrentDirectory()}\\Public\\";
            var storePath = Path.Combine(directoryPath, fileStorePath ?? "");
            if (!Directory.Exists(storePath))
            {
                Directory.CreateDirectory(storePath);
            }
            var fullPath = Path.Combine(storePath, fileName + extention);
            var stream = File.OpenWrite(fullPath);
            formFile.CopyTo(stream);
            stream.Close();
            return fullPath;
        }

        public bool ValidateFile(IFormFile formFile)
        {
            var extention = Path.GetExtension(formFile.FileName);
            if (extention is null) throw new Exception("Couldn't find file extentsion");
            var allowedImageExtentsion = new List<string>() { ".jpg", ".jpeg", ".png" };
            var isAllowed = allowedImageExtentsion.Contains(extention);
            if (!isAllowed) throw new Exception("Unsupported file extension.");
            return true;
        }
    }
}
