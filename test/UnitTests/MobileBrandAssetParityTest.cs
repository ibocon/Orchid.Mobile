using FluentAssertions;
using Xunit;

namespace Orchid.UnitTests.Mobile;

/// <summary>
/// Mobile 앱 아이콘 SVG가 형제 저장소 Orchid.Desktop의 단일 원본과 바이트 단위로 같은지 검증하는 단위 테스트입니다.
/// 형제 저장소가 없는 환경(저장소를 따로 체크아웃하는 CI)에서는 검증할 대상이 없으므로 통과로 처리합니다.
/// </summary>
public sealed class MobileBrandAssetParityTest
{
    [Theory]
    [InlineData("src/Resources/AppIcon/appicon.svg")]
    [InlineData("src/Resources/AppIcon/appiconfg.svg")]
    public void BrandSvgWhenSiblingDesktopExistsShouldMatchDesktopSource(string relativePath)
    {
        // Given
        string mobileRoot = FindMobileRoot();
        string? desktopRoot = ResolveDesktopRoot(mobileRoot);
        string desktopFile = desktopRoot is null ? string.Empty : Path.Combine(desktopRoot, relativePath);
        if (desktopRoot is null || !File.Exists(desktopFile))
        {
            return;
        }

        // When
        byte[] mobileBytes = File.ReadAllBytes(Path.Combine(mobileRoot, "src", "Maui", relativePath["src/".Length..]));
        byte[] desktopBytes = File.ReadAllBytes(desktopFile);

        // Then
        mobileBytes.Should().Equal(
            desktopBytes,
            "Desktop의 New-OrchidBrandVisualAssets.ps1을 다시 실행해 {0}을(를) 동기화해야 합니다.",
            relativePath);
    }

    private static string? ResolveDesktopRoot(string mobileRoot)
    {
        string? explicitRoot = Environment.GetEnvironmentVariable("ORCHID_DESKTOP_REPO_ROOT");
        string? candidate = string.IsNullOrWhiteSpace(explicitRoot)
            ? Path.Combine(Path.GetDirectoryName(mobileRoot) ?? string.Empty, "Orchid.Desktop")
            : explicitRoot;
        return Directory.Exists(candidate) ? candidate : null;
    }

    private static string FindMobileRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Orchid.Mobile.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Orchid.Mobile.slnx not found.");
    }
}
