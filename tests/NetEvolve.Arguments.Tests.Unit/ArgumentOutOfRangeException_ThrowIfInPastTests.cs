#if NET8_0_OR_GREATER
namespace NetEvolve.Arguments.Tests.Unit;

using System;

public sealed class ArgumentOutOfRangeException_ThrowIfInPastTests
{
    [Test]
    public void ThrowIfInPast_DateTimeOffset_WhenValueIsInPast_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var argument = DateTimeOffset.UtcNow.AddHours(-1);

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfInPast(argument);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfInPast_DateTimeOffset_WhenValueIsSlightlyInFuture_DoesNotThrow()
    {
        // Arrange
        var argument = DateTimeOffset.UtcNow.AddSeconds(1D);

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfInPast(argument);
        _ = await Assert.That(argument).IsGreaterThan(DateTimeOffset.UtcNow.AddSeconds(-1));
    }

    [Test]
    public async Task ThrowIfInPast_DateTimeOffset_WhenValueIsInFuture_DoesNotThrow()
    {
        // Arrange
        var argument = DateTimeOffset.UtcNow.AddHours(1D);

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfInPast(argument);
        _ = await Assert.That(argument).IsGreaterThan(DateTimeOffset.UtcNow);
    }

    [Test]
    public void ThrowIfInPast_DateTime_WhenValueIsInPast_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var argument = DateTime.UtcNow.AddHours(-1);

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfInPast(argument);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfInPast_DateTime_WhenValueIsInFuture_DoesNotThrow()
    {
        // Arrange
        var argument = DateTime.UtcNow.AddHours(1D);

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfInPast(argument);
        _ = await Assert.That(argument).IsGreaterThan(DateTime.UtcNow);
    }

    [Test]
    public void ThrowIfInPast_DateOnly_WhenValueIsInPast_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var argument = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfInPast(argument);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfInPast_DateOnly_WhenValueIsToday_DoesNotThrow()
    {
        // Arrange
        var argument = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfInPast(argument);
        _ = await Assert.That(argument).IsGreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));
    }

    private static readonly DateTimeOffset _fixedNow = new(2100, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private static readonly TimeProvider _provider = new FixedTimeProvider(_fixedNow);

    [Test]
    public async Task ThrowIfInPast_DateTimeOffset_FixedProvider_ValueEqualsNow_DoesNotThrow()
    {
        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfInPast(_fixedNow, _provider);
        _ = await Assert.That(_fixedNow).IsEqualTo(_provider.GetUtcNow());
    }

    [Test]
    public async Task ThrowIfInPast_DateTimeOffset_FixedProvider_ValueOneTickOff_Throws()
    {
        // Arrange
        var argument = _fixedNow.AddTicks(-1);

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfInPast(argument, _provider);

        // Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
        _ = await Assert.That(exception.Message).Contains("Value must not be in the past");
    }

    [Test]
    public async Task ThrowIfInPast_DateTime_FixedProvider_ValueEqualsNow_DoesNotThrow()
    {
        // Arrange
        var argument = _fixedNow.UtcDateTime;

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfInPast(argument, _provider);
        _ = await Assert.That(argument).IsEqualTo(_provider.GetUtcNow().UtcDateTime);
    }

    [Test]
    public async Task ThrowIfInPast_DateTime_FixedProvider_ValueOneTickOff_Throws()
    {
        // Arrange
        var argument = _fixedNow.UtcDateTime.AddTicks(-1);

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfInPast(argument, _provider);

        // Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
        _ = await Assert.That(exception.Message).Contains("Value must not be in the past");
    }

    [Test]
    public async Task ThrowIfInPast_DateOnly_FixedProvider_ValueIsToday_DoesNotThrow()
    {
        // Arrange
        var argument = DateOnly.FromDateTime(_fixedNow.UtcDateTime);

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfInPast(argument, _provider);
        _ = await Assert.That(argument).IsEqualTo(DateOnly.FromDateTime(_provider.GetUtcNow().UtcDateTime));
    }

    [Test]
    public async Task ThrowIfInPast_DateOnly_FixedProvider_ValueOneDayOff_Throws()
    {
        // Arrange
        var argument = DateOnly.FromDateTime(_fixedNow.UtcDateTime).AddDays(-1);

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfInPast(argument, _provider);

        // Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
        _ = await Assert.That(exception.Message).Contains("Value must not be in the past");
    }
}
#endif
