namespace Application.Dto;

public sealed record AuthorDto(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    byte[] RowVersion);
