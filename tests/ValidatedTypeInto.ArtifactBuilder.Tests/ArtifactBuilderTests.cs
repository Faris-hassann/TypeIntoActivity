namespace ValidatedTypeInto.ArtifactBuilder.Tests;

public sealed class ArtifactBuilderTests
{
    [Fact]
    public void Repository_locator_walks_up_from_a_child_directory()
    {
        using var directory = new TemporaryDirectory();
        Directory.CreateDirectory(Path.Combine(directory.Path, "src", "ValidatedTypeInto.Packaging"));
        File.WriteAllText(Path.Combine(directory.Path, "ValidatedTypeInto.slnx"), "<Solution />");
        File.WriteAllText(Path.Combine(directory.Path, "NuGet.config"), "<configuration />");
        File.WriteAllText(Path.Combine(directory.Path, "src", "ValidatedTypeInto.Packaging",
            "ValidatedTypeInto.Packaging.csproj"), "<Project />");
        var child = Directory.CreateDirectory(Path.Combine(directory.Path, "a", "b")).FullName;

        Assert.Equal(directory.Path, RepositoryLocator.Find(child));
    }

    [Fact]
    public void Repository_locator_returns_null_when_markers_are_missing()
    {
        using var directory = new TemporaryDirectory();
        Assert.Null(RepositoryLocator.Find(directory.Path));
    }

    [Fact]
    public void Sdk_parser_selects_highest_supported_stable_or_preview_sdk()
    {
        const string output = "8.0.425 [C:\\dotnet]" + "\n" +
                              "10.0.100-preview.2 [C:\\dotnet]" + "\n" +
                              "10.0.401 [C:\\dotnet]";
        Assert.Equal(new Version(10, 0, 401), DotNetSdkLocator.ParseHighestSupportedSdk(output));
    }

    [Fact]
    public void Sdk_parser_rejects_versions_older_than_ten()
    {
        Assert.Null(DotNetSdkLocator.ParseHighestSupportedSdk("8.0.425 [C:\\dotnet]"));
    }

    [Fact]
    public void Sdk_selection_skips_incompatible_candidates()
    {
        var sdk = DotNetSdkLocator.SelectFirstCompatible(
            new[] { "old-dotnet", "new-dotnet" },
            candidate => candidate == "old-dotnet" ? "8.0.425 [x]" : "10.0.401 [x]");
        Assert.Equal("new-dotnet", sdk?.ExecutablePath);
    }

    [Fact]
    public void Metadata_reads_package_id_and_version_and_builds_expected_path()
    {
        using var directory = CreateMetadataRepository("1.2.3");
        var metadata = ArtifactLocator.ReadMetadata(directory.Path);
        Assert.Equal("Vodafone.Robotics.ValidatedTypeInto.Activities", metadata.PackageId);
        Assert.Equal("1.2.3", metadata.Version);
        Assert.Equal(Path.Combine(directory.Path, "artifacts", "packages",
            "Vodafone.Robotics.ValidatedTypeInto.Activities.1.2.3.nupkg"),
            metadata.GetExpectedPackagePath(directory.Path));
    }

    [Theory]
    [InlineData("1.0")]
    [InlineData("01.0.0")]
    [InlineData("1.0.0-beta")]
    public void Metadata_rejects_non_stable_versions(string version)
    {
        using var directory = CreateMetadataRepository(version);
        Assert.Throws<PackageConfigurationException>(() => ArtifactLocator.ReadMetadata(directory.Path));
    }

    [Fact]
    public async Task Pipeline_runs_all_steps_in_order()
    {
        var runner = new RecordingRunner();
        var result = await new BuildPipeline(runner, TextWriter.Null)
            .RunAsync("dotnet.exe", "repo", CancellationToken.None);
        Assert.True(result.Succeeded);
        Assert.Equal(BuildPipeline.Steps.Select(step => step.Name), runner.StepNames);
    }

    [Fact]
    public async Task Pipeline_stops_immediately_after_a_failed_step()
    {
        var runner = new RecordingRunner(failAtCall: 3);
        var result = await new BuildPipeline(runner, TextWriter.Null)
            .RunAsync("dotnet.exe", "repo", CancellationToken.None);
        Assert.False(result.Succeeded);
        Assert.Equal(BuildPipeline.Steps[2], result.FailedStep);
        Assert.Equal(42, result.ExitCode);
        Assert.Equal(3, runner.StepNames.Count);
    }

    private static TemporaryDirectory CreateMetadataRepository(string version)
    {
        var directory = new TemporaryDirectory();
        var packageDirectory = Path.Combine(directory.Path, "src", "ValidatedTypeInto.Packaging");
        Directory.CreateDirectory(packageDirectory);
        File.WriteAllText(Path.Combine(packageDirectory, "ValidatedTypeInto.Packaging.csproj"),
            "<Project><PropertyGroup><PackageId>Vodafone.Robotics.ValidatedTypeInto.Activities</PackageId></PropertyGroup></Project>");
        File.WriteAllText(Path.Combine(directory.Path, "Directory.Build.props"),
            $"<Project><PropertyGroup><Version>{version}</Version></PropertyGroup></Project>");
        return directory;
    }

    private sealed class RecordingRunner(int? failAtCall = null) : ICommandRunner
    {
        public List<string> StepNames { get; } = new();

        public Task<int> RunAsync(string executablePath, IReadOnlyList<string> arguments,
            string workingDirectory, CancellationToken cancellationToken)
        {
            var step = BuildPipeline.Steps.Single(candidate => candidate.Arguments.SequenceEqual(arguments));
            StepNames.Add(step.Name);
            return Task.FromResult(StepNames.Count == failAtCall ? 42 : 0);
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(), "ValidatedTypeIntoBuilderTests", Guid.NewGuid().ToString("N"));
        public TemporaryDirectory() => Directory.CreateDirectory(Path);
        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
}
