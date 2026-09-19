using Application.Dto;
using Domain.Entities;

namespace Application.Extensions;

internal static class BookMappings
{
    public static BookDto ToDto(this Book book) => new(book.Id, book.Title, book.PublicationYear, book.TableOfContentsXml, book.Authors.Select(x => x.ToDto()).ToArray(), book.CreatedAt, book.UpdatedAt, book.RowVersion);

    public static BookListItemDto ToListItemDto(this Book book) => new(book.Id, book.Title, book.PublicationYear, book.Authors.Select(x => x.ToDto()).ToArray(), book.CreatedAt, book.UpdatedAt, book.RowVersion);
}
