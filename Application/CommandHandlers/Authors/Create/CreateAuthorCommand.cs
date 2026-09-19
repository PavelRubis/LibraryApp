using Application.Dto;
using MediatR;

namespace Application.CommandHandlers.Authors.Create;

public sealed record CreateAuthorCommand(string Name) : IRequest<AuthorDto>;
