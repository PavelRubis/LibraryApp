using Application.CommandHandlers.Books.Create;

namespace Application.UnitTests;

public sealed class BookValidatorTests
{
    private readonly CreateBookCommandValidator _validator = new();

    [Fact]
    public async Task ValidBookPassesValidation()
    {
        var command = new CreateBookCommand("CLR via C#", 2024, "<toc><h1>Runtime</h1></toc>", [Guid.NewGuid()]);

        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task DuplicateAuthorIdsFailValidation()
    {
        var authorId = Guid.NewGuid();
        var command = new CreateBookCommand("CLR via C#", 2024, "<toc />", [authorId, authorId]);

        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.AuthorIds));
    }
}
