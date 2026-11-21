namespace Orchid.Mobile.Service;

/// <summary>
/// 모바일 플랫폼의 QR 페어링 키 스캔을 제공합니다.
/// </summary>
public interface IQrPairingScanner
{
    /// <summary>
    /// QR 코드에서 페어링 키 하나를 스캔합니다.
    /// </summary>
    /// <param name="cancellationToken">스캔 취소를 관찰하는 토큰입니다.</param>
    /// <returns>성공한 페어링 키 또는 취소·미지원 시 <see langword="null"/>입니다.</returns>
    Task<string?> ScanAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// 카메라 QR 스캔을 지원하지 않는 플랫폼의 대체 구현입니다.
/// </summary>
public sealed class UnsupportedQrPairingScanner : IQrPairingScanner
{
    /// <inheritdoc />
    public Task<string?> ScanAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(null);
    }
}
