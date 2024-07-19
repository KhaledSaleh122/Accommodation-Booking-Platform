namespace Application
{
    public class PaginationParameters(int page, int pageSize)
    {
        public int Page { get; set; } = page > 0 ? page : 1;
        public int pageSize { get; set; } = pageSize > 0 && pageSize <= 100 ? pageSize : 10;
    }
}
