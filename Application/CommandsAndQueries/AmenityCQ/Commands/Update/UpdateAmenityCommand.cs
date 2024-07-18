using MediatR;

namespace Application.CommandsAndQueries.AmenityCQ.Commands.Update
{
    public class UpdateAmenityCommand(string name, string description) : IRequest
    {
        public int id;
        public string Name { get; set; } = name;
        public string Description { get; set; } = description;
    }
}
