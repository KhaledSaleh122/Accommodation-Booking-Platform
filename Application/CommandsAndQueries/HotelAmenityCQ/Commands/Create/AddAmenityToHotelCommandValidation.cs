using Domain.Abstractions;
using FluentValidation;

namespace Application.CommandsAndQueries.HotelAmenityCQ.Commands.Create
{
    public class AddAmenityToHotelCommandValidation : AbstractValidator<AddAmenityToHotelCommand>
    {
        public AddAmenityToHotelCommandValidation()
        {
        }
    }
}
