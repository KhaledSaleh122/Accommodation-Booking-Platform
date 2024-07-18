using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.CommandsAndQueries.AmenityCQ.Commands.Update
{
    public class UpdateAmenityCommand(string name, string description) : IRequest
    {
        [Required]
        public int id;
        [Required]
        public string Name { get; set; } = name;
        [Required]
        public string Description { get; set; } = description;
    }
}
