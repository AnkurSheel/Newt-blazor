using Microsoft.Extensions.Configuration;

namespace NetWorthTracker.App;

internal static class LocalEnvFile
{
    public static void AddToConfiguration(ConfigurationManager configuration)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        AddFile(values, ".env");
        AddFile(values, ".env.local");

        var environmentLicenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");

        if (!string.IsNullOrWhiteSpace(environmentLicenseKey))
        {
            values["Syncfusion:LicenseKey"] = environmentLicenseKey;
        }

        if (values.Count > 0)
        {
            configuration.AddInMemoryCollection(values);
        }
    }

    private static void AddFile(IDictionary<string, string?> values, string fileName)
    {
        var path = FindFile(fileName);

        if (path is null)
        {
            return;
        }

        foreach (var line in File.ReadLines(path))
        {
            AddLine(values, line);
        }
    }

    private static void AddLine(IDictionary<string, string?> values, string line)
    {
        var trimmedLine = line.Trim();

        if (trimmedLine.Length == 0 || trimmedLine.StartsWith('#'))
        {
            return;
        }

        var separatorIndex = trimmedLine.IndexOf('=');

        if (separatorIndex <= 0)
        {
            return;
        }

        var key = trimmedLine[..separatorIndex].Trim();
        var value = trimmedLine[(separatorIndex + 1)..].Trim();

        if (value.Length >= 2 &&
            ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\'')))
        {
            value = value[1..^1];
        }

        values[key.Replace("__", ":", StringComparison.Ordinal)] = value;

        if (key.Equals("SYNCFUSION_LICENSE_KEY", StringComparison.OrdinalIgnoreCase))
        {
            values["Syncfusion:LicenseKey"] = value;
        }
    }

    private static string? FindFile(string fileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var path = Path.Combine(directory.FullName, fileName);

            if (File.Exists(path))
            {
                return path;
            }

            if (directory.EnumerateFiles("*.sln").Any())
            {
                return null;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
