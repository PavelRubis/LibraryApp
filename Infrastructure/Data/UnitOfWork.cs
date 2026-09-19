using Application.Dependencies.DataAccess;

namespace Infrastructure.Data;

internal sealed class UnitOfWork(
    DapperSession session,
    IAuthorRepository authors,
    IBookRepository books) : IUnitOfWork
{
    public IAuthorRepository Authors { get; } = authors;
    public IBookRepository Books { get; } = books;

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        session.BeginTransactionAsync(cancellationToken);

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default) =>
        session.CommitTransactionAsync(cancellationToken);

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) =>
        session.RollbackTransactionAsync(cancellationToken);

    public ValueTask DisposeAsync() => session.DisposeAsync();
}

