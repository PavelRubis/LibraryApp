using Application.Common;
using Application.Dependencies.DataAccess;
using Application.Extensions;
using Application.Dto;
using MediatR;

namespace Application.QueryHandlers.Authors.GetById;

public sealed class GetAuthorByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAuthorByIdQuery, AuthorDto>
{
    public async Task<AuthorDto> Handle(GetAuthorByIdQuery request, CancellationToken cancellationToken)
    {
        var author = await unitOfWork.Authors.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Author was not found.");
        return author.ToDto();
    }
}
