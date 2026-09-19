using Domain.Common;

namespace Application.Dependencies.DataAccess;

public interface IRepository<TEntity> where TEntity : IHasId
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity> UpdateAsync(
        TEntity entity,
        byte[] expectedRowVersion,
        CancellationToken cancellationToken = default);
    Task RemoveAsync(
        Guid id,
        byte[] expectedRowVersion,
        CancellationToken cancellationToken = default);
}
