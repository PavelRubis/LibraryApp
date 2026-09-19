using Application.Dto;
using Domain.Entities;

namespace Application.Extensions;

internal static class AuthorMappings
{
    public static AuthorDto ToDto(this Author author) => new(author.Id, author.Name, author.CreatedAt, author.UpdatedAt, author.RowVersion);
}
