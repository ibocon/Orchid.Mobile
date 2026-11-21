#if ANDROID
using Orchid.Mobile.Service;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace Orchid.Mobile.Platforms.Android;

/// <summary>
/// Android 카메라에서 Orchid 페어링 QR 코드 하나를 읽습니다.
/// </summary>
public sealed class QrPairingScannerService : IQrPairingScanner
{
    /// <inheritdoc />
    public async Task<string?> ScanAsync(CancellationToken cancellationToken = default)
    {
        if (!BarcodeScanning.IsSupported)
        {
            return null;
        }

        PermissionStatus permission = await Permissions
            .CheckStatusAsync<Permissions.Camera>()
            .ConfigureAwait(false);
        if (permission != PermissionStatus.Granted)
        {
            permission = await Permissions
                .RequestAsync<Permissions.Camera>()
                .ConfigureAwait(false);
        }

        if (permission != PermissionStatus.Granted)
        {
            return null;
        }

        TaskCompletionSource<string?> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            IReadOnlyList<Window> windows = Application.Current?.Windows ?? [];
            Page? hostPage = windows.Count > 0 ? windows[0].Page : null;
            if (hostPage is null)
            {
                completion.TrySetResult(null);
                return;
            }

            await hostPage.Navigation
                .PushModalAsync(new QrPairingScannerPage(completion))
                .ConfigureAwait(false);
        }).ConfigureAwait(false);

        using CancellationTokenRegistration registration = cancellationToken.Register(
            () => completion.TrySetResult(null));
        return await completion.Task.ConfigureAwait(false);
    }
}

internal sealed class QrPairingScannerPage : ContentPage
{
    private readonly TaskCompletionSource<string?> _completion;
    private readonly CameraBarcodeReaderView _reader;

    public QrPairingScannerPage(TaskCompletionSource<string?> completion)
    {
        _completion = completion;
        _reader = new CameraBarcodeReaderView
        {
            Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.TwoDimensional,
                AutoRotate = true,
                Multiple = false,
            },
        };
        _reader.BarcodesDetected += OnBarcodesDetected;

        Content = new Grid
        {
            Children =
            {
                _reader,
                new VerticalStackLayout
                {
                    Padding = 24,
                    VerticalOptions = LayoutOptions.End,
                    Children =
                    {
                        new Button
                        {
                            Text = "취소",
                            Command = new Command(() => Complete(null)),
                        },
                    },
                },
            },
        };
    }

    protected override void OnDisappearing()
    {
        _reader.BarcodesDetected -= OnBarcodesDetected;
        _completion.TrySetResult(null);
        base.OnDisappearing();
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs eventArgs)
    {
        string? payload = eventArgs.Results
            .Select(result => result.Value)
            .FirstOrDefault(IsPairingPayload);
        if (payload is not null)
        {
            MainThread.BeginInvokeOnMainThread(() => Complete(payload));
        }
    }

    private static bool IsPairingPayload(string? value)
    {
        return value?.StartsWith("orchid-pair:v1:", StringComparison.Ordinal) == true;
    }

    private void Complete(string? payload)
    {
        if (!_completion.TrySetResult(payload))
        {
            return;
        }

        _ = Navigation.PopModalAsync();
    }
}
#endif
