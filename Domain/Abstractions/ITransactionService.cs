namespace Domain.Abstractions
{
    public interface ITransactionService
    {
        public Task BeginTransactionAsync();
        public Task CommitTransactionAsync();
        public Task RollbackTransactionAsync();
    }
}
