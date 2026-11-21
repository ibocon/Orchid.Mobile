// reference: Garden.Share 10.0.0 / IResourceOwnerAuth, AuthenticationProfile in Garden.Share
using System.Security.Claims;
using Garden.Share;

namespace Orchid.UnitTests.Mobile;

/// <summary>
/// 로그인 결과와 프로필을 테스트가 지정하는 <see cref="IResourceOwnerAuth"/> Stub입니다.
/// </summary>
internal sealed class FakeResourceOwnerAuth : IResourceOwnerAuth
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public ClaimsPrincipal ClaimsPrincipal { get; private set; } = Anonymous;

    public AuthenticationProfile Profile { get; set; } = AuthenticationProfile.Anonymous;

    public ClaimsPrincipal? LoginResult { get; set; }

    public string? LastIdentityProvider { get; private set; }

    public Task<bool> IsLoggedInAsync()
    {
        return Task.FromResult(ClaimsPrincipal.Identity?.IsAuthenticated is true);
    }

    public Task<ClaimsPrincipal> LoginAsync(string identityProvider)
    {
        LastIdentityProvider = identityProvider;
        ClaimsPrincipal = LoginResult ?? Anonymous;

        return Task.FromResult(ClaimsPrincipal);
    }

    public Task<bool> TryRefreshAsync()
    {
        return Task.FromResult(true);
    }

    public Task LogoutAsync()
    {
        ClaimsPrincipal = Anonymous;
        Profile = AuthenticationProfile.Anonymous;

        return Task.CompletedTask;
    }

    public AuthenticationProfile GetProfile()
    {
        return ClaimsPrincipal.Identity?.IsAuthenticated is true ? Profile : AuthenticationProfile.Anonymous;
    }

    public static ClaimsPrincipal CreateAuthenticated()
    {
        return new ClaimsPrincipal(new ClaimsIdentity("test"));
    }
}
