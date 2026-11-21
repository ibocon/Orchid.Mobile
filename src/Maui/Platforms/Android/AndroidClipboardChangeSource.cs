#if ANDROID
using Android.Content;
using Orchid.Core.Abstraction;

namespace Orchid.Mobile.Platforms.Android;

/// <summary>
/// Android 시스템 클립보드의 변경을 <see cref="IClipboardChangeSource"/> 계약으로 전달합니다.
/// </summary>
public sealed class AndroidClipboardChangeSource : Java.Lang.Object, IClipboardChangeSource, ClipboardManager.IOnPrimaryClipChangedListener
{
    private readonly ClipboardManager _clipboardManager;
    private bool _isStarted;

    /// <summary>
    /// <see cref="AndroidClipboardChangeSource"/> 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="context">Android 애플리케이션 컨텍스트입니다.</param>
    public AndroidClipboardChangeSource(Context context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _clipboardManager = context.GetSystemService(Context.ClipboardService) as ClipboardManager
            ?? throw new InvalidOperationException("Android ClipboardManager is unavailable.");
    }

    /// <inheritdoc />
    public event Func<Task>? Changed;

    /// <inheritdoc />
    public void Start()
    {
        if (_isStarted)
        {
            return;
        }

        _clipboardManager.AddPrimaryClipChangedListener(this);
        _isStarted = true;
    }

    /// <summary>
    /// 시스템 기본 클립보드가 변경됐을 때 등록된 비동기 처리기를 시작합니다.
    /// </summary>
    public void OnPrimaryClipChanged()
    {
        _ = NotifyChangedAsync();
    }

    private async Task NotifyChangedAsync()
    {
        Func<Task>? handler = Changed;
        if (handler is null)
        {
            return;
        }

        try
        {
            await
                handler()
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            global::Android.Util.Log.Error("Orchid", $"Clipboard change handling failed: {exception}");
        }
    }
}
#endif
