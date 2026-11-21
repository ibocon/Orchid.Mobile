using System.Reflection;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.Messaging;
using Garden.Maui;
using Garden.Razor;
using Garden.Share;
using MudBlazor.Services;
using Orchid.Core.Abstraction;
using Orchid.Core.Cryptography;
using Orchid.Core.Login;
using Orchid.Core.Storage;
using Orchid.Core.Sync;
using Orchid.Core.Sync.Relay;
using Orchid.Mobile.Service;
using ZXing.Net.Maui.Controls;

namespace Orchid.Mobile;

/// <summary>
/// .NET MAUI 모바일 애플리케이션 진입점 및 의존성 주입을 구성합니다.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// 모바일 MAUI 앱 호스트를 빌드하고 초기화합니다.
    /// </summary>
    /// <returns>구성된 <see cref="MauiApp"/> 인스턴스</returns>
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseBarcodeReader()
            .ConfigureAppSettings() // ✅ Identity 섹션을 먼저 구성에 올린다
            .ConfigureGardenMaui();

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        builder.Services.AddMediator();
        builder.Services.AddMudServices();
        builder.Services.AddGardenRazor();
        builder.Services.AddDesignSystem();
        builder.Services.AddLocalization();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        builder.Services.AddSingleton<IAesGcmCryptoService, AesGcmCryptoService>();
        // ✅ BlazorWebView 스코프에서 IResourceOwnerAuth를 받도록 스코프로 등록한다
        builder.Services.AddScoped<IOrchidMobileAuthService, OrchidMobileAuthService>();
        builder.Services.AddScoped<OrchidLoginOutcomeHandler>();
        builder.Services.AddScoped<ILoginOutcomeHandler>(
            provider => provider.GetRequiredService<OrchidLoginOutcomeHandler>());

        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "orchid_mobile.db");
        builder.Services.AddSingleton<IClipboardRepository>(_ => new SqliteClipboardRepository(dbPath));
        builder.Services.AddSingleton<ISyncEngine, SyncEngine>();

#if ANDROID
        builder.Services.AddSingleton<ISystemClipboard>(_ => new Orchid.Mobile.Platforms.Android.AndroidClipboardAdapter(Android.App.Application.Context));
        builder.Services.AddSingleton<IClipboardChangeSource>(_ => new Orchid.Mobile.Platforms.Android.AndroidClipboardChangeSource(Android.App.Application.Context));
#else
        builder.Services.AddSingleton<IClipboardChangeSource, NullClipboardChangeSource>();
#endif
        string relayUrl = typeof(MauiProgram).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "OrchidRelayUrl").Value!;
        builder.Services.AddSingleton(new RelayOptions(new Uri(new Uri(relayUrl.TrimEnd('/') + "/"), "hubs/clipboard")));
        builder.Services.AddSingleton<IClipboardRelayClient, SignalRClipboardRelayClient>();
        builder.Services.AddSingleton<ClipboardSyncCoordinator>();
        builder.Services.AddSingleton<MobileRelayConnectionState>();
        builder.Services.AddSingleton<IQrCodeImageEncoder, QrCodeImageEncoder>();
#if ANDROID
        builder.Services.AddSingleton<IQrPairingScanner, Platforms.Android.QrPairingScannerService>();
#else
        builder.Services.AddSingleton<IQrPairingScanner, UnsupportedQrPairingScanner>();
#endif

        return builder.Build();
    }
}
