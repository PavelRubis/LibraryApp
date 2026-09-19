using Application.Dependencies.DataAccess;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.CommandHandlers.Books.Delete;

public sealed class DeleteBookCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<DeleteBookCommandHandler> logger)
    : IRequestHandler<DeleteBookCommand>
{
    public async Task Handle(DeleteBookCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await unitOfWork.Books.RemoveAsync(request.Id, request.RowVersion, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            logger.LogInformation("Book {BookId} was deleted", request.Id);
        }
        catch (Exception exception)
        {
            await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            logger.LogError(exception, "Failed to delete book {BookId}", request.Id);
            throw;
        }
    }
}
