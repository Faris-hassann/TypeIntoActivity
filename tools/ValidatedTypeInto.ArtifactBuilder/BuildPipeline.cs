using System.Diagnostics;

namespace Vodafone.Robotics.ValidatedTypeInto.ArtifactBuilder;

internal sealed record BuildStep(string Name, IReadOnlyList<string> Arguments);

internal sealed record BuildResult(bool Succeeded, BuildStep? FailedStep, int ExitCode)
{
    public static BuildResult Success { get; } = new(true, null, 0);
}

internal interface ICommandRunner
{
    Task<int> RunAsync(string executablePath, IReadOnlyList<string> arguments,
        string workingDirectory, CancellationToken cancellationToken);
}

internal sealed class ConsoleCommandRunner : ICommandRunner
{
    public async Task<int> RunAsync(string executablePath, IReadOnlyList<string> arguments,
        string workingDirectory, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false
        };
        foreach (var argument in arguments) startInfo.ArgumentList.Add(argument);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Could not start '{executablePath}'.");
        try
        {
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            throw;
        }
        return process.ExitCode;
    }
}

internal sealed class BuildPipeline(ICommandRunner commandRunner, TextWriter output)
{
    internal static IReadOnlyList<BuildStep> Steps { get; } =
    [
        new("Restore packaging project",
        [
            "restore", "src/ValidatedTypeInto.Packaging/ValidatedTypeInto.Packaging.csproj",
            "--configfile", "NuGet.config"
        ]),
        new("Build UiPath package",
        [
            "build", "src/ValidatedTypeInto.Packaging/ValidatedTypeInto.Packaging.csproj",
            "-c", "Release", "--no-restore"
        ]),
        new("Restore solution",
        [
            "restore", "ValidatedTypeInto.slnx", "--configfile", "NuGet.config"
        ]),
        new("Build solution",
        [
            "build", "ValidatedTypeInto.slnx", "-c", "Release", "--no-restore"
        ]),
        new("Run activity tests",
        [
            "test", "tests/ValidatedTypeInto.Tests/ValidatedTypeInto.Tests.csproj",
            "-c", "Release", "--no-build", "--no-restore"
        ]),
        new("Run artifact-builder tests",
        [
            "test", "tests/ValidatedTypeInto.ArtifactBuilder.Tests/ValidatedTypeInto.ArtifactBuilder.Tests.csproj",
            "-c", "Release", "--no-build", "--no-restore"
        ])
    ];

    public async Task<BuildResult> RunAsync(string dotNetExecutable, string repositoryRoot,
        CancellationToken cancellationToken)
    {
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            output.WriteLine();
            output.WriteLine($"[{index + 1}/{Steps.Count}] {step.Name}");
            output.WriteLine(new string('-', step.Name.Length + 6));
            var exitCode = await commandRunner.RunAsync(
                dotNetExecutable, step.Arguments, repositoryRoot, cancellationToken).ConfigureAwait(false);
            if (exitCode != 0) return new BuildResult(false, step, exitCode);
        }
        return BuildResult.Success;
    }
}
