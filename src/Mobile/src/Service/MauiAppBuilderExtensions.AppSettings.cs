using Microsoft.Extensions.Configuration;

namespace Orchid.Mobile.Service;

/// <summary>
/// 임베드된 appsettings 리소스를 MAUI 구성에 추가합니다.
/// </summary>
public static class MauiAppBuilderExtensions
{
    private const string AppSettingsResourcePath = "Orchid.Mobile.Resources.appsettings.json";

    /// <summary>
    /// 임베드된 <c>appsettings.json</c>을 <see cref="MauiAppBuilder.Configuration"/>에 추가합니다.
    /// </summary>
    /// <param name="builder">MAUI 앱 빌더</param>
    /// <returns>체이닝 가능한 MAUI 앱 빌더</returns>
    public static MauiAppBuilder ConfigureAppSettings(this MauiAppBuilder builder)
    {
        using Stream resourceStream = typeof(MauiAppBuilderExtensions).Assembly.GetManifestResourceStream(AppSettingsResourcePath)
            ?? throw new InvalidOperationException($"Embedded resource '{AppSettingsResourcePath}'을 읽지 못했습니다.");

        builder.Configuration.AddJsonStream(resourceStream); // ✅ Identity 섹션을 구성에 추가한다

        return builder;
    }
}
