using Application.Common;
using Domain.Entities;

namespace Application.Dependencies.DataAccess;

public interface IBookRepository : IRepository<Book>
{
    Task<PagedResult<Book>> SearchAsync(
        string? query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}

