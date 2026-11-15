namespace Orchid.Mobile.Component;

/// <summary>
/// MobileHeader 리소스 키 상수 마커 클래스입니다.
/// </summary>
public sealed class MobileHeaderResource
{
    /// <summary>모바일 상단바 앱 표시 이름 리소스 키입니다.</summary>
    public const string AppTitle = nameof(AppTitle);

    /// <summary>릴레이 연결 상태(동기화 중) 리소스 키입니다.</summary>
    public const string SyncingLabel = nameof(SyncingLabel);

    /// <summary>릴레이 연결 끊김(오프라인) 리소스 키입니다.</summary>
    public const string OfflineLabel = nameof(OfflineLabel);

    /// <summary>테마 전환 버튼 레이블 리소스 키입니다.</summary>
    public const string ToggleThemeLabel = nameof(ToggleThemeLabel);

    /// <summary>로그아웃 레이블 리소스 키입니다.</summary>
    public const string SignOutLabel = nameof(SignOutLabel);

    private MobileHeaderResource()
    {
    }
}
