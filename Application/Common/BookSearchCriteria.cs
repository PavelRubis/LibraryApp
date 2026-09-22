namespace Application.Common;

[Flags]
public enum BookSearchFields
{
    Title = 1,
    Author = 2,
    TableOfContents = 4,
    All = Title | Author | TableOfContents
}

public sealed record BookSearchCriteria(
    string? Query,
    BookSearchFields Fields);
