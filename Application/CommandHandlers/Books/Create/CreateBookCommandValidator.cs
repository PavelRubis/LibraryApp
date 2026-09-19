using Application.Common;
using FluentValidation;

namespace Application.CommandHandlers.Books.Create;

public sealed class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.PublicationYear).InclusiveBetween((short)1, (short)DateTime.UtcNow.Year);
        RuleFor(x => x.AuthorIds).NotEmpty().Must(x => x.Count <= 10).WithMessage("A book can have at most 10 authors.").Must(x => x.All(id => id != Guid.Empty)).WithMessage("Author IDs must not be empty.").Must(x => x.Distinct().Count() == x.Count).WithMessage("Author IDs must be unique.");
        RuleFor(x => x.TableOfContentsXml).Must((_, xml, context) => ValidateXml(xml, context)).WithMessage("{XmlError}");
    }

    private static bool ValidateXml(
        string xml,
        ValidationContext<CreateBookCommand> context)
    {
        if (TableOfContentsValidator.IsValid(xml, out var error))
        {
            return true;
        }

        context.MessageFormatter.AppendArgument("XmlError", error);
        return false;
    }
}
