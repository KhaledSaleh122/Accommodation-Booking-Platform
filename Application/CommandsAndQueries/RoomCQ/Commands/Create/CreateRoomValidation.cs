using Application.Validation;
using Domain.Abstractions;
using FluentValidation;

namespace Application.CommandsAndQueries.RoomCQ.Commands.Create
{
    public class CreateRoomValidation : AbstractValidator<CreateRoomCommand>
    {
        public CreateRoomValidation(IHotelRepository hotelRepository, IImageService imageRepository)
        {
            RuleFor(room => room.RoomNumber)
                .NotEmpty()
                .MaximumLength(10);
            RuleFor(room => room.AdultCapacity)
                .GreaterThanOrEqualTo(0);
            RuleFor(room => room.ChildrenCapacity)
                .GreaterThanOrEqualTo(0);
            RuleFor(room => room.Thumbnail).Cascade(CascadeMode.Stop)
              .NotEmpty()
              .Custom((image, context) =>
                imageRepository.ValidateImage(image, context, "Thumbnail")
              );
            RuleFor(room => room.Images).Cascade(CascadeMode.Stop)
              .NotEmpty()
              .WithMessage("Please upload at least one image.")
              .Must(images => images.Count <= 20)
              .WithMessage("You can upload up to 20 images.")
              .Custom((images, context) =>
                images.ForEach(
                  (image) => imageRepository.ValidateImage(image, context, "Images")
                )
              );
            RuleFor(m => m).CustomAsync(
                async (command, context, token) =>
                {
                    if (command.RoomNumber is not null)
                    {
                        var isExist = await hotelRepository.RoomNumberExistsAsync(
                                command.hotelId,
                                command.RoomNumber
                            );
                        if (isExist)
                        {
                            context.AddFailure("RoomNumber", "The RoomNumber already Exist");
                        }
                    }
                }
            );
        }
    }
}
