using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace Orchid.UnitTests.Mobile;

/// <summary>
/// Mobile ?몄뒪???섏씠吏媛 Android?먯꽌 Blazor ?먮룞 ?쒖옉??留됰뒗 ?ㅽ겕由쏀듃 ?쒓렇瑜??좎??섎뒗吏 寃利앺븯???⑥쐞 ?뚯뒪?몄엯?덈떎.
/// </summary>
public sealed partial class MobileHostPageTest
{
    [Fact]
    public void IndexHtmlWhenBlazorScriptLoadedShouldDeferAndDisableAutostart()
    {
        // Given
        string html = File.ReadAllText(FindRepositoryFile("src/wwwroot/index.html"));

        // When
        Match script = BlazorWebViewScriptRegex().Match(html);

        // Then
        script.Success.Should().BeTrue();
        script.Value.Should().Contain("defer");
        script.Value.Should().Contain("autostart=\"false\"");
    }

    private static string FindRepositoryFile(string relativePath)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Orchid.Mobile.slnx")))
        {
            directory = directory.Parent;
        }

        return Path.Combine(directory?.FullName ?? throw new InvalidOperationException("Orchid.Mobile.slnx not found."), relativePath);
    }

    [GeneratedRegex("<script[^>]*src=\"_framework/blazor\\.webview\\.js\"[^>]*>")]
    private static partial Regex BlazorWebViewScriptRegex();
}
