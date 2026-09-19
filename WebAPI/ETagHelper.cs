using FluentValidation;
using FluentValidation.Results;

namespace WebAPI;

internal static class ETagHelper
{
    public static string Format(byte[] rowVersion) => $"\"{Convert.ToBase64String(rowVersion)}\"";

    public static byte[] Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw Invalid("If-Match header is required.");
        }

        var text = value.Trim();
        if (text.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
        {
            text = text[2..].Trim();
        }

        text = text.Trim('"');

        try
        {
            var bytes = Convert.FromBase64String(text);
            return bytes.Length == 8 ? bytes : throw Invalid("If-Match must contain an 8-byte row version.");
        }
        catch (FormatException)
        {
            throw Invalid("If-Match is not a valid Base64 row version.");
        }
    }

    private static ValidationException Invalid(string message) =>
        new([new ValidationFailure("If-Match", message)]);
}

