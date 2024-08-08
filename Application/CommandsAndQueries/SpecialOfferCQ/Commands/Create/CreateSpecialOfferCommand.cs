using Application.Dtos.SpecialOfferDtos;
using Domain.Enums;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.CommandsAndQueries.SpecialOfferCQ.Commands.Create
{
    public class CreateSpecialOfferCommand : IRequest<SpecialOfferDto>
    {
        public int hotelId;
        public string? Id { get; set; }
        [Required]
        public int DiscountPercentage { get; set; }
        [Required]
        public OfferType OfferType { get; set; }
        [Required]
        public DateOnly ExpireDate { get; set; }
    }
}
