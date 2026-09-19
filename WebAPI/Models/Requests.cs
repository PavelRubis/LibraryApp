namespace WebAPI.Models;

public sealed record CreateAuthorRequest(string Name);

public sealed record UpdateAuthorRequest(string Name, byte[] RowVersion);

public sealed record CreateBookRequest(
    string Title,
    short PublicationYear,
    string TableOfContentsXml,
    IReadOnlyCollection<Guid> AuthorIds);

public sealed record UpdateBookRequest(
    string Title,
    short PublicationYear,
    string TableOfContentsXml,
    IReadOnlyCollection<Guid> AuthorIds,
    byte[] RowVersion);

