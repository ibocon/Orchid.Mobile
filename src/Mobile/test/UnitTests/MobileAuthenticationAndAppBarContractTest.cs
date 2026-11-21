using System.Text.Json;
// keep: FluentAssertions assertions
using FluentAssertions;
// keep: Garden.Razor design system
using Garden.Razor;
// keep: Microsoft.Extensions.DependencyInjection
using Microsoft.Extensions.DependencyInjection;
// keep: Orchid.Mobile service namespace
using Orchid.Mobile.Service;
// keep: xUnit Fact attribute
using Xunit;

namespace Orchid.UnitTests.Mobile;

/// <summary>
/// Orchid.Mobile의 Keycloak OIDC 로그인 서비스, Android 콜백 Activity 및 GardenTopAppBar/GardenAccountMenu 계약을 검증합니다.
/// </summary>
public sealed class MobileAuthenticationAndAppBarContractTest
{
    [Fact]
    public void DesignSystemSchemeSetShouldSwitchBetweenLightAndDarkContexts()
    {
        // Given
        ServiceCollection services = new();
        services.AddLogging();
        services.AddDesignSystem();
        using ServiceProvider provider = services.BuildServiceProvider();
        DesignSystem designSystem = provider.GetRequiredService<DesignSystem>();
        List<SchemeContext> raisedContexts = [];
        designSystem.Color.SchemeSet.ContextChanged += (_, eventArgs) => raisedContexts.Add(eventArgs.SchemeContext);

        // When
        SchemeContext initial = designSystem.Color.SchemeSet.Context;
        string lightSurface = designSystem.Color.SchemeSet.Current.Surface.ToHex();

        designSystem.Color.SchemeSet.ChangeContext(SchemeContext.Dark);
        SchemeContext toggled = designSystem.Color.SchemeSet.Context;
        string darkSurface = designSystem.Color.SchemeSet.Current.Surface.ToHex();

        designSystem.Color.SchemeSet.ChangeContext(SchemeContext.Light);
        SchemeContext restored = designSystem.Color.SchemeSet.Context;
        string restoredSurface = designSystem.Color.SchemeSet.Current.Surface.ToHex();

        // Then
        // ✅ 초기 문맥이 Light이고 Dark 전환 시 Surface 색상이 달라지며 ContextChanged가 발생한다
        initial.Should().Be(SchemeContext.Light);
        toggled.Should().Be(SchemeContext.Dark);
        lightSurface.Should().NotBe(darkSurface);
        // ✅ Light 복귀 시 문맥과 Surface 색상이 원복된다
        restored.Should().Be(SchemeContext.Light);
        restoredSurface.Should().Be(lightSurface);
        raisedContexts.Should().Equal(SchemeContext.Dark, SchemeContext.Light);
    }

    [Fact]
    public void IdentitySettingsShouldMatchAndroidCallbackScheme()
    {
        // Given
        string repoRoot = FindRepositoryRoot();
        string settings = File.ReadAllText(Path.Combine(repoRoot, "src/Mobile/src/Resources/appsettings.json"));
        string callbackActivity = File.ReadAllText(Path.Combine(repoRoot, "src/Mobile/src/Platforms/Android/WebAuthenticationCallbackActivity.cs"));

        // When
        using JsonDocument document = JsonDocument.Parse(settings);
        JsonElement identity = document.RootElement.GetProperty("Identity");

        // Then
        // ✅ 클라이언트 ID와 리다이렉트 URL이 Android 콜백 Activity의 scheme·host·path와 같다
        identity.GetProperty("ClientId").GetString().Should().Be("orchid-mobile");
        identity.GetProperty("RedirectUrl").GetString().Should().Be("orchid://openid-connect/callback");
        callbackActivity.Should().Contain("\"orchid\"").And.Contain("\"openid-connect\"").And.Contain("\"/callback\"");
    }

    [Fact]
    public void MobileFilesShouldDeclareScopedCssCallbackActivityAndGardenLoginComponents()
    {
        // Given
        string repoRoot = FindRepositoryRoot();
        string indexHtml = File.ReadAllText(Path.Combine(repoRoot, "src/Mobile/src/wwwroot/index.html"));
        string callbackActivity = File.ReadAllText(Path.Combine(repoRoot, "src/Mobile/src/Platforms/Android/WebAuthenticationCallbackActivity.cs"));
        string mobileHeader = File.ReadAllText(Path.Combine(repoRoot, "src/Mobile/src/Component/MobileHeader/MobileHeader.razor"));
        string loginPanel = File.ReadAllText(Path.Combine(repoRoot, "src/Mobile/src/Component/LoginPanel/LoginPanel.razor"));

        // When & Then
        // ✅ Scoped CSS 번들, WebAuthenticatorCallbackActivity, GardenAccountMenu, 공통 로그인 화면이 모두 포함된다
        indexHtml.Should().Contain("Orchid.Mobile.styles.css");
        callbackActivity.Should().Contain("WebAuthenticatorCallbackActivity");
        callbackActivity.Should().Contain("orchid");
        mobileHeader.Should().Contain("<GardenTopAppBar");
        mobileHeader.Should().Contain("<GardenAccountMenu");
        loginPanel.Should().Contain("<LoginScreen Branding=\"_branding\">");
    }

    [Fact]
    public void LoginPanelShouldHostSharedLoginScreenWithOrchidBranding()
    {
        // Given
        string repoRoot = FindRepositoryRoot();
        string logoPath = Path.Combine(repoRoot, "src/Mobile/src/wwwroot/logo-login.svg");
        string loginPanel = File.ReadAllText(Path.Combine(repoRoot, "src/Mobile/src/Component/LoginPanel/LoginPanel.razor"));

        // When & Then
        // ✅ 브랜드 로고 SVG가 존재하고 유효한 svg 마크업을 포함한다
        File.Exists(logoPath).Should().BeTrue();
        File.ReadAllText(logoPath).Should().Contain("<svg");

        loginPanel.Should().Contain("OrchidLoginBranding.Create()");
        loginPanel.Should().Contain("<GardenLoadingIndicator />");
        loginPanel.Should().Contain("OnAuthenticated");
    }

    [Fact]
    public void MauiProgramShouldRegisterMediatorForResourceOwnerAuth()
    {
        // Given
        string repoRoot = FindRepositoryRoot();
        string mauiProgram = File.ReadAllText(Path.Combine(repoRoot, "src/Mobile/src/MauiProgram.cs"));

        // When & Then
        // ✅ ResourceOwnerAuth가 요구하는 IMediator를 composition root에서 등록한다
        mauiProgram.Should().Contain("builder.Services.AddMediator();");
    }

    [Fact]
    public void MobileCanvasAndLoginMarkShouldKeepTheBrandHierarchy()
    {
        // Given
        string repoRoot = FindRepositoryRoot();
        string appCss = File.ReadAllText(Path.Combine(repoRoot, "src/Mobile/src/wwwroot/css/app.css"));
        string logo = File.ReadAllText(Path.Combine(repoRoot, "src/Mobile/src/wwwroot/logo-login.svg"));

        // When & Then
        appCss.Should().Contain("--orchid-mobile-canvas-start");
        appCss.Should().Contain("--orchid-mobile-canvas-end");
        appCss.Should().Contain("linear-gradient(160deg");
        logo.Should().Contain("id=\"petal-top\"");
        logo.Should().Contain("id=\"petal-left\"");
        logo.Should().Contain("id=\"petal-right\"");
        logo.Should().Contain("id=\"orchid-core\"");
        logo.Should().Contain("id=\"orchid-throat\"");
        logo.Should().Contain("#A8796A");
        logo.Should().NotContain("id=\"stem\"");
        logo.Should().NotContain("vein");
    }

    [Fact]
    public void MobileThemeProviderAndAppShellShouldFollowLightDefaultThemeContract()
    {
        // Given
        string repoRoot = FindRepositoryRoot();
        string provider = File.ReadAllText(Path.Combine(repoRoot, "src/Mobile/src/Component/OrchidMobileDesignSystemProvider/OrchidMobileDesignSystemProvider.razor.cs"));
        string mobileApp = File.ReadAllText(Path.Combine(repoRoot, "src/Mobile/src/Component/MobileApp/MobileApp.razor"));
        string mainPageXaml = File.ReadAllText(Path.Combine(repoRoot, "src/Mobile/src/MainPage.xaml"));
        string appCss = File.ReadAllText(Path.Combine(repoRoot, "src/Mobile/src/wwwroot/css/app.css"));

        // When & Then
        // ✅ 프로바이더가 Light 기본 컨텍스트, PaletteLight/PaletteDark 양방향 매핑, 테마 전환 API를 노출한다
        provider.Should().Contain("ChangeContext(SchemeContext.Light)");
        provider.Should().Contain("PaletteLight");
        provider.Should().Contain("PaletteDark");
        provider.Should().Contain("IsDarkMode");
        provider.Should().Contain("CurrentSchemeContext");
        provider.Should().Contain("ToggleTheme()");
        provider.Should().Contain("SetTheme(SchemeContext");

        // ✅ 루트 앱셸이 다크 고정 리터럴 없이 프로바이더에 바인딩하고 MobileHeader에 테마 상태를 전달한다
        mobileApp.Should().NotContain("IsDarkMode=\"true\"");
        mobileApp.Should().Contain("IsDarkMode=\"@(_provider?.IsDarkMode ?? false)\"");
        mobileApp.Should().Contain("OnToggleTheme=\"ToggleTheme\"");

        // ✅ 네이티브 셸과 앱 CSS에 다크 하드코딩이 없고 디자인 토큰을 사용한다
        mainPageXaml.Should().NotContain("#121212").And.NotContain("#1e1e1e");
        appCss.Should().NotContain("#121212").And.NotContain("#1e1e1e");
        appCss.Should().Contain("var(--gdn-sys-color-on-surface)");
        appCss.Should().Contain("var(--gdn-sys-color-surface-container)");
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
