using System.Diagnostics;

namespace Vodafone.Robotics.ValidatedTypeInto.ArtifactBuilder;

internal sealed record DotNetSdk(string ExecutablePath, Version Version);

internal static class DotNetSdkLocator
{
    public const int MinimumMajorVersion = 10;

    public static DotNetSdk? Find()
    {
        foreach (var candidate in GetCandidates())
        {
            var version = ProbeHighestSupportedSdk(candidate);
            if (version is not null) return new DotNetSdk(candidate, version);
        }
        return null;
    }

    internal static Version? ParseHighestSupportedSdk(string output, int minimumMajorVersion = MinimumMajorVersion)
    {
        Version? highest = null;
        foreach (var line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var versionText = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            versionText = versionText?.Split('-', 2)[0];
            if (!Version.TryParse(versionText, out var version) || version.Major < minimumMajorVersion) continue;
            if (highest is null || version > highest) highest = version;
        }
        return highest;
    }

    internal static DotNetSdk? SelectFirstCompatible(IEnumerable<string> candidates, Func<string, string?> probe)
    {
        foreach (var candidate in candidates)
        {
            var output = probe(candidate);
            if (output is null) continue;
            var version = ParseHighestSupportedSdk(output);
            if (version is not null) return new DotNetSdk(candidate, version);
        }
        return null;
    }

    private static IEnumerable<string> GetCandidates()
    {
        var candidates = new List<string>();
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        candidates.AddRange(path.Split(Path.PathSeparator,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(directory => Path.Combine(directory.Trim('"'), "dotnet.exe")));
        AddFromRoot(candidates, Environment.GetEnvironmentVariable("DOTNET_ROOT"));
        AddFromRoot(candidates, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!string.IsNullOrWhiteSpace(localAppData))
            candidates.Add(Path.Combine(localAppData, "Microsoft", "dotnet", "dotnet.exe"));
        return candidates.Where(File.Exists).Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static void AddFromRoot(ICollection<string> candidates, string? root)
    {
        if (string.IsNullOrWhiteSpace(root)) return;
        var normalizedRoot = root.Trim('"');
        candidates.Add(normalizedRoot.EndsWith("dotnet", StringComparison.OrdinalIgnoreCase)
            ? Path.Combine(normalizedRoot, "dotnet.exe")
            : Path.Combine(normalizedRoot, "dotnet", "dotnet.exe"));
    }

    private static Version? ProbeHighestSupportedSdk(string executablePath)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add("--list-sdks");
            using var process = Process.Start(startInfo);
            if (process is null) return null;
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return process.ExitCode == 0 ? ParseHighestSupportedSdk(output) : null;
        }
        catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or InvalidOperationException or IOException)
        {
            return null;
        }
    }
}
