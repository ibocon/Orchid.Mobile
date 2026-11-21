using FluentAssertions;
using Xunit;

namespace Orchid.UnitTests.Mobile;

public sealed class MobileClipboardChangeSourceRegistrationTest
{
    [Fact]
    public void GivenAndroidCompositionWhenReadingMauiProgramThenConcreteClipboardChangeSourceIsRegistered()
    {
        // Given
        string mauiProgram = File.ReadAllText(FindRepositoryFile("src/Mobile/src/MauiProgram.cs"));

        // When
        bool registersConcreteSource = mauiProgram.Contains(
            "AddSingleton<IClipboardChangeSource>(_ => new Orchid.Mobile.Platforms.Android.AndroidClipboardChangeSource",
            StringComparison.Ordinal);

        // Then
        registersConcreteSource.Should().BeTrue();
    }

    [Fact]
    public void GivenAndroidClipboardChangeSourceWhenReadingImplementationThenPrimaryClipListenerAndSingleStartGuardAreDeclared()
    {
        // Given
        string source = File.ReadAllText(FindRepositoryFile("src/Mobile/src/Platforms/Android/AndroidClipboardChangeSource.cs"));

        // When
        bool implementsListener = source.Contains("ClipboardManager.IOnPrimaryClipChangedListener", StringComparison.Ordinal);
        bool registersListener = source.Contains("AddPrimaryClipChangedListener(this)", StringComparison.Ordinal);
        bool guardsRepeatedStart = source.Contains("if (_isStarted)", StringComparison.Ordinal);

        // Then
        implementsListener.Should().BeTrue();
        registersListener.Should().BeTrue();
        guardsRepeatedStart.Should().BeTrue();
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
}
