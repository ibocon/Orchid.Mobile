// reference: Garden.Share 10.0.0 / IdentityProvider, AuthenticationProfile in Garden.Share
using FluentAssertions;
using Garden.Share;
using Orchid.Mobile.Service;
using Xunit;

namespace Orchid.UnitTests.Mobile;

/// <summary>
/// <see cref="OrchidMobileAuthService"/>가 IResourceOwnerAuth의 실제 프로필만 노출하는지 검증합니다.
/// </summary>
public sealed class OrchidMobileAuthServiceTest
{
    [Fact]
    public async Task SignInWithProviderAsyncExposesProfileFromResourceOwnerAuth()
    {
        // Given
        FakeResourceOwnerAuth fake = new()
        {
            LoginResult = FakeResourceOwnerAuth.CreateAuthenticated(),
            Profile = new AuthenticationProfile { Name = "Yegun Kim", Email = "wlfka102@gmail.com", Avatar = "Y" },
        };
        OrchidMobileAuthService sut = new(fake);

        // When
        await sut.SignInWithProviderAsync("google");

        // Then
        sut.IsAuthenticated.Should().BeTrue();
        sut.CurrentUserEmail.Should().Be("wlfka102@gmail.com");
        sut.CurrentUserName.Should().Be("Yegun Kim");
        sut.CurrentUserInitial.Should().Be("Y");
        fake.LastIdentityProvider.Should().Be(IdentityProvider.Google);
    }

    [Fact]
    public async Task SignInWithProviderAsyncKeepsAnonymousWhenLoginIsCanceled()
    {
        // Given
        FakeResourceOwnerAuth fake = new() { LoginResult = null };
        OrchidMobileAuthService sut = new(fake);

        // When
        await sut.SignInWithProviderAsync("kakao");

        // Then
        sut.IsAuthenticated.Should().BeFalse();
        sut.CurrentUserEmail.Should().BeEmpty();
        sut.CurrentUserInitial.Should().Be("O");
        fake.LastIdentityProvider.Should().Be("kakao");
    }

    [Fact]
    public async Task SignOutAsyncClearsAuthenticationStateAndProfile()
    {
        // Given
        FakeResourceOwnerAuth fake = new()
        {
            LoginResult = FakeResourceOwnerAuth.CreateAuthenticated(),
            Profile = new AuthenticationProfile { Name = "Yegun Kim", Email = "wlfka102@gmail.com", Avatar = "Y" },
        };
        OrchidMobileAuthService sut = new(fake);
        await sut.SignInWithProviderAsync("google");

        // When
        await sut.SignOutAsync();

        // Then
        sut.IsAuthenticated.Should().BeFalse();
        sut.CurrentUserEmail.Should().BeEmpty();
    }

    [Fact]
    public async Task RestoreSessionAsyncReturnsFalseWhenNoStoredSession()
    {
        // Given
        OrchidMobileAuthService sut = new(new FakeResourceOwnerAuth());

        // When
        bool restored = await sut.RestoreSessionAsync();

        // Then
        restored.Should().BeFalse();
        sut.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task SignInWithProviderAsyncRejectsUnsupportedProvider()
    {
        // Given
        OrchidMobileAuthService sut = new(new FakeResourceOwnerAuth());

        // When
        Func<Task> act = () => sut.SignInWithProviderAsync("github");

        // Then
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}
