using System.ComponentModel.DataAnnotations;

namespace Presentation.Responses.Pagination
{
    public class ResultWithPaginationResponse<TData>
    {
        [Required]
        public TData Results { get; set; }
        [Required]
        public int Page { get; set; }
        [Required]
        public int PageSize { get; set; }
        [Required]
        public int TotalRecords { get; set; }
        public int TotalPages { get => (int)Math.Ceiling((decimal)TotalRecords / PageSize); }
    }
}
