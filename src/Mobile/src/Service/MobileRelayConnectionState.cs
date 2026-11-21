namespace Orchid.Mobile.Service;

/// <summary>
/// 모바일 릴레이의 사용자 표시 연결 상태를 공유합니다.
/// </summary>
public enum MobileRelayConnectionStatus
{
    /// <summary>페어링 키 입력을 기다리는 상태입니다.</summary>
    AwaitingPairing,

    /// <summary>릴레이 허브에 접속하는 상태입니다.</summary>
    Connecting,

    /// <summary>릴레이 허브 접속이 완료된 상태입니다.</summary>
    Connected,

    /// <summary>릴레이 허브 접속이 실패한 상태입니다.</summary>
    Failed,
}

/// <summary>
/// Android Foreground Service와 Blazor UI 사이에서 릴레이 연결 상태를 공유합니다.
/// </summary>
public sealed class MobileRelayConnectionState
{
    /// <summary>연결 상태가 바뀌었을 때 발생합니다.</summary>
    public event EventHandler? Changed;

    /// <summary>현재 릴레이 연결 상태입니다.</summary>
    public MobileRelayConnectionStatus Status { get; private set; } = MobileRelayConnectionStatus.AwaitingPairing;

    /// <summary>현재 릴레이 연결 상태를 갱신합니다.</summary>
    /// <param name="status">새 연결 상태입니다.</param>
    public void SetStatus(MobileRelayConnectionStatus status)
    {
        if (Status == status)
        {
            return;
        }

        Status = status;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
