using Application.Common;
using Application.Dependencies.DataAccess;
using Application.Extensions;
using Application.Dto;
using MediatR;

namespace Application.QueryHandlers.Books.GetById;

public sealed class GetBookByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetBookByIdQuery, BookDto>
{
    public async Task<BookDto> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
    {
        var book = await unitOfWork.Books.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Book was not found.");
        return book.ToDto();
    }
}
