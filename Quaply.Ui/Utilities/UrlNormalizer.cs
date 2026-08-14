namespace Quaply.Ui.Utilities;

public static class UrlNormalizer
{
    public static string ExtractLinkedInUsername(string input)
    {
        string value = input.Trim();

        if (!LooksLikeUrl(value))
        {
            return value;
        }

        Uri? uri = ToUri(value);
        if (uri is null)
        {
            return value;
        }

        string[] segments = uri
            .AbsolutePath.Trim('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        // The path is typically /in/username or /company/name
        // -> extract the last segment.
        return segments.Length > 0 ? segments[1] : value;
    }

    public static string ExtractGithubUsername(string input)
    {
        string value = input.Trim();

        if (!LooksLikeUrl(value))
        {
            return value;
        }

        Uri? uri = ToUri(value);
        if (uri is null)
        {
            return value;
        }

        string[] segments = uri
            .AbsolutePath.Trim('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        // The path is /username -> take the first segment.
        return segments.Length > 0 ? segments[0] : value;
    }

    public static string ExtractDomain(string input)
    {
        string value = input.Trim();

        Uri? uri = ToUri(value);
        if (uri is null)
        {
            return value.TrimEnd('/');
        }

        string host = uri.Host;
        if (host.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
        {
            host = host[4..];
        }

        return host;
    }

    private static bool LooksLikeUrl(string value)
    {
        return value.Contains("http", StringComparison.OrdinalIgnoreCase)
            || value.Contains(
                "linkedin.com",
                StringComparison.OrdinalIgnoreCase
            )
            || value.Contains("github.com", StringComparison.OrdinalIgnoreCase)
            || value.Contains("www.", StringComparison.OrdinalIgnoreCase);
    }

    private static Uri? ToUri(string value)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out Uri? uri))
        {
            return uri;
        }

        // The user might paste the link without the protocol.
        // For example, "linkedin.com/in/john".
        if (Uri.TryCreate("https://" + value, UriKind.Absolute, out uri))
        {
            return uri;
        }

        return null;
    }
}
