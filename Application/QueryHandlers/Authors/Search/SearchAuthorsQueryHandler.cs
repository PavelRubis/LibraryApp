using Application.Common;
using Application.Dependencies.DataAccess;
using Application.Extensions;
using Application.Dto;
using MediatR;

namespace Application.QueryHandlers.Authors.Search;

public sealed class SearchAuthorsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<SearchAuthorsQuery, PagedResult<AuthorDto>>
{
    public async Task<PagedResult<AuthorDto>> Handle(SearchAuthorsQuery request, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Authors.SearchAsync(request.Query?.Trim(), request.Page, request.PageSize, cancellationToken);
        return new PagedResult<AuthorDto>(result.Items.Select(x => x.ToDto()).ToArray(), result.Page, result.PageSize, result.TotalCount);
    }
}
