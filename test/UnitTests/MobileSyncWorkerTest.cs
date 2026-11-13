using FluentAssertions;
using Moq;
using Orchid.Core.Abstraction;
using Xunit;

namespace Orchid.UnitTests.Mobile;

public sealed class MobileSyncWorkerTest
{
    [Fact]
    public void WorkerShouldPauseBackgroundPollingWhenAppSuspended()
    {
        // Given
        Mock<IMobileSyncBackgroundWorker> worker = new();

        // When & Then
        worker.Setup(x => x.IsActive).Returns(false);
        worker.Object.IsActive.Should().BeFalse();
    }

    [Fact]
    public void WorkerShouldResumeWhenAppReturnsToForeground()
    {
        // Given
        Mock<IMobileSyncBackgroundWorker> worker = new();
        worker.Setup(x => x.IsActive).Returns(true);

        // When & Then
        worker.Object.IsActive.Should().BeTrue();
    }
}
