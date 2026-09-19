using System.Net;
using System.Net.Http.Json;
using Application.Common;
using Application.Dto;
using Microsoft.Data.SqlClient;
using WebAPI.Models;

namespace WebAPI.IntegrationTests;

public sealed class LibraryApiTests(LibraryApiFactory factory) : IClassFixture<LibraryApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public void MigrationsCanBeAppliedTwice()
    {
        factory.ApplyMigrationsAgain();
        factory.ApplyMigrationsAgain();
    }

    [Fact]
    public async Task SearchFindsBookByTitleAuthorAndXml()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var marker = Guid.NewGuid().ToString("N");
        var (author, book) = await CreateBookAsync($"SearchAuthor{marker}", $"SearchTitle{marker}", $"<toc><h1>SearchXml{marker}</h1></toc>", cancellationToken);

        var titleSearch = await _client.GetFromJsonAsync<PagedResult<BookListItemDto>>($"/api/books?q={book.Title}", cancellationToken);
        var authorSearch = await _client.GetFromJsonAsync<PagedResult<BookListItemDto>>($"/api/books?q={author.Name}", cancellationToken);
        var xmlSearch = await _client.GetFromJsonAsync<PagedResult<BookListItemDto>>($"/api/books?q=SearchXml{marker}", cancellationToken);

        Assert.Equal(book.Id, Assert.Single(titleSearch!.Items).Id);
        Assert.Equal(book.Id, Assert.Single(authorSearch!.Items).Id);
        Assert.Equal(book.Id, Assert.Single(xmlSearch!.Items).Id);

        var outOfRangeSearch = await _client.GetFromJsonAsync<PagedResult<BookListItemDto>>($"/api/books?q={book.Title}&page=999", cancellationToken);
        Assert.Empty(outOfRangeSearch!.Items);
        Assert.Equal(1, outOfRangeSearch.TotalCount);

        var outOfRangeAuthorSearch = await _client.GetFromJsonAsync<PagedResult<AuthorDto>>($"/api/authors?q={author.Name}&page=999", cancellationToken);
        Assert.Empty(outOfRangeAuthorSearch!.Items);
        Assert.Equal(1, outOfRangeAuthorSearch.TotalCount);
    }

    [Fact]
    public async Task ConcurrentUpdatesWithSameRowVersionAllowOnlyOneWriter()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var marker = Guid.NewGuid().ToString("N");
        var (author, book) = await CreateBookAsync($"ConcurrencyAuthor{marker}", $"ConcurrencyBook{marker}", "<toc><h1>Before</h1></toc>", cancellationToken);

        var firstUpdate = _client.PutAsJsonAsync($"/api/books/{book.Id}", new UpdateBookRequest(book.Title, book.PublicationYear, "<toc><h1>First</h1></toc>", [author.Id], book.RowVersion), cancellationToken);
        var secondUpdate = _client.PutAsJsonAsync($"/api/books/{book.Id}", new UpdateBookRequest(book.Title, book.PublicationYear, "<toc><h1>Second</h1></toc>", [author.Id], book.RowVersion), cancellationToken);

        var responses = await Task.WhenAll(firstUpdate, secondUpdate);
        Assert.Contains(responses, response => response.StatusCode == HttpStatusCode.OK);
        Assert.Contains(responses, response => response.StatusCode == HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DeletionUsesSoftDeleteAndProtectsUsedAuthor()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var marker = Guid.NewGuid().ToString("N");
        var authorName = $"DeleteAuthor{marker}";
        var (author, book) = await CreateBookAsync(authorName, $"DeleteBook{marker}", "<toc><h1>Delete</h1></toc>", cancellationToken);

        using var deleteUsedAuthor = CreateDeleteRequest($"/api/authors/{author.Id}", author.RowVersion);
        var authorConflict = await _client.SendAsync(deleteUsedAuthor, cancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, authorConflict.StatusCode);

        using var deleteBook = CreateDeleteRequest($"/api/books/{book.Id}", book.RowVersion);
        var deleteBookResponse = await _client.SendAsync(deleteBook, cancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, deleteBookResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/books/{book.Id}", cancellationToken)).StatusCode);

        await using var connection = new SqlConnection(factory.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("SELECT IsDeleted FROM library.Books WHERE Id = @Id", connection);
        command.Parameters.AddWithValue("Id", book.Id);
        Assert.True((bool)(await command.ExecuteScalarAsync(cancellationToken))!);

        using var deleteAuthor = CreateDeleteRequest($"/api/authors/{author.Id}", author.RowVersion);
        var deleteAuthorResponse = await _client.SendAsync(deleteAuthor, cancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, deleteAuthorResponse.StatusCode);

        var replacement = await CreateAuthorAsync(authorName, cancellationToken);
        Assert.NotEqual(author.Id, replacement.Id);
    }

    [Fact]
    public async Task InvalidXmlReturnsProblemDetails()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var author = await CreateAuthorAsync($"XmlAuthor{Guid.NewGuid():N}", cancellationToken);

        var response = await _client.PostAsJsonAsync("/api/books", new CreateBookRequest("Invalid XML", 1968, "<toc><script>alert(1)</script></toc>", [author.Id]), cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task MissingAuthorRollsBackBookCreation()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var title = $"TransactionTest{Guid.NewGuid():N}";
        var response = await _client.PostAsJsonAsync("/api/books", new CreateBookRequest(title, 2020, "<toc />", [Guid.NewGuid()]), cancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        await using var connection = new SqlConnection(factory.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("SELECT COUNT(*) FROM library.Books WHERE Title = @Title", connection);
        command.Parameters.AddWithValue("Title", title);
        Assert.Equal(0, Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)));
    }

    private async Task<(AuthorDto Author, BookDto Book)> CreateBookAsync(
        string authorName,
        string title,
        string tableOfContentsXml,
        CancellationToken cancellationToken)
    {
        var author = await CreateAuthorAsync(authorName, cancellationToken);
        var response = await _client.PostAsJsonAsync("/api/books", new CreateBookRequest(title, 2024, tableOfContentsXml, [author.Id]), cancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookDto>(cancellationToken);
        Assert.NotNull(book);
        Assert.Equal(author.Id, Assert.Single(book.Authors).Id);
        return (author, book);
    }

    private async Task<AuthorDto> CreateAuthorAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var response = await _client.PostAsJsonAsync("/api/authors", new CreateAuthorRequest(name), cancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<AuthorDto>(cancellationToken))!;
    }

    private static HttpRequestMessage CreateDeleteRequest(string path, byte[] rowVersion)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, path);
        request.Headers.TryAddWithoutValidation("If-Match", $"\"{Convert.ToBase64String(rowVersion)}\"");
        return request;
    }
}
