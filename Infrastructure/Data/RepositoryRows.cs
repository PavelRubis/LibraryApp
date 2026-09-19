namespace Infrastructure.Data;

internal sealed class AuthorRow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Guid? BookId { get; set; }
}

internal sealed class BookRow
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public short PublicationYear { get; set; }
    public string TableOfContentsXml { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
