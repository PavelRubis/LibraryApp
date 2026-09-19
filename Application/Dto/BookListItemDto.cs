namespace Application.Dto;

public sealed record BookListItemDto(
    Guid Id,
    string Title,
    short PublicationYear,
    IReadOnlyCollection<AuthorDto> Authors,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    byte[] RowVersion);
