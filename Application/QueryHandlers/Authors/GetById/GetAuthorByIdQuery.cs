using Application.Dto;
using MediatR;

namespace Application.QueryHandlers.Authors.GetById;

public sealed record GetAuthorByIdQuery(Guid Id) : IRequest<AuthorDto>;
