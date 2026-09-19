using Application.Dependencies.DataAccess;
using Application.Extensions;
using Application.Dto;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.CommandHandlers.Books.Update;

public sealed class UpdateBookCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<UpdateBookCommandHandler> logger)
    : IRequestHandler<UpdateBookCommand, BookDto>
{
    public async Task<BookDto> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var book = await unitOfWork.Books.UpdateAsync(new Book
            {
                Id = request.Id,
                Title = request.Title.Trim(),
                PublicationYear = request.PublicationYear,
                TableOfContentsXml = request.TableOfContentsXml,
                Authors = request.AuthorIds.Select(id => new Author { Id = id }).ToList()
            }, request.RowVersion, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            logger.LogInformation("Book {BookId} was updated", book.Id);
            return book.ToDto();
        }
        catch (Exception exception)
        {
            await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            logger.LogError(exception, "Failed to update book {BookId}", request.Id);
            throw;
        }
    }
}
