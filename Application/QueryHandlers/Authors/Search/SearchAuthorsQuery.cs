using Application.Common;
using Application.Dto;
using MediatR;

namespace Application.QueryHandlers.Authors.Search;

public sealed record SearchAuthorsQuery(string? Query, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<AuthorDto>>;
