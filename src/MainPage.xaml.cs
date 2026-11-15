using Garden.Maui;
using Microsoft.Extensions.Logging;

namespace Orchid.Mobile;

/// <summary>
/// Peony.Mobile의 BasePage 규약을 따라 BlazorWebView와 시스템 바 인셋을 호스팅하는 모바일 메인 페이지입니다.
/// </summary>
public partial class MainPage : BasePage<MainViewModel>
{
    /// <summary>
    /// <see cref="MainPage"/> 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="logger">페이지 로거</param>
    /// <param name="viewModel">메인 페이지 뷰모델</param>
    public MainPage(
        ILogger<MainPage> logger,
        MainViewModel viewModel)
        : base(logger, viewModel)
    {
        InitializeComponent();
    }
}
