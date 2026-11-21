using Garden.Share;

namespace Orchid.Mobile.Service;

/// <summary>
/// Garden.Maui의 <see cref="IResourceOwnerAuth"/>에 위임해 모바일 Keycloak OIDC 로그인 상태를 노출합니다.
/// </summary>
public sealed class OrchidMobileAuthService(IResourceOwnerAuth resourceOwnerAuth) : IOrchidMobileAuthService
{
    private const string KakaoProvider = "kakao";

    /// <inheritdoc />
    public bool IsAuthenticated => resourceOwnerAuth.ClaimsPrincipal.Identity?.IsAuthenticated is true;

    /// <inheritdoc />
    public string CurrentUserName => resourceOwnerAuth.GetProfile().Name;

    /// <inheritdoc />
    public string CurrentUserEmail => resourceOwnerAuth.GetProfile().Email;

    /// <inheritdoc />
    public string CurrentUserInitial
    {
        get
        {
            string avatar = resourceOwnerAuth.GetProfile().Avatar;
            return string.IsNullOrWhiteSpace(avatar) ? "O" : avatar;
        }
    }

    /// <inheritdoc />
    public async Task SignInWithProviderAsync(string provider, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        cancellationToken.ThrowIfCancellationRequested();

        string identityProvider = ResolveIdentityProvider(provider);

        // ✅ 실제 Keycloak 로그인에 위임한다. 취소되거나 완료되지 않은 로그인은 익명 주체로 돌아와 IsAuthenticated가 false로 남는다.
        await resourceOwnerAuth
            .LoginAsync(identityProvider)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> RestoreSessionAsync()
    {
        return await resourceOwnerAuth
            .IsLoggedInAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await resourceOwnerAuth
            .LogoutAsync()
            .ConfigureAwait(false);
    }

    private static string ResolveIdentityProvider(string provider)
    {
        string normalized = provider.Trim().ToLowerInvariant();

        return normalized switch
        {
            IdentityProvider.Google => IdentityProvider.Google,
            IdentityProvider.Microsoft => IdentityProvider.Microsoft,
            KakaoProvider => KakaoProvider,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "지원하지 않는 로그인 공급자입니다."),
        };
    }
}
