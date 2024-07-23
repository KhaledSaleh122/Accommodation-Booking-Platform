using Domain.Abstractions;

namespace Infrastructure.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _dbContext;

        public TransactionService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task BeginTransactionAsync()
        {
            await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_dbContext.Database.CurrentTransaction is null) return;
            await _dbContext.Database.CurrentTransaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            if (_dbContext.Database.CurrentTransaction is null) return;
            await _dbContext.Database.CurrentTransaction.RollbackAsync();
        }

    }
}
