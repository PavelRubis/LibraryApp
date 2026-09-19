using Application.Dto;
using MediatR;

namespace Application.QueryHandlers.Books.GetById;

public sealed record GetBookByIdQuery(Guid Id) : IRequest<BookDto>;
