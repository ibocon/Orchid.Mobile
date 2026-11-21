using FluentAssertions;
using Xunit;

namespace Orchid.UnitTests.Mobile;

/// <summary>
/// Orchid.Mobile 로그인 화면의 공통 화면 배선 계약을 검증합니다.
/// </summary>
public sealed class MobileLoginWiringContractTest
{
    /// <summary>
    /// 로그인 결과 핸들러가 BlazorWebView 스코프와 공통 계약에 등록되는지 검증합니다.
    /// </summary>
    [Fact]
    public void MauiProgramRegistersLoginOutcomeHandlerInBlazorWebViewScope()
    {
        // Given
        string program = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "src/Maui/MauiProgram.cs"));

        // When
        bool registersHandler = program.Contains(
            "builder.Services.AddScoped<OrchidLoginOutcomeHandler>();",
            StringComparison.Ordinal);
        bool forwardsContract = program.Contains(
            "builder.Services.AddScoped<ILoginOutcomeHandler>(",
            StringComparison.Ordinal);
        bool registersMessengerNamespace = program.Contains(
            "using CommunityToolkit.Mvvm.Messaging;",
            StringComparison.Ordinal);
        bool registersMessenger = program.Contains(
            "builder.Services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);",
            StringComparison.Ordinal);

        // Then
        registersHandler.Should().BeTrue();
        forwardsContract.Should().BeTrue("공통 LoginViewModel은 ILoginOutcomeHandler 계약으로 핸들러를 주입받는다");
        registersMessengerNamespace.Should().BeTrue();
        registersMessenger.Should().BeTrue("인증 상태 이벤트 소비자는 IMessenger를 생성자 의존성으로 요구한다");
    }

    /// <summary>
    /// 로그인 패널이 공통 화면을 호스팅하고 인증 완료를 부모에게 전달하는지 검증합니다.
    /// </summary>
    [Fact]
    public void LoginPanelHostsSharedScreenAndForwardsAuthenticationToParent()
    {
        // Given
        string root = FindRepositoryRoot();
        string loginPanel = File.ReadAllText(Path.Combine(root, "src/Maui/Component/LoginPanel/LoginPanel.razor"));
        string mobileApp = File.ReadAllText(Path.Combine(root, "src/Maui/Component/MobileApp/MobileApp.razor"));

        // When & Then
        loginPanel.Should().Contain("<LoginScreen Branding=\"_branding\">");
        loginPanel.Should().Contain("OrchidLoginBranding.Create()");
        loginPanel.Should().Contain("<GardenLoadingIndicator />");
        loginPanel.Should().Contain("Outcome.Authenticated += NotifyAuthenticatedAsync;");
        loginPanel.Should().Contain("Outcome.Authenticated -= NotifyAuthenticatedAsync;");
        mobileApp.Should().Contain("<LoginPanel OnAuthenticated=\"StateHasChanged\" />");
        File.Exists(Path.Combine(root, "src/Maui/Component/LoginPanel/LoginPanel.razor.css"))
            .Should().BeFalse("로그인 스타일은 Garden.Razor.LoginScreen이 소유한다");
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Orchid.Mobile.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Orchid.Mobile.slnx를 찾을 수 없다.");
    }
}
