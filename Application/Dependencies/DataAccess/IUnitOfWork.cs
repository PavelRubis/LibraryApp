namespace Application.Dependencies.DataAccess;

public interface IUnitOfWork : IAsyncDisposable
{
    IAuthorRepository Authors { get; }
    IBookRepository Books { get; }
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

