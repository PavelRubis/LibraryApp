using System.Xml;
using System.Xml.Linq;

namespace Application.Common;

public static class TableOfContentsValidator
{
    public const int MaximumLength = 1024 * 1024;

    public static bool IsValid(string? xml, out string? error)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            error = "Table of contents is required.";
            return false;
        }

        if (xml.Length > MaximumLength)
        {
            error = "Table of contents must not exceed 1 MB.";
            return false;
        }

        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = MaximumLength
            };

            using var stringReader = new StringReader(xml);
            using var xmlReader = XmlReader.Create(stringReader, settings);
            var document = XDocument.Load(xmlReader, LoadOptions.PreserveWhitespace);

            if (document.Root is null || document.Root.Name.LocalName != "toc")
            {
                error = "The root element must be <toc>.";
                return false;
            }

            foreach (var element in document.Root.DescendantsAndSelf())
            {
                if (element.Name.LocalName.Equals("script", StringComparison.OrdinalIgnoreCase))
                {
                    error = "The <script> element is not allowed.";
                    return false;
                }

                foreach (var attribute in element.Attributes())
                {
                    if (attribute.Name.LocalName.StartsWith("on", StringComparison.OrdinalIgnoreCase))
                    {
                        error = "Event handler attributes are not allowed.";
                        return false;
                    }

                    if (attribute.Name.LocalName is "href" or "src"
                        && IsDangerousUrl(attribute.Value))
                    {
                        error = "Unsafe URL is not allowed.";
                        return false;
                    }
                }
            }

            error = null;
            return true;
        }
        catch (XmlException exception)
        {
            error = exception.Message;
            return false;
        }
    }

    private static bool IsDangerousUrl(string value)
    {
        var trimmed = value.Trim();
        return trimmed.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("vbscript:", StringComparison.OrdinalIgnoreCase);
    }
}
