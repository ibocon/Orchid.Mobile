// keep: FluentAssertions assertions
using FluentAssertions;
// keep: xUnit Fact attribute
using Xunit;

namespace Orchid.UnitTests.Mobile;

/// <summary>
/// 모바일 QR 페어링 스캐너의 정적 연결 계약을 검증합니다.
/// </summary>
public sealed class MobileQrPairingScannerContractTest
{
    [Fact]
    public void GivenAndroidManifestWhenInspectingCameraAccessThenCameraPermissionExists()
    {
        // Given
        string manifest = ReadRepositoryFile("src/Mobile/src/Platforms/Android/AndroidManifest.xml");

        // When
        bool hasCameraPermission = manifest.Contains("android.permission.CAMERA", StringComparison.Ordinal);

        // Then
        hasCameraPermission.Should().BeTrue();
    }

    [Fact]
    public void GivenMobileHostWhenInspectingBootstrapThenBarcodeReaderIsConfigured()
    {
        // Given
        string project = ReadRepositoryFile("src/Mobile/src/Orchid.Mobile.csproj");
        string mauiProgram = ReadRepositoryFile("src/Mobile/src/MauiProgram.cs");

        // When
        bool packageReferenced = project.Contains("ZXing.Net.Maui.Controls", StringComparison.Ordinal);
        bool readerConfigured = mauiProgram.Contains("UseBarcodeReader()", StringComparison.Ordinal);

        // Then
        packageReferenced.Should().BeTrue();
        readerConfigured.Should().BeTrue();
    }

    [Fact]
    public void GivenQrPairingComponentWhenScanningSucceedsThenItUsesExistingPairingCallback()
    {
        // Given
        string component = ReadRepositoryFile("src/Mobile/src/Component/QrPairingScanner/QrPairingScanner.razor");

        // When
        bool scannerInjected = component.Contains("IQrPairingScanner", StringComparison.Ordinal);
        bool callbackUsed = component.Contains("OnQrScanned.InvokeAsync(payload)", StringComparison.Ordinal);

        // Then
        scannerInjected.Should().BeTrue();
        callbackUsed.Should().BeTrue();
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
