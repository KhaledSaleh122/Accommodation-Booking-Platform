namespace Presentation.Responses.Pagination
{
    public class ResultWithPaginationResponse<TData>
    {
        public TData Results { get; set; }
        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalRecords { get; set; }
        public int TotalPages { get => (int)Math.Ceiling((decimal)TotalRecords / PageSize); }
    }
}
