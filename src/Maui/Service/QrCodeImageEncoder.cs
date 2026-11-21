using QRCoder;

namespace Orchid.Mobile.Service;

/// <summary>
/// 페어링 페이로드를 화면 표시용 PNG data URL로 인코딩합니다.
/// </summary>
public interface IQrCodeImageEncoder
{
    /// <summary>
    /// 페어링 페이로드를 PNG data URL로 변환합니다.
    /// </summary>
    /// <param name="payload">QR 코드에 담을 표준 페어링 페이로드입니다.</param>
    /// <returns>화면에 즉시 표시할 PNG data URL입니다.</returns>
    string ToPngDataUrl(string payload);
}

/// <summary>
/// QRCoder를 사용해 페어링 페이로드의 PNG 이미지를 생성합니다.
/// </summary>
public sealed class QrCodeImageEncoder : IQrCodeImageEncoder
{
    /// <inheritdoc />
    public string ToPngDataUrl(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        byte[] png = CreatePng(payload);
        return $"data:image/png;base64,{Convert.ToBase64String(png)}";
    }

    private static readonly byte[] DarkColorRgba = [0, 0, 0, 255];

    private static readonly byte[] LightColorRgba = [255, 255, 255, 255];

    private static byte[] CreatePng(string payload)
    {
        // reference: QRCoder 1.9.0 / PngByteQRCodeHelper.GetQRCode
        return PngByteQRCodeHelper.GetQRCode(payload, 12, DarkColorRgba, LightColorRgba, QRCodeGenerator.ECCLevel.Q);
    }
}
