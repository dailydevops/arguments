#if NET8_0_OR_GREATER
namespace NetEvolve.Arguments.Tests.Unit;

using System;

public sealed class ArgumentOutOfRangeException_ThrowIfInFutureTests
{
    [Test]
    public void ThrowIfInFuture_DateTimeOffset_WhenValueIsInFuture_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var argument = DateTimeOffset.UtcNow.AddHours(1);

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
        _ = await Assert.That(argument).IsLessThanOrEqualTo(DateTimeOffset.UtcNow.AddSeconds(5));
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
        var argument = DateTime.UtcNow.AddHours(1);

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
        var argument = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

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
        _ = await Assert.That(argument).IsLessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
    }
}
#endif
