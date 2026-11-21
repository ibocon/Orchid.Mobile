// keep: FluentAssertions assertions
using FluentAssertions;
// keep: xUnit Fact attribute
using Xunit;

namespace Orchid.UnitTests.Mobile;

/// <summary>
/// Peony.Mobile의 SafeArea 및 WindowInsets 아키텍처(IWindowInsets 구독, --root-shell-padding-top, --root-bottom-inset, 100svh, BasePage, 헤더 현지화) 계약을 검증합니다.
/// </summary>
public sealed class MobileSafeAreaAndWindowInsetContractTest
{
    [Fact]
    public void GivenComponentsReadingWindowInsetsWhenScannedThenEachSubscribesToChanged()
    {
        // Given
        string repoRoot = FindRepositoryRoot();
        string mobileApp = File.ReadAllText(Path.Combine(repoRoot, "src/Maui/Component/MobileApp/MobileApp.razor"));

        // When & Then
        // ✅ Insets.TopPixel 및 Insets.BottomPixel을 읽는 루트 셸 컴포넌트는 반드시 Insets.Changed를 구독 및 해제해야 한다
        mobileApp.Should().Contain("Insets.TopPixel");
        mobileApp.Should().Contain("Insets.BottomPixel");
        mobileApp.Should().Contain("Insets.Changed +=");
        mobileApp.Should().Contain("Insets.Changed -=");
    }

    [Fact]
    public void GivenGlobalAndShellStylesWhenInspectedThenDeclarePeonyMobileSafeAreaVariables()
    {
        // Given
        string repoRoot = FindRepositoryRoot();
        string indexHtml = File.ReadAllText(Path.Combine(repoRoot, "src/Maui/wwwroot/index.html"));
        string appCss = File.ReadAllText(Path.Combine(repoRoot, "src/Maui/wwwroot/css/app.css"));
        string shellCss = File.ReadAllText(Path.Combine(repoRoot, "src/Maui/Component/MobileApp/MobileApp.razor.css"));

        // When & Then
        // ✅ Peony.Mobile과 동일하게 status-bar-safe-area, --gdn-mobile-layout-content-height: 100svh, --root-shell-padding-top, --root-bottom-inset을 선언한다
        indexHtml.Should().Contain("status-bar-safe-area");
        appCss.Should().Contain("--gdn-mobile-layout-content-height: 100svh;");
        appCss.Should().Contain("var(--root-bottom-inset, 0px)");
        shellCss.Should().Contain("padding-top: var(--root-shell-padding-top, 0px);");
        shellCss.Should().Contain("padding-bottom: var(--root-bottom-inset, 0px);");
    }

    [Fact]
    public void GivenMobileHostAndHeaderWhenInspectedThenFollowPeonyMobileBasePageAndLocalizedTitleContract()
    {
        // Given
        string repoRoot = FindRepositoryRoot();
        string mauiProgram = File.ReadAllText(Path.Combine(repoRoot, "src/Maui/MauiProgram.cs"));
        string mainPageXaml = File.ReadAllText(Path.Combine(repoRoot, "src/Maui/MainPage.xaml"));
        string stylesXml = File.ReadAllText(Path.Combine(repoRoot, "src/Maui/Platforms/Android/Resources/values/styles.xml"));
        string mobileHeader = File.ReadAllText(Path.Combine(repoRoot, "src/Maui/Component/MobileHeader/MobileHeader.razor"));
        string mobileHeaderKoResx = File.ReadAllText(Path.Combine(repoRoot, "src/Maui/Component/MobileHeader/MobileHeaderResource.ko-KR.resx"));

        // When & Then
        // ✅ ConfigureGardenMaui, garden:BasePage, AppTheme, 그리고 MobileHeader의 모두클립 현지화 리소스가 구성된다
        mauiProgram.Should().Contain("ConfigureGardenMaui()");
        mainPageXaml.Should().Contain("garden:BasePage");
        stylesXml.Should().Contain("name=\"AppTheme\"");
        stylesXml.Should().Contain("android:windowDrawsSystemBarBackgrounds");
        mobileHeader.Should().Contain("IStringLocalizer<MobileHeaderResource>");
        mobileHeaderKoResx.Should().Contain("모두클립");
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
