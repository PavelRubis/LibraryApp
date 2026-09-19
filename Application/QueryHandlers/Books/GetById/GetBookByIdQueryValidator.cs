using FluentValidation;

namespace Application.QueryHandlers.Books.GetById;

public sealed class GetBookByIdQueryValidator : AbstractValidator<GetBookByIdQuery>
{
    public GetBookByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
