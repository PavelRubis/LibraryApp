using MediatR;

namespace Application.CommandHandlers.Books.Delete;

public sealed record DeleteBookCommand(Guid Id, byte[] RowVersion) : IRequest;
