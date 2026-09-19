using Application.Dependencies.DataAccess;
using Application.Extensions;
using Application.Dto;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.CommandHandlers.Books.Create;

public sealed class CreateBookCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<CreateBookCommandHandler> logger)
    : IRequestHandler<CreateBookCommand, BookDto>
{
    public async Task<BookDto> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var book = await unitOfWork.Books.AddAsync(new Book
            {
                Id = Guid.NewGuid(),
                Title = request.Title.Trim(),
                PublicationYear = request.PublicationYear,
                TableOfContentsXml = request.TableOfContentsXml,
                Authors = request.AuthorIds.Select(id => new Author { Id = id }).ToList()
            }, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            logger.LogInformation("Book {BookId} was created", book.Id);
            return book.ToDto();
        }
        catch (Exception exception)
        {
            await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            logger.LogError(exception, "Failed to create book");
            throw;
        }
    }
}
