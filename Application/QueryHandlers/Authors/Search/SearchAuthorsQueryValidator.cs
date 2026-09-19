using FluentValidation;

namespace Application.QueryHandlers.Authors.Search;

public sealed class SearchAuthorsQueryValidator : AbstractValidator<SearchAuthorsQuery>
{
    public SearchAuthorsQueryValidator()
    {
        RuleFor(x => x.Query).MaximumLength(200);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
