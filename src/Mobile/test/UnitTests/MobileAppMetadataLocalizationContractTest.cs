// keep: System.Xml.Linq for Android XML resource inspection
using System.Xml.Linq;
// keep: FluentAssertions assertions
using FluentAssertions;
// keep: xUnit Fact attribute
using Xunit;

namespace Orchid.UnitTests.Mobile;

/// <summary>
/// Android 패키지 메타데이터가 문화권별 앱 이름 리소스를 참조하는지 검증합니다.
/// </summary>
public sealed class MobileAppMetadataLocalizationContractTest
{
    private static readonly XNamespace AndroidNamespace = "http://schemas.android.com/apk/res/android";

    [Fact]
    public void GivenAndroidPackageMetadataWhenReadThenKoreanAndDefaultAppNamesAndAdaptiveLauncherIconAreDeclared()
    {
        // Given
        string repoRoot = FindRepositoryRoot();
        XDocument manifest = XDocument.Load(Path.Combine(repoRoot, "src/Mobile/src/Platforms/Android/AndroidManifest.xml"));
        XDocument project = XDocument.Load(Path.Combine(repoRoot, "src/Mobile/src/Orchid.Mobile.csproj"));

        // When
        XElement application = manifest.Root!
            .Element("application")!;
        XElement mauiIcon = project.Descendants("MauiIcon").Single();
        string foregroundScale = mauiIcon.Attribute("ForegroundScale")!.Value;
        string icon = application.Attribute(AndroidNamespace + "icon")!.Value;
        string label = application.Attribute(AndroidNamespace + "label")!.Value;
        string roundIcon = application.Attribute(AndroidNamespace + "roundIcon")!.Value;
        string defaultName = ReadAppName(repoRoot, "values");
        string koreanName = ReadAppName(repoRoot, "values-ko");

        // Then
        foregroundScale.Should().Be("0.84");
        icon.Should().Be("@mipmap/appicon");
        label.Should().Be("@string/app_name");
        roundIcon.Should().Be("@mipmap/appicon_round");
        defaultName.Should().Be("Universal Clipboard Bridge");
        koreanName.Should().Be("모두클립");
    }

    private static string ReadAppName(string repositoryRoot, string resourceQualifier)
    {
        string resourcePath = Path.Combine(
            repositoryRoot,
            "src", "Mobile", "src",
            "Platforms",
            "Android",
            "Resources",
            resourceQualifier,
            "strings.xml");
        XDocument resources = XDocument.Load(resourcePath);

        return resources.Root!
            .Elements("string")
            .Single(element => element.Attribute("name")?.Value == "app_name")
            .Value;
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
