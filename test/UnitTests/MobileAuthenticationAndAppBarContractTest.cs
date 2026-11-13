// keep: FluentAssertions assertions
using FluentAssertions;
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
    public async Task SignInAndSignOutShouldTransitionMobileAuthenticationState()
    {
        // Given
        OrchidMobileAuthService authService = new();
        authService.IsAuthenticated.Should().BeFalse();

        // When
        await authService.SignInWithProviderAsync("kakao");

        // Then
        // ✅ 로그인 후 인증 상태와 프로필이 활성화되고 로그아웃 시 해제된다
        authService.IsAuthenticated.Should().BeTrue();
        authService.CurrentUserName.Should().NotBeNullOrWhiteSpace();
        authService.BuildAuthorizationUrl("kakao").AbsoluteUri.Should().Contain("orchid%3A%2F%2Fopenid-connect%2Fcallback");

        await authService.SignOutAsync();
        authService.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void MobileFilesShouldDeclareScopedCssCallbackActivityAndGardenLoginComponents()
    {
        // Given
        string repoRoot = FindRepositoryRoot();
        string indexHtml = File.ReadAllText(Path.Combine(repoRoot, "src/wwwroot/index.html"));
        string callbackActivity = File.ReadAllText(Path.Combine(repoRoot, "src/Platforms/Android/WebAuthenticationCallbackActivity.cs"));
        string mobileHeader = File.ReadAllText(Path.Combine(repoRoot, "src/Component/MobileHeader/MobileHeader.razor"));
        string loginPanel = File.ReadAllText(Path.Combine(repoRoot, "src/Component/LoginPanel/LoginPanel.razor"));

        // When & Then
        // ✅ Scoped CSS 번들, WebAuthenticatorCallbackActivity, GardenAccountMenu, 로그인 버튼 3종이 모두 포함된다
        indexHtml.Should().Contain("Orchid.Mobile.styles.css");
        callbackActivity.Should().Contain("WebAuthenticatorCallbackActivity");
        callbackActivity.Should().Contain("orchid");
        mobileHeader.Should().Contain("<GardenTopAppBar");
        mobileHeader.Should().Contain("<GardenAccountMenu");
        loginPanel.Should().Contain("<GoogleLoginButton");
        loginPanel.Should().Contain("<KakaoLoginButton");
        loginPanel.Should().Contain("<MicrosoftLoginButton");
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
