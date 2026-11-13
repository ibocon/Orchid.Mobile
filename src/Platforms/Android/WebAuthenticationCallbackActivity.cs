using Android.App;
using Android.Content.PM;

namespace Orchid.Mobile.Platforms.Android;

/// <summary>
/// OAuth 및 Keycloak OIDC 인증 콜백을 수신하는 Android Activity입니다.
/// 커스텀 스키마(orchid://openid-connect/callback)를 수신합니다.
/// </summary>
[Activity(
    NoHistory = true,
    LaunchMode = LaunchMode.SingleTop,
    Exported = true)]
[IntentFilter(
    [global::Android.Content.Intent.ActionView],
    Categories = [global::Android.Content.Intent.CategoryDefault, global::Android.Content.Intent.CategoryBrowsable],
    DataScheme = CallbackScheme,
    DataHost = CallbackHost,
    DataPathPrefix = CallbackPathPrefix)]
public class WebAuthenticationCallbackActivity
    : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
{
    private const string CallbackScheme = "orchid";
    private const string CallbackHost = "openid-connect";
    private const string CallbackPathPrefix = "/callback";
}
