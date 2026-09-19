using Application.Dependencies.DataAccess;
using Application.Extensions;
using Application.Dto;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.CommandHandlers.Authors.Create;

public sealed class CreateAuthorCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<CreateAuthorCommandHandler> logger)
    : IRequestHandler<CreateAuthorCommand, AuthorDto>
{
    public async Task<AuthorDto> Handle(CreateAuthorCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var author = await unitOfWork.Authors.AddAsync(new Author
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim()
            }, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            logger.LogInformation("Author {AuthorId} was created", author.Id);
            return author.ToDto();
        }
        catch (Exception exception)
        {
            await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            logger.LogError(exception, "Failed to create author");
            throw;
        }
    }
}
