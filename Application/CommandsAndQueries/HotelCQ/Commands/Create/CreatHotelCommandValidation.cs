using Application.Validation;
using Domain.Abstractions;
using Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.CommandsAndQueries.HotelCQ.Commands.Create
{
    public class CreatHotelCommandValidation : AbstractValidator<CreateHotelCommand>
    {
        public CreatHotelCommandValidation(
            IHotelRepository hotelRepository,
            ICityRepository cityRepository,
            IImageService imageRepository)
        {
            RuleFor(hotel => hotel.Name).NotEmpty().MaximumLength(50);
            RuleFor(hotel => hotel.Owner).NotEmpty().MaximumLength(50);
            RuleFor(hotel => hotel.Address).NotEmpty().MaximumLength(100);
            RuleFor(hotel => hotel.Description).NotEmpty().MaximumLength(160);
            RuleFor(hotel => hotel.Thumbnail).Cascade(CascadeMode.Stop)
              .NotEmpty()
              .Custom(
                (image, context) =>
                    imageRepository.ValidateImage(image, context, "Thumbnail")
              );
            RuleFor(hotel => hotel.Images).Cascade(CascadeMode.Stop)
              .NotEmpty()
              .WithMessage("Please upload at least one image.")
              .Must(images => images.Count <= 20)
              .WithMessage("You can upload up to 20 images.")
              .Custom((images, context) =>
              images.ForEach(
                  (image) => 
                    imageRepository.ValidateImage<CreateHotelCommand>(image,context,"Images")
                )
              );

            RuleFor(hotel => hotel.CityId).NotEmpty()
              .MustAsync(
                async (cityId, token) => {
                    var city = await cityRepository.GetByIdAsync(cityId);
                    return city is not null;
                }
              ).WithMessage("City Not Found");
            RuleFor(hotel => hotel.PricePerNight).NotEmpty().GreaterThanOrEqualTo(0);
            RuleFor(hotel => hotel.HotelType).Must(
                ht =>
                Enum.IsDefined(
                  typeof(HotelType),
                  ht
                )
              )
              .WithMessage("The hotel type is unknown.");
        }
    }
}