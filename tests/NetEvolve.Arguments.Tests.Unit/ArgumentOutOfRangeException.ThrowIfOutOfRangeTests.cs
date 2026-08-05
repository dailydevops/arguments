namespace NetEvolve.Arguments.Tests.Unit;

using System;

public sealed class ArgumentOutOfRangeException_ThrowIfOutOfRangeTests
{
    [Test]
    public void ThrowIfOutOfRange_WhenValueBelowMinimum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var argument = 3;

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfOutOfRange(argument, 5, 10);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
    }

    [Test]
    public void ThrowIfOutOfRange_WhenValueAboveMaximum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var argument = 15;

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfOutOfRange(argument, 5, 10);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfOutOfRange_WhenValueEqualsMinimum_DoesNotThrow()
    {
        // Arrange
        var argument = 5;

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfOutOfRange(argument, 5, 10);
        _ = await Assert.That(argument).IsEqualTo(5);
    }

    [Test]
    public async Task ThrowIfOutOfRange_WhenValueEqualsMaximum_DoesNotThrow()
    {
        // Arrange
        var argument = 10;

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfOutOfRange(argument, 5, 10);
        _ = await Assert.That(argument).IsEqualTo(10);
    }

    [Test]
    public async Task ThrowIfOutOfRange_WhenValueWithinRange_DoesNotThrow()
    {
        // Arrange
        var argument = 7;

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfOutOfRange(argument, 5, 10);
        _ = await Assert.That(argument).IsGreaterThanOrEqualTo(5).And.IsLessThanOrEqualTo(10);
    }

    [Test]
    public async Task ThrowIfOutOfRange_WithDoubles_WhenValueWithinRange_DoesNotThrow()
    {
        // Arrange
        var argument = 7.5;

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfOutOfRange(argument, 5.0, 10.0);
        _ = await Assert.That(argument).IsGreaterThanOrEqualTo(5.0).And.IsLessThanOrEqualTo(10.0);
    }
}
