using Application.Dto;
using MediatR;

namespace Application.CommandHandlers.Authors.Update;

public sealed record UpdateAuthorCommand(Guid Id, string Name, byte[] RowVersion)
    : IRequest<AuthorDto>;
