namespace NetEvolve.Arguments.Tests.Unit;

using System;

public sealed class ArgumentException_ThrowIfLengthLessThanTests
{
    [Test]
    public void ThrowIfLengthLessThan_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfLengthLessThan(argument, 5);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfLengthLessThan_WhenLengthLessThanMinimum_ThrowsArgumentException()
    {
        // Arrange
        var argument = "hi";

        // Act
        void Act() => ArgumentException.ThrowIfLengthLessThan(argument, 5);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfLengthLessThan_WhenLengthEqualsMinimum_DoesNotThrow()
    {
        // Arrange
        var argument = "12345";

        // Act & Assert
        ArgumentException.ThrowIfLengthLessThan(argument, 5);
        _ = await Assert.That(argument).HasLength().EqualTo(5);
    }

    [Test]
    public async Task ThrowIfLengthLessThan_WhenLengthGreaterThanMinimum_DoesNotThrow()
    {
        // Arrange
        var argument = "longer string";

        // Act & Assert
        ArgumentException.ThrowIfLengthLessThan(argument, 5);
        _ = await Assert.That(argument.Length).IsGreaterThan(5);
    }

    [Test]
    public void ThrowIfLengthLessThan_WhenEmptyStringAndMinimumIsPositive_ThrowsArgumentException()
    {
        // Arrange
        var argument = string.Empty;

        // Act
        void Act() => ArgumentException.ThrowIfLengthLessThan(argument, 1);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }
}
