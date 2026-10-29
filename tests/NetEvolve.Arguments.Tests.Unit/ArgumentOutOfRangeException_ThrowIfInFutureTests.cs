#if NET8_0_OR_GREATER
namespace NetEvolve.Arguments.Tests.Unit;

using System;

public sealed class ArgumentOutOfRangeException_ThrowIfInFutureTests
{
    [Test]
    public void ThrowIfInFuture_DateTimeOffset_WhenValueIsInFuture_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var argument = DateTimeOffset.UtcNow.AddHours(1D);

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfInFuture(argument);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfInFuture_DateTimeOffset_WhenValueIsNow_DoesNotThrow()
    {
        // Arrange
        var argument = DateTimeOffset.UtcNow;

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfInFuture(argument);
        _ = await Assert.That(argument).IsLessThanOrEqualTo(DateTimeOffset.UtcNow.AddSeconds(5D));
    }

    [Test]
    public async Task ThrowIfInFuture_DateTimeOffset_WhenValueIsInPast_DoesNotThrow()
    {
        // Arrange
        var argument = DateTimeOffset.UtcNow.AddHours(-1);

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfInFuture(argument);
        _ = await Assert.That(argument).IsLessThan(DateTimeOffset.UtcNow);
    }

    [Test]
    public void ThrowIfInFuture_DateTime_WhenValueIsInFuture_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var argument = DateTime.UtcNow.AddHours(1D);

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfInFuture(argument);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfInFuture_DateTime_WhenValueIsInPast_DoesNotThrow()
    {
        // Arrange
        var argument = DateTime.UtcNow.AddHours(-1);

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfInFuture(argument);
        _ = await Assert.That(argument).IsLessThan(DateTime.UtcNow);
    }

    [Test]
    public void ThrowIfInFuture_DateOnly_WhenValueIsInFuture_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var argument = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1D));

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfInFuture(argument);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfInFuture_DateOnly_WhenValueIsToday_DoesNotThrow()
    {
        // Arrange
        var argument = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfInFuture(argument);
        _ = await Assert.That(argument).IsLessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1D)));
    }

    private static readonly DateTimeOffset _fixedNow = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private static readonly TimeProvider _provider = new FixedTimeProvider(_fixedNow);

    [Test]
    public async Task ThrowIfInFuture_DateTimeOffset_FixedProvider_ValueEqualsNow_DoesNotThrow()
    {
        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfInFuture(_fixedNow, _provider);
        _ = await Assert.That(_fixedNow).IsEqualTo(_provider.GetUtcNow());
    }

    [Test]
    public async Task ThrowIfInFuture_DateTimeOffset_FixedProvider_ValueOneTickOff_Throws()
    {
        // Arrange
        var argument = _fixedNow.AddTicks(1);

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfInFuture(argument, _provider);

        // Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
        _ = await Assert.That(exception.Message).Contains("Value must not be in the future");
    }

    [Test]
    public async Task ThrowIfInFuture_DateTime_FixedProvider_ValueEqualsNow_DoesNotThrow()
    {
        // Arrange
        var argument = _fixedNow.UtcDateTime;

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfInFuture(argument, _provider);
        _ = await Assert.That(argument).IsEqualTo(_provider.GetUtcNow().UtcDateTime);
    }

    [Test]
    public async Task ThrowIfInFuture_DateTime_FixedProvider_ValueOneTickOff_Throws()
    {
        // Arrange
        var argument = _fixedNow.UtcDateTime.AddTicks(1);

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfInFuture(argument, _provider);

        // Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
        _ = await Assert.That(exception.Message).Contains("Value must not be in the future");
    }

    [Test]
    public async Task ThrowIfInFuture_DateOnly_FixedProvider_ValueIsToday_DoesNotThrow()
    {
        // Arrange
        var argument = DateOnly.FromDateTime(_fixedNow.UtcDateTime);

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfInFuture(argument, _provider);
        _ = await Assert.That(argument).IsEqualTo(DateOnly.FromDateTime(_provider.GetUtcNow().UtcDateTime));
    }

    [Test]
    public async Task ThrowIfInFuture_DateOnly_FixedProvider_ValueOneDayOff_Throws()
    {
        // Arrange
        var argument = DateOnly.FromDateTime(_fixedNow.UtcDateTime).AddDays(1);

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfInFuture(argument, _provider);

        // Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
        _ = await Assert.That(exception.Message).Contains("Value must not be in the future");
    }
}
#endif
