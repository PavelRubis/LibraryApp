using Application.Common;

namespace Application.UnitTests;

public sealed class TableOfContentsValidatorTests
{
    [Theory]
    [InlineData("<toc />")]
    [InlineData("<toc><h1>Part one</h1><ul><li>Chapter</li></ul></toc>")]
    [InlineData("<toc><a href=\"https://example.com\">Chapter</a></toc>")]
    public void ValidXmlIsAccepted(string xml)
    {
        Assert.True(TableOfContentsValidator.IsValid(xml, out var error));
        Assert.Null(error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("<div />")]
    [InlineData("<toc><p>not closed</toc>")]
    [InlineData("<toc><script>alert(1)</script></toc>")]
    [InlineData("<toc><a onclick=\"alert(1)\">Chapter</a></toc>")]
    [InlineData("<toc><a href=\"javascript:alert(1)\">Chapter</a></toc>")]
    [InlineData("<!DOCTYPE toc [<!ENTITY xxe SYSTEM 'file:///etc/passwd'>]><toc>&xxe;</toc>")]
    public void InvalidOrUnsafeXmlIsRejected(string xml)
    {
        Assert.False(TableOfContentsValidator.IsValid(xml, out var error));
        Assert.NotNull(error);
    }
}
