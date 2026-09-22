using System.Data;
using Application.Common;
using Application.Dependencies.DataAccess;
using Dapper;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

internal sealed class BookRepository(
    DapperSession session,
    ILogger<BookRepository> logger) : IBookRepository
{
    public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) 
        => RunAsync(() => GetByIdCoreAsync(id, cancellationToken), "get book", id);

    public Task<Book> AddAsync(Book entity, CancellationToken cancellationToken = default) 
        => RunAsync(() => AddCoreAsync(entity, cancellationToken), "insert book", entity.Id);

    public Task<Book> UpdateAsync(Book entity, byte[] expectedRowVersion, CancellationToken cancellationToken = default) 
        => RunAsync(() => UpdateCoreAsync(entity, expectedRowVersion, cancellationToken), "update book", entity.Id);

    public Task RemoveAsync(Guid id, byte[] expectedRowVersion, CancellationToken cancellationToken = default) 
        => RunAsync(() => RemoveCoreAsync(id, expectedRowVersion, cancellationToken), "delete book", id);

    public Task<PagedResult<Book>> SearchBookBySearchCriterias(
        BookSearchCriteria searchCriteria,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
        => RunAsync(() => SearchCoreAsync(searchCriteria, page, pageSize, cancellationToken), "search books");

    private async Task<Book?> GetByIdCoreAsync(Guid id, CancellationToken cancellationToken)
    {
        var connection = await session.GetOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            "library.Book_GetById",
            new { Id = id },
            session.Transaction,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);
        using var results = await connection.QueryMultipleAsync(command);
        var row = await results.ReadSingleOrDefaultAsync<BookRow>();
        if (row is null)
        {
            return null;
        }

        var authors = (await results.ReadAsync<AuthorRow>()).Select(MapAuthor).ToList();
        return Map(row, authors);
    }

    private async Task<Book> AddCoreAsync(Book entity, CancellationToken cancellationToken)
    {
        var status = await ExecuteMutationAsync("library.Book_Insert", BuildParameters(entity), cancellationToken);
        status.EnsureSucceeded("Book");
        return await GetByIdAsync(entity.Id, cancellationToken) ?? throw new InvalidOperationException("Inserted book could not be read.");
    }

    private async Task<Book> UpdateCoreAsync(Book entity, byte[] expectedRowVersion, CancellationToken cancellationToken)
    {
        var parameters = BuildParameters(entity);
        parameters.Add("ExpectedRowVersion", expectedRowVersion, DbType.Binary);
        var status = await ExecuteMutationAsync("library.Book_Update", parameters, cancellationToken);
        status.EnsureSucceeded("Book");
        return await GetByIdAsync(entity.Id, cancellationToken) ?? throw new InvalidOperationException("Updated book could not be read.");
    }

    private async Task RemoveCoreAsync(Guid id, byte[] expectedRowVersion, CancellationToken cancellationToken)
    {
        var status = await ExecuteMutationAsync("library.Book_SoftDelete", new { Id = id, ExpectedRowVersion = expectedRowVersion }, cancellationToken);
        status.EnsureRemoved("Book");
    }

    private async Task<PagedResult<Book>> SearchCoreAsync(
        BookSearchCriteria searchCriteria,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var connection = await session.GetOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            "library.Book_Search",
            new
            {
                searchCriteria.Query,
                SearchFields = (int)searchCriteria.Fields,
                Page = page,
                PageSize = pageSize
            },
            session.Transaction,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);
        using var results = await connection.QueryMultipleAsync(command);
        var rows = (await results.ReadAsync<BookRow>()).AsList();
        var authorRows = (await results.ReadAsync<AuthorRow>()).AsList();
        var totalCount = await results.ReadSingleAsync<int>();
        var authors = authorRows.Where(x => x.BookId.HasValue).GroupBy(x => x.BookId!.Value).ToDictionary(x => x.Key, x => x.Select(MapAuthor).ToList());
        var books = rows.Select(row => Map(row, authors.GetValueOrDefault(row.Id) ?? [])).ToArray();
        return new PagedResult<Book>(books, page, pageSize, totalCount);
    }

    private async Task<RepositoryMutationStatus> ExecuteMutationAsync(string procedure, object parameters, CancellationToken cancellationToken)
    {
        var connection = await session.GetOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(procedure, parameters, session.Transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
        return (RepositoryMutationStatus)await connection.QuerySingleAsync<int>(command);
    }

    private static DynamicParameters BuildParameters(Book book)
    {
        var ids = new DataTable();
        ids.Columns.Add("Id", typeof(Guid));
        foreach (var author in book.Authors)
        {
            ids.Rows.Add(author.Id);
        }

        var parameters = new DynamicParameters();
        parameters.Add("Id", book.Id, DbType.Guid);
        parameters.Add("Title", book.Title, DbType.String, size: 300);
        parameters.Add("PublicationYear", book.PublicationYear, DbType.Int16);
        parameters.Add("TableOfContents", book.TableOfContentsXml, DbType.Xml);
        parameters.Add("AuthorIds", ids.AsTableValuedParameter("library.EntityIdList"));
        return parameters;
    }

    private async Task<T> RunAsync<T>(Func<Task<T>> action, string operation, Guid? id = null)
    {
        try
        {
            return await action();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to {DatabaseOperation} for book {BookId}", operation, id);
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
            logger.LogError(exception, "Failed to {DatabaseOperation} for book {BookId}", operation, id);
            throw;
        }
    }

    private static Book Map(BookRow row, List<Author> authors) => new()
    {
        Id = row.Id,
        Title = row.Title,
        PublicationYear = row.PublicationYear,
        TableOfContentsXml = row.TableOfContentsXml,
        CreatedAt = row.CreatedAt,
        UpdatedAt = row.UpdatedAt,
        RowVersion = row.RowVersion,
        Authors = authors
    };

    private static Author MapAuthor(AuthorRow row) => new()
    {
        Id = row.Id,
        Name = row.Name,
        CreatedAt = row.CreatedAt,
        UpdatedAt = row.UpdatedAt,
        RowVersion = row.RowVersion
    };
}
