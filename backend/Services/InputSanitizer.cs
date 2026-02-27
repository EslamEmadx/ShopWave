using System.Text.RegularExpressions;

namespace backend.Services;

public partial class InputSanitizer
{
    /// <summary>
    /// Strips HTML tags, blocks URLs/links, and trims whitespace.
    /// Used for review comments and user-generated content.
    /// </summary>
    public string SanitizeText(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Strip all HTML tags
        var result = HtmlTagRegex().Replace(input, string.Empty);

        // Block URLs (http, https, ftp, www)
        result = UrlRegex().Replace(result, "[link removed]");

        // Collapse excessive whitespace
        result = ExcessiveWhitespaceRegex().Replace(result, " ");

        return result.Trim();
    }

    /// <summary>
    /// Checks if the input contains prohibited content (links, HTML).
    /// Returns true if content is clean.
    /// </summary>
    public bool IsClean(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;

        return !HtmlTagRegex().IsMatch(input) && !UrlRegex().IsMatch(input);
    }

    [GeneratedRegex(@"<[^>]+>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"(https?://|ftp://|www\.)\S+", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex UrlRegex();

    [GeneratedRegex(@"\s{2,}", RegexOptions.Compiled)]
    private static partial Regex ExcessiveWhitespaceRegex();
}
