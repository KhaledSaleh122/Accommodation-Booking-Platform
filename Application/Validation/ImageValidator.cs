

using Application.CommandsAndQueries.HotelCQ.Commands.Create;
using Domain.Abstractions;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Validation
{
    public static class ImageValidator
    {
        public static void ValidateImage<TRequest>(
                this IImageService imageRepository,
                IFormFile image,
                ValidationContext<TRequest> context,
                string propertyName
            )
        {
            try
            {
                imageRepository.ValidateFile(image);
            }
            catch (Exception ex)
            {
                context.AddFailure(propertyName, ex.Message);
            }
        }
    }
}
