using Application.Common;
using Application.Dto;
using MediatR;

namespace Application.QueryHandlers.Books.Search;

public sealed record SearchBooksQuery(string? Query, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<BookListItemDto>>;
