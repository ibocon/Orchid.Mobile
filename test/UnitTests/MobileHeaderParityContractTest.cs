// keep: FluentAssertions assertions
using FluentAssertions;
// keep: xUnit Fact attribute
using Xunit;

namespace Orchid.UnitTests.Mobile;

/// <summary>
/// 모바일 헤더가 데스크톱과 합의한 상단 정보 구조를 유지하는지 검증합니다.
/// </summary>
public sealed class MobileHeaderParityContractTest
{
    [Fact]
    public void GivenMobileHeaderWhenInspectingTrailingContentThenItUsesTheCommonHeaderOrder()
    {
        // Given
        string header = ReadRepositoryFile("src/Maui/Component/MobileHeader/MobileHeader.razor");

        // When
        int encryption = header.IndexOf("EncryptionLabel", StringComparison.Ordinal);
        int connection = header.IndexOf("GetConnectionLabel()", StringComparison.Ordinal);
        int theme = header.IndexOf("ToggleThemeLabel", StringComparison.Ordinal);
        int account = header.IndexOf("GardenAccountMenu", StringComparison.Ordinal);

        // Then
        encryption.Should().BeGreaterThanOrEqualTo(0);
        connection.Should().BeGreaterThan(encryption);
        theme.Should().BeGreaterThan(connection);
        account.Should().BeGreaterThan(theme);
    }

    [Fact]
    public void GivenMobileHeaderResourcesWhenInspectingThenEncryptionLabelIsLocalized()
    {
        // Given
        string resource = ReadRepositoryFile("src/Maui/Component/MobileHeader/MobileHeaderResource.cs");
        string defaultCulture = ReadRepositoryFile("src/Maui/Component/MobileHeader/MobileHeaderResource.resx");
        string korean = ReadRepositoryFile("src/Maui/Component/MobileHeader/MobileHeaderResource.ko-KR.resx");

        // When
        bool hasResourceKey = resource.Contains("EncryptionLabel", StringComparison.Ordinal);

        // Then
        hasResourceKey.Should().BeTrue();
        defaultCulture.Should().Contain("AES-256-GCM");
        korean.Should().Contain("AES-256-GCM");
    }

    private static string ReadRepositoryFile(string relativePath)
    {
        return File.ReadAllText(Path.Combine(FindRepositoryRoot(), relativePath));
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Orchid.Mobile.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Orchid.Mobile.slnx not found.");
    }
}
