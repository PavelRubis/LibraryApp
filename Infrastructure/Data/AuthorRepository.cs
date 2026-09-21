using System.Data;
using Application.Common;
using Application.Dependencies.DataAccess;
using Dapper;
using Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

internal sealed class AuthorRepository(
    DapperSession session,
    ILogger<AuthorRepository> logger) : IAuthorRepository
{
    public Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) 
        => RunAsync(() => GetByIdCoreAsync(id, cancellationToken), "get author", id);

    public Task<Author> AddAsync(Author entity, CancellationToken cancellationToken = default) 
        => RunAsync(() => AddCoreAsync(entity, cancellationToken), "insert author", entity.Id);

    public Task<Author> UpdateAsync(Author entity, byte[] expectedRowVersion, CancellationToken cancellationToken = default)
        => RunAsync(() => UpdateCoreAsync(entity, expectedRowVersion, cancellationToken), "update author", entity.Id);

    public Task RemoveAsync(Guid id, byte[] expectedRowVersion, CancellationToken cancellationToken = default) 
        => RunAsync(() => RemoveCoreAsync(id, expectedRowVersion, cancellationToken), "delete author", id);

    public Task<PagedResult<Author>> SearchAsync(string? query, int page, int pageSize, CancellationToken cancellationToken = default)
        => RunAsync(() => SearchCoreAsync(query, page, pageSize, cancellationToken), "search authors");

    private async Task<Author?> GetByIdCoreAsync(Guid id, CancellationToken cancellationToken)
    {
        var connection = await session.GetOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            "library.Author_GetById",
            new { Id = id },
            session.Transaction,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<AuthorRow>(command);
        return row is null ? null : Map(row);
    }

    private async Task<Author> AddCoreAsync(Author entity, CancellationToken cancellationToken)
    {
        var status = await ExecuteMutationAsync("library.Author_Insert", new { entity.Id, entity.Name }, cancellationToken);
        status.EnsureSucceeded("Author");
        return await GetByIdAsync(entity.Id, cancellationToken) ?? throw new InvalidOperationException("Inserted author could not be read.");
    }

    private async Task<Author> UpdateCoreAsync(Author entity, byte[] expectedRowVersion, CancellationToken cancellationToken)
    {
        var status = await ExecuteMutationAsync("library.Author_Update", new { entity.Id, entity.Name, ExpectedRowVersion = expectedRowVersion }, cancellationToken);
        status.EnsureSucceeded("Author");
        return await GetByIdAsync(entity.Id, cancellationToken) ?? throw new InvalidOperationException("Updated author could not be read.");
    }

    private async Task RemoveCoreAsync(Guid id, byte[] expectedRowVersion, CancellationToken cancellationToken)
    {
        var status = await ExecuteMutationAsync("library.Author_SoftDelete", new { Id = id, ExpectedRowVersion = expectedRowVersion }, cancellationToken);
        status.EnsureRemoved("Author");
    }

    private async Task<PagedResult<Author>> SearchCoreAsync(string? query, int page, int pageSize, CancellationToken cancellationToken)
    {
        var connection = await session.GetOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            "library.Author_Search",
            new { Query = query, Page = page, PageSize = pageSize },
            session.Transaction,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);
        using var results = await connection.QueryMultipleAsync(command);
        var rows = (await results.ReadAsync<AuthorRow>()).AsList();
        var totalCount = await results.ReadSingleAsync<int>();
        return new PagedResult<Author>(rows.Select(Map).ToArray(), page, pageSize, totalCount);
    }

    private async Task<RepositoryMutationStatus> ExecuteMutationAsync(string procedure, object parameters, CancellationToken cancellationToken)
    {
        var connection = await session.GetOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(procedure, parameters, session.Transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
        return (RepositoryMutationStatus)await connection.QuerySingleAsync<int>(command);
    }

    private async Task<T> RunAsync<T>(Func<Task<T>> action, string operation, Guid? id = null)
    {
        try
        {
            return await action();
        }
        catch (SqlException exception) when (IsUniqueConstraintViolation(exception))
        {
            logger.LogError(exception, "Failed to {DatabaseOperation} for author {AuthorId}", operation, id);
            throw new ConflictException("An active author with the same name already exists.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to {DatabaseOperation} for author {AuthorId}", operation, id);
            throw;
        }
    }

    private async Task RunAsync(Func<Task> action, string operation, Guid? id = null)
    {
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to {DatabaseOperation} for author {AuthorId}", operation, id);
            throw;
        }
    }

    private static bool IsUniqueConstraintViolation(SqlException exception) => exception.Errors.Cast<SqlError>().Any(error => error.Number is 2601 or 2627);

    private static Author Map(AuthorRow row) => new()
    {
        Id = row.Id,
        Name = row.Name,
        CreatedAt = row.CreatedAt,
        UpdatedAt = row.UpdatedAt,
        RowVersion = row.RowVersion
    };
}
