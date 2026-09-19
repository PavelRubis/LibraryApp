using Application.Common;
using Application.Dependencies.DataAccess;
using Application.Extensions;
using Application.Dto;
using MediatR;

namespace Application.QueryHandlers.Books.Search;

public sealed class SearchBooksQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<SearchBooksQuery, PagedResult<BookListItemDto>>
{
    public async Task<PagedResult<BookListItemDto>> Handle(SearchBooksQuery request, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.Books.SearchAsync(request.Query?.Trim(), request.Page, request.PageSize, cancellationToken);
        return new PagedResult<BookListItemDto>(result.Items.Select(x => x.ToListItemDto()).ToArray(), result.Page, result.PageSize, result.TotalCount);
    }
}
