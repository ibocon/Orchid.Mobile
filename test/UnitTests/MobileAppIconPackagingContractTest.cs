using FluentAssertions;
using Xunit;

namespace Orchid.UnitTests.Mobile;

/// <summary>
/// Android 패키징에 필요한 MAUI 앱 아이콘 입력 계약을 검증합니다.
/// </summary>
public sealed class MobileAppIconPackagingContractTest
{
    [Fact]
    public void ProjectShouldDeclarePairedMauiIconWithExplicitBaseSize()
    {
        // Given
        string repositoryRoot = FindRepositoryRoot();
        string project = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Maui", "Orchid.Mobile.csproj"));
        string background = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Maui", "Resources", "AppIcon", "appicon.svg"));

        // When
        const string appIconDeclaration = "<MauiIcon Include=\"Resources\\AppIcon\\appicon.svg\"";

        // Then
        project.Should().Contain(appIconDeclaration);
        project.Should().Contain("ForegroundFile=\"Resources\\AppIcon\\appiconfg.svg\"");
        project.Should().Contain("ForegroundScale=\"0.84\"");
        project.Should().Contain("BaseSize=\"1024,1024\"");
        background.Should().Contain("viewBox=\"0 0 1024 1024\"");
    }

    [Fact]
    public void ForegroundShouldMatchCompactLoginMark()
    {
        // Given
        string repositoryRoot = FindRepositoryRoot();
        string foreground = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Maui", "Resources", "AppIcon", "appiconfg.svg"));
        string loginMark = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Maui", "wwwroot", "logo-login.svg"));

        // When
        bool isCompactThreePetalMark = foreground.Contains("id=\"petal-top\"", StringComparison.Ordinal);

        // Then
        foreground.Should().Be(loginMark);
        isCompactThreePetalMark.Should().BeTrue();
        foreground.Should().Contain("id=\"orchid-throat\"");
        foreground.Should().Contain("#6D5443");
        foreground.Should().NotContain("id=\"stem\"");
        foreground.Should().NotContain("vein");
    }

    [Fact]
    public void ForegroundShouldUseThickInkOutlineAndKeepSafeZoneScale()
    {
        // Given
        string repositoryRoot = FindRepositoryRoot();
        string project = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Maui", "Orchid.Mobile.csproj"));
        string foreground = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Maui", "Resources", "AppIcon", "appiconfg.svg"));

        // When & Then
        project.Should().Contain("ForegroundScale=\"0.84\"");
        foreground.Should().Contain("stroke=\"#6D5443\"").And.Contain("stroke-width=\"30\"");
        foreground.Should().NotContain("#A8796A");
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Orchid.Mobile.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Orchid.Mobile.slnx를 찾을 수 없습니다.");
    }
}
