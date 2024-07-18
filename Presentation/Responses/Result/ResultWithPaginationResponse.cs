using System.ComponentModel.DataAnnotations;

namespace Presentation.Responses.Pagination
{
    public class ResultWithPaginationResponse<TData>
    {
        [Required]
        public TData Results { get; set; }
        [Required]
        public uint Page { get; set; }
        [Required]
        public uint PageSize { get; set; }
        [Required]
        public uint TotalRecords { get; set; }
        public uint TotalPages { get => (uint)Math.Ceiling((decimal)TotalRecords / PageSize); }
    }
}
