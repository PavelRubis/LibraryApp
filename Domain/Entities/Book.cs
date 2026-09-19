using Domain.Common;

namespace Domain.Entities;

public class Book : EntityBase
{
    public string Title { get; set; } = string.Empty;
    public short PublicationYear { get; set; }
    public string TableOfContentsXml { get; set; } = "<toc />";
    public List<Author> Authors { get; set; } = [];
}
