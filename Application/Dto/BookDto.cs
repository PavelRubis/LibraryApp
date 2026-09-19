namespace Application.Dto;

public sealed record BookDto(
    Guid Id,
    string Title,
    short PublicationYear,
    string TableOfContentsXml,
    IReadOnlyCollection<AuthorDto> Authors,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    byte[] RowVersion);
