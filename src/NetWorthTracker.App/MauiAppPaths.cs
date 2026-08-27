using Microsoft.Extensions.Options;

using NetWorthTracker.Data.Api;

namespace NetWorthTracker.App;

public class MauiAppPaths : IAppPaths
{
    public MauiAppPaths(IOptions<AppPathOptions> options)
    {
        var appPathOptions = options.Value;
        var databaseDirectory = appPathOptions.DatabaseDirectory;

        databaseDirectory = string.IsNullOrWhiteSpace(databaseDirectory)
            ? FileSystem.AppDataDirectory
            : ResolveConfiguredDirectory(databaseDirectory);

        Directory.CreateDirectory(databaseDirectory);

        DatabaseFilePath = Path.Combine(databaseDirectory, appPathOptions.DatabaseFileName);
    }

    public string DatabaseFilePath { get; }

    private static string ResolveConfiguredDirectory(string configuredDirectory)
    {
        var expandedDirectory = Environment.ExpandEnvironmentVariables(configuredDirectory);

        if (Path.IsPathRooted(expandedDirectory))
        {
            return expandedDirectory;
        }

        var baseDirectory = FindSolutionDirectory() ?? AppContext.BaseDirectory;

        return Path.GetFullPath(Path.Combine(baseDirectory, expandedDirectory));
    }

    private static string? FindSolutionDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (directory.EnumerateFiles("*.sln").Any())
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
