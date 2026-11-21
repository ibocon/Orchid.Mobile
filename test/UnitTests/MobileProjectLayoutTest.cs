using FluentAssertions;
using Xunit;

namespace Orchid.UnitTests.Mobile;

/// <summary>
/// Orchid.Mobile 호스트가 하네스가 검사하는 모바일 프로젝트 경로에 배치되는지 검증합니다.
/// </summary>
public sealed class MobileProjectLayoutTest
{
    /// <summary>
    /// 표준 모바일 프로젝트 루트가 실제로 존재하는지 검증합니다.
    /// </summary>
    [Fact]
    public void GivenRepositoryLayoutWhenResolvingMobileProjectRootsThenMatchesHarnessConvention()
    {
        // Given
        string repositoryRoot = FindRepositoryRoot();

        // When
        string projectPath = Path.Combine(repositoryRoot, "src", "Maui", "Orchid.Mobile.csproj");
        string testProjectPath = Path.Combine(repositoryRoot, "test", "UnitTests", "Orchid.Mobile.UnitTests.csproj");
        string componentPath = Path.Combine(repositoryRoot, "src", "Maui", "Component");

        // Then
        File.Exists(projectPath).Should().BeTrue();
        File.Exists(testProjectPath).Should().BeTrue();
        Directory.Exists(componentPath).Should().BeTrue();
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
