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
    public async Task ThrowIfInPast_DateTimeOffset_WhenValueIsNow_DoesNotThrow()
    {
        // Arrange
        var argument = DateTimeOffset.UtcNow.AddSeconds(1);

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfInPast(argument);
        _ = await Assert.That(argument).IsGreaterThan(DateTimeOffset.UtcNow.AddSeconds(-1));
    }

    [Test]
    public async Task ThrowIfInPast_DateTimeOffset_WhenValueIsInFuture_DoesNotThrow()
    {
        // Arrange
        var argument = DateTimeOffset.UtcNow.AddHours(1);

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
        var argument = DateTime.UtcNow.AddHours(1);

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
}
#endif
