using FluentAssertions;
using Orchid.Mobile.Service;
using Xunit;

namespace Orchid.UnitTests.Mobile;

/// <summary>
/// 모바일 릴레이 연결 상태의 전이와 화면 계약을 검증합니다.
/// </summary>
public sealed class MobileRelayConnectionStateTest
{
    [Fact]
    public void GivenNewStateWhenCreatedThenItAwaitsPairing()
    {
        // Given & When
        MobileRelayConnectionState state = new();

        // Then
        state.Status.Should().Be(MobileRelayConnectionStatus.AwaitingPairing);
    }

    [Theory]
    [InlineData(MobileRelayConnectionStatus.Connecting)]
    [InlineData(MobileRelayConnectionStatus.Connected)]
    [InlineData(MobileRelayConnectionStatus.Failed)]
    public void GivenStateWhenStatusChangesThenItPublishesTheCurrentStatus(MobileRelayConnectionStatus status)
    {
        // Given
        MobileRelayConnectionState state = new();
        int changedCount = 0;
        state.Changed += (_, _) => changedCount++;

        // When
        state.SetStatus(status);

        // Then
        state.Status.Should().Be(status);
        changedCount.Should().Be(1);
    }

    [Fact]
    public void GivenSameStatusWhenSetThenItDoesNotPublishADuplicateChange()
    {
        // Given
        MobileRelayConnectionState state = new();
        int changedCount = 0;
        state.Changed += (_, _) => changedCount++;

        // When
        state.SetStatus(MobileRelayConnectionStatus.AwaitingPairing);

        // Then
        changedCount.Should().Be(0);
    }

    [Fact]
    public void MobileAppShouldBindTheHeaderAndBodyToTheSharedConnectionState()
    {
        // Given
        string mobileApp = ReadMobileSource("Component", "MobileApp", "MobileApp.razor");

        // When & Then
        mobileApp.Should().Contain("ConnectionStatus=\"@ConnectionState.Status\"");
        mobileApp.Should().Contain("ConnectionState.Status is MobileRelayConnectionStatus.AwaitingPairing or MobileRelayConnectionStatus.Failed");
        mobileApp.Should().Contain("ConnectionState.Changed += OnConnectionStateChanged;");
    }

    [Fact]
    public void MobileHeaderShouldNotUseADefaultConnectedBoolean()
    {
        // Given
        string mobileHeader = ReadMobileSource("Component", "MobileHeader", "MobileHeader.razor");

        // When & Then
        mobileHeader.Should().Contain("public MobileRelayConnectionStatus ConnectionStatus");
        mobileHeader.Should().NotContain("public bool IsConnected { get; set; } = true;");
        mobileHeader.Should().Contain("MobileRelayConnectionStatus.Connected");
        mobileHeader.Should().Contain("MobileRelayConnectionStatus.Failed");
    }

    private static string ReadMobileSource(params string[] segments)
    {
        string repoRoot = FindRepositoryRoot();
        string path = Path.Combine([repoRoot, "src", "Mobile", "src", .. segments]);
        return File.ReadAllText(path);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Orchid.Mobile.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Orchid.Mobile.slnx not found.");
    }
}
