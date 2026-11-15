using System.Globalization;

namespace Orchid.Mobile;

/// <summary>
/// .NET MAUI 모바일 애플리케이션 진입 클래스입니다.
/// </summary>
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// <see cref="App"/> 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="serviceProvider">DI 서비스 공급자</param>
    public App(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _serviceProvider = serviceProvider;

        CultureInfo koreanCulture = new("ko-KR");
        CultureInfo.DefaultThreadCurrentCulture = koreanCulture;
        CultureInfo.DefaultThreadCurrentUICulture = koreanCulture;
        CultureInfo.CurrentCulture = koreanCulture;
        CultureInfo.CurrentUICulture = koreanCulture;

        InitializeComponent();
        UserAppTheme = AppTheme.Light;
    }

    /// <inheritdoc />
    protected override Window CreateWindow(IActivationState? activationState)
    {
        MainPage mainPage = _serviceProvider.GetRequiredService<MainPage>();
        return new Window(mainPage);
    }
}
