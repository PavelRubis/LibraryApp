using Application.Dto;
using MediatR;

namespace Application.CommandHandlers.Books.Create;

public sealed record CreateBookCommand(
    string Title,
    short PublicationYear,
    string TableOfContentsXml,
    IReadOnlyCollection<Guid> AuthorIds) : IRequest<BookDto>;
