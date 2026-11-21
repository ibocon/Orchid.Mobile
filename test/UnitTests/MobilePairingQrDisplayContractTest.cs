// keep: FluentAssertions assertions
using FluentAssertions;
// keep: xUnit Fact attribute
using Xunit;

namespace Orchid.UnitTests.Mobile;

/// <summary>
/// 모바일에서 발급하는 페어링 QR의 화면 수명 계약을 검증합니다.
/// </summary>
public sealed class MobilePairingQrDisplayContractTest
{
    [Fact]
    public void GivenMobilePairingUiWhenInspectingThenItGeneratesAndRendersOnlyTheStandardPayload()
    {
        // Given
        string component = ReadRepositoryFile("src/Maui/Component/QrPairingCode/QrPairingCode.razor");

        // When
        bool generatesKey = component.Contains("PairingKey.Generate().ToPayload()", StringComparison.Ordinal);
        bool rendersQr = component.Contains("ToPngDataUrl(_payload)", StringComparison.Ordinal);

        // Then
        generatesKey.Should().BeTrue();
        rendersQr.Should().BeTrue();
    }

    [Fact]
    public void GivenQrImageEncoderWhenInspectingThenItUsesCrossPlatformPngWithoutLoggingPayload()
    {
        // Given
        string encoder = ReadRepositoryFile("src/Maui/Service/QrCodeImageEncoder.cs");

        // When
        bool createsPng = encoder.Contains("PngByteQRCodeHelper.GetQRCode", StringComparison.Ordinal);
        bool producesDataUrl = encoder.Contains("data:image/png;base64", StringComparison.Ordinal);
        bool logsPayload = encoder.Contains("Logger", StringComparison.Ordinal) || encoder.Contains("Log", StringComparison.Ordinal);

        // Then
        createsPng.Should().BeTrue();
        producesDataUrl.Should().BeTrue();
        logsPayload.Should().BeFalse();
    }

    [Fact]
    public void GivenPairingCardWhenInspectingThenItComposesDisplayWithoutAutoSubmittingGeneratedKey()
    {
        // Given
        string scanner = ReadRepositoryFile("src/Maui/Component/QrPairingScanner/QrPairingScanner.razor");

        // When
        bool composesDisplay = scanner.Contains("<QrPairingCode />", StringComparison.Ordinal);
        bool autoSubmitsGeneratedKey = scanner.Contains("OnQrScanned.InvokeAsync(_payload)", StringComparison.Ordinal);

        // Then
        composesDisplay.Should().BeTrue();
        autoSubmitsGeneratedKey.Should().BeFalse();
    }

    [Fact]
    public void GivenMobilePairingUiWhenInspectingThenQrIsCollapsedByDefaultAndGeneratedLazily()
    {
        // Given
        string component = ReadRepositoryFile("src/Maui/Component/QrPairingCode/QrPairingCode.razor");

        // When
        bool collapsedByDefault = component.Contains("private bool _expanded;", StringComparison.Ordinal);
        bool rendersOnlyWhenExpanded = component.Contains("@if (_expanded)", StringComparison.Ordinal);
        bool generatesOnInitialize = component.Contains("OnInitialized", StringComparison.Ordinal);

        // Then
        collapsedByDefault.Should().BeTrue();
        rendersOnlyWhenExpanded.Should().BeTrue();
        generatesOnInitialize.Should().BeFalse();
    }

    [Fact]
    public void GivenQrStyleWhenInspectingThenImageWidthIsCappedAndNotNestedInCard()
    {
        // Given
        string style = ReadRepositoryFile("src/Maui/Component/QrPairingCode/QrPairingCode.razor.css");
        string component = ReadRepositoryFile("src/Maui/Component/QrPairingCode/QrPairingCode.razor");

        // When
        bool capsImageWidth = style.Contains("width: min(168px, 60%);", StringComparison.Ordinal);
        bool nestsCard = component.Contains("<GardenCard", StringComparison.Ordinal);
        bool hasIconActions = component.Contains("Icons.Material.Filled.ContentCopy", StringComparison.Ordinal)
            && component.Contains("Icons.Material.Filled.Refresh", StringComparison.Ordinal);

        // Then
        capsImageWidth.Should().BeTrue();
        nestsCard.Should().BeFalse();
        hasIconActions.Should().BeTrue();
    }

    [Fact]
    public void GivenPairingCardWhenInspectingThenActionsPrecedeQrAndGuidanceIsNotDuplicated()
    {
        // Given
        string scanner = ReadRepositoryFile("src/Maui/Component/QrPairingScanner/QrPairingScanner.razor");
        string component = ReadRepositoryFile("src/Maui/Component/QrPairingCode/QrPairingCode.razor");

        // When
        int connectIndex = scanner.IndexOf("OnClick=\"SubmitAsync\"", StringComparison.Ordinal);
        int qrIndex = scanner.IndexOf("<QrPairingCode />", StringComparison.Ordinal);
        bool duplicatesGuidance = component.Contains("스캔하거나", StringComparison.Ordinal);

        // Then
        connectIndex.Should().BeGreaterThan(-1);
        qrIndex.Should().BeGreaterThan(connectIndex);
        duplicatesGuidance.Should().BeFalse();
    }

    private static string ReadRepositoryFile(string relativePath)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Orchid.Mobile.slnx")))
        {
            directory = directory.Parent;
        }

        string repositoryRoot = directory?.FullName
            ?? throw new InvalidOperationException("Orchid.Mobile.slnx not found.");
        return File.ReadAllText(Path.Combine(repositoryRoot, relativePath));
    }
}
