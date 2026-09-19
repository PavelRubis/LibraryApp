using Application.Dependencies.DataAccess;
using Application.Extensions;
using Application.Dto;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.CommandHandlers.Authors.Update;

public sealed class UpdateAuthorCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<UpdateAuthorCommandHandler> logger)
    : IRequestHandler<UpdateAuthorCommand, AuthorDto>
{
    public async Task<AuthorDto> Handle(UpdateAuthorCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var author = await unitOfWork.Authors.UpdateAsync(new Author
            {
                Id = request.Id,
                Name = request.Name.Trim()
            }, request.RowVersion, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            logger.LogInformation("Author {AuthorId} was updated", author.Id);
            return author.ToDto();
        }
        catch (Exception exception)
        {
            await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            logger.LogError(exception, "Failed to update author {AuthorId}", request.Id);
            throw;
        }
    }
}
