using Application.Dto;
using MediatR;

namespace Application.CommandHandlers.Books.Update;

public sealed record UpdateBookCommand(
    Guid Id,
    string Title,
    short PublicationYear,
    string TableOfContentsXml,
    IReadOnlyCollection<Guid> AuthorIds,
    byte[] RowVersion) : IRequest<BookDto>;
