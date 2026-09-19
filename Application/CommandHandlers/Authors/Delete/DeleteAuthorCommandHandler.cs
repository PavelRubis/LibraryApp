using Application.Dependencies.DataAccess;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.CommandHandlers.Authors.Delete;

public sealed class DeleteAuthorCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<DeleteAuthorCommandHandler> logger)
    : IRequestHandler<DeleteAuthorCommand>
{
    public async Task Handle(DeleteAuthorCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await unitOfWork.Authors.RemoveAsync(request.Id, request.RowVersion, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            logger.LogInformation("Author {AuthorId} was deleted", request.Id);
        }
        catch (Exception exception)
        {
            await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            logger.LogError(exception, "Failed to delete author {AuthorId}", request.Id);
            throw;
        }
    }
}
