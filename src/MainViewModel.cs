using CommunityToolkit.Mvvm.ComponentModel;
using Garden.Maui;
using Garden.Razor;
using Garden.Share;
using Microsoft.Extensions.Logging;
using MauiColor = Microsoft.Maui.Graphics.Color;

namespace Orchid.Mobile;

/// <summary>
/// Peony.Mobile의 MainViewModel 규약을 따라 모바일 시스템 상태 표시줄 및 네비게이션 바 테마를 동기화하는 뷰모델입니다.
/// </summary>
public sealed partial class MainViewModel : ViewModelBase, IDisposable
{
    private readonly DesignSystem _designSystem;
    private bool _disposed;

    [ObservableProperty]
    private MauiColor _statusBarColor = null!;

    [ObservableProperty]
    private StatusBarStyle _statusBarStyle;

    [ObservableProperty]
    private MauiColor _navigationBarColor = null!;

    [ObservableProperty]
    private Garden.Maui.NavigationBarStyle _navigationBarStyle;

    /// <summary>
    /// <see cref="MainViewModel"/> 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="logger">로거 인스턴스</param>
    /// <param name="designSystem">Garden 디자인 시스템 인스턴스</param>
    public MainViewModel(
        ILogger<MainViewModel> logger,
        DesignSystem designSystem)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(designSystem);

        _designSystem = designSystem;
        _designSystem.Color.SchemeSet.ContextChanged += OnSchemeContextChanged;
        UpdateSystemBarTheme();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _designSystem.Color.SchemeSet.ContextChanged -= OnSchemeContextChanged;
        _disposed = true;
    }

    private void OnSchemeContextChanged(object? sender, SchemeContextChangedEventArgs e)
    {
        UpdateSystemBarTheme();
    }

    private void UpdateSystemBarTheme()
    {
        string surfaceHex = _designSystem.Color.SchemeSet.Current.SurfaceContainer.ToHex();
        StatusBarColor = MauiColor.FromArgb(surfaceHex);
        NavigationBarColor = MauiColor.FromArgb(surfaceHex);

        if (_designSystem.Color.SchemeSet.Context == SchemeContext.Dark)
        {
            StatusBarStyle = StatusBarStyle.LightContent;
            NavigationBarStyle = Garden.Maui.NavigationBarStyle.LightContent;
        }
        else
        {
            StatusBarStyle = StatusBarStyle.DarkContent;
            NavigationBarStyle = Garden.Maui.NavigationBarStyle.DarkContent;
        }
    }
}
