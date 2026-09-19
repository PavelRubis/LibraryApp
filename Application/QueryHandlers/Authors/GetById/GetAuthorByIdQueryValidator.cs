using FluentValidation;

namespace Application.QueryHandlers.Authors.GetById;

public sealed class GetAuthorByIdQueryValidator : AbstractValidator<GetAuthorByIdQuery>
{
    public GetAuthorByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
