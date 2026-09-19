using MediatR;

namespace Application.CommandHandlers.Authors.Delete;

public sealed record DeleteAuthorCommand(Guid Id, byte[] RowVersion) : IRequest;
