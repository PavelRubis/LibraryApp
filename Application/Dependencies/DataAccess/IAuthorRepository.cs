using Application.Common;
using Domain.Entities;

namespace Application.Dependencies.DataAccess;

public interface IAuthorRepository : IRepository<Author>
{
    Task<PagedResult<Author>> SearchAsync(
        string? query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}

