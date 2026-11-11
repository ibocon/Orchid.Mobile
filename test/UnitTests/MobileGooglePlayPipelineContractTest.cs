using FluentAssertions;
using Xunit;

namespace Orchid.UnitTests.Mobile;

public sealed class MobileGooglePlayPipelineContractTest
{
    [Fact]
    public void CsprojAndWorkflowShouldConfigureGooglePlaySignedAabUpload()
    {
        // Given
        string repoRoot = FindRepositoryRoot();
        string csproj = File.ReadAllText(Path.Combine(repoRoot, "src/Orchid.Mobile.csproj"));
        string workflow = File.ReadAllText(Path.Combine(repoRoot, ".github/workflows/publish-orchid-google-play-internal.yml"));

        // When & Then
        csproj.Should().Contain("<ApplicationId>com.botanicsoft.orchid</ApplicationId>");
        workflow.Should().Contain("-p:AndroidPackageFormat=aab");
        workflow.Should().Contain("r0adkll/upload-google-play@");
        workflow.Should().Contain("packageName: com.botanicsoft.orchid");
        workflow.Should().Contain("https://stag.botanicsoft.com/orchid");
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? dir = new(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Orchid.Mobile.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Orchid.Mobile.slnx not found.");
    }
}