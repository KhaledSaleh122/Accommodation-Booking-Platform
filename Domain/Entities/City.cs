#nullable disable

namespace Domain.Entities
{
    public sealed class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }

        public string PostOffice { get; set; }

    }
}
