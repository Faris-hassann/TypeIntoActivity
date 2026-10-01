namespace Vodafone.Robotics.ValidatedTypeInto.ArtifactBuilder;

internal static class RepositoryLocator
{
    private static readonly string[] RequiredPaths =
    {
        "ValidatedTypeInto.slnx",
        "NuGet.config",
        Path.Combine("src", "ValidatedTypeInto.Packaging", "ValidatedTypeInto.Packaging.csproj")
    };

    public static string? Find(params string?[] startingDirectories)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var startingDirectory in startingDirectories)
        {
            if (string.IsNullOrWhiteSpace(startingDirectory)) continue;
            DirectoryInfo? directory;
            try { directory = new DirectoryInfo(Path.GetFullPath(startingDirectory)); }
            catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
            {
                continue;
            }

            while (directory is not null && visited.Add(directory.FullName))
            {
                if (RequiredPaths.All(path => File.Exists(Path.Combine(directory.FullName, path))))
                    return directory.FullName;
                directory = directory.Parent;
            }
        }
        return null;
    }
}
