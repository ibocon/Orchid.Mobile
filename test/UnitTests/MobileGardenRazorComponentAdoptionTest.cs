// keep: FluentAssertions assertions
using FluentAssertions;
// keep: xUnit Fact attribute
using Xunit;

namespace Orchid.UnitTests.Mobile;

/// <summary>
/// Orchid.Mobile의 모든 Razor 컴포넌트가 원시 MudBlazor 태그 대신 Garden.Razor 컴포넌트를 사용하는지 검증합니다.
/// </summary>
public sealed class MobileGardenRazorComponentAdoptionTest
{
    [Fact]
    public void MobileRazorComponentsShouldUseGardenRazorComponentsInsteadOfRawMudUiTags()
    {
        // Given
        string repoRoot = FindRepositoryRoot();
        string mobileApp = File.ReadAllText(Path.Combine(repoRoot, "src/Component/MobileApp/MobileApp.razor"));
        string mobileHeader = File.ReadAllText(Path.Combine(repoRoot, "src/Component/MobileHeader/MobileHeader.razor"));
        string qrScanner = File.ReadAllText(Path.Combine(repoRoot, "src/Component/QrPairingScanner/QrPairingScanner.razor"));
        string swipeableItem = File.ReadAllText(Path.Combine(repoRoot, "src/Component/SwipeableItem/SwipeableItem.razor"));

        // When & Then
        // ✅ 모든 모바일 UI 컴포넌트가 Garden.Razor 컴포넌트로 구성되고 원시 Mud* UI 태그를 사용하지 않는다
        mobileApp.Should().Contain("<GardenSnackbarProvider");
        mobileApp.Should().NotContain("<MudSnackbarProvider");

        mobileHeader.Should().Contain("<GardenTopAppBar");
        mobileHeader.Should().Contain("<GardenTypography");
        mobileHeader.Should().Contain("<GardenChipAssist");
        mobileHeader.Should().NotContain("<MudAppBar");
        mobileHeader.Should().NotContain("<MudText");
        mobileHeader.Should().NotContain("<MudChip");

        qrScanner.Should().Contain("<GardenCard");
        qrScanner.Should().Contain("<GardenTypography");
        qrScanner.Should().Contain("<GardenTextField");
        qrScanner.Should().Contain("<GardenButton");
        qrScanner.Should().NotContain("<MudPaper");
        qrScanner.Should().NotContain("<MudText");
        qrScanner.Should().NotContain("<MudTextField");
        qrScanner.Should().NotContain("<MudButton");

        swipeableItem.Should().Contain("<GardenCard");
        swipeableItem.Should().Contain("<GardenChipAssist");
        swipeableItem.Should().Contain("<GardenIconButton");
        swipeableItem.Should().Contain("<GardenTypography");
        swipeableItem.Should().NotContain("<MudCard");
        swipeableItem.Should().NotContain("<MudChip");
        swipeableItem.Should().NotContain("<MudIconButton");
        swipeableItem.Should().NotContain("<MudText");
    }

    [Fact]
    public void LoginPanelShouldUseGardenLoginComponentsWithoutCardBoxOrRawMudTags()
    {
        // Given
        string repoRoot = FindRepositoryRoot();
        string loginPanel = File.ReadAllText(Path.Combine(repoRoot, "src/Component/LoginPanel/LoginPanel.razor"));

        // When & Then
        // ✅ 로그인 패널이 Garden.Razor 로그인 버튼 3종으로 구성된다
        loginPanel.Should().Contain("<GoogleLoginButton");
        loginPanel.Should().Contain("<KakaoLoginButton");
        loginPanel.Should().Contain("<MicrosoftLoginButton");

        // ✅ 기존 카드 박스 로그인 UI와 원시 Mud* 태그를 사용하지 않는다
        loginPanel.Should().NotContain("<GardenCard");
        loginPanel.Should().NotContain("LockPerson");
        loginPanel.Should().NotContain("<MudIcon");
        loginPanel.Should().NotContain("<MudPaper");
        loginPanel.Should().NotContain("<MudCard");
        loginPanel.Should().NotContain("<MudButton");
        loginPanel.Should().NotContain("<MudText");
    }

    [Fact]
    public void MobileHeaderTrailingContentShouldExposeGardenIconButtonThemeToggleWithoutRawMudTags()
    {
        // Given
        string repoRoot = FindRepositoryRoot();
        string mobileHeader = File.ReadAllText(Path.Combine(repoRoot, "src/Component/MobileHeader/MobileHeader.razor"));

        // When
        int trailingStart = mobileHeader.IndexOf("<TrailingContent>", StringComparison.Ordinal);
        int trailingEnd = mobileHeader.IndexOf("</TrailingContent>", StringComparison.Ordinal);

        // Then
        // ✅ TrailingContent 안에 GardenIconButton 기반 Light/Dark 전환 아이콘이 위치하고 원시 MudIconButton을 사용하지 않는다
        trailingStart.Should().BeGreaterThanOrEqualTo(0);
        trailingEnd.Should().BeGreaterThan(trailingStart);
        string trailingContent = mobileHeader[trailingStart..trailingEnd];
        trailingContent.Should().Contain("<GardenIconButton");
        trailingContent.Should().Contain("Icons.Material.Filled.LightMode");
        trailingContent.Should().Contain("Icons.Material.Filled.DarkMode");
        mobileHeader.Should().NotContain("<MudIconButton");
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
