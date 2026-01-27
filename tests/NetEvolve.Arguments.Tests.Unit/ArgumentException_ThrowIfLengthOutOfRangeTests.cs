namespace NetEvolve.Arguments.Tests.Unit;

using System;

public sealed class ArgumentException_ThrowIfLengthOutOfRangeTests
{
    [Test]
    public void ThrowIfLengthOutOfRange_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfLengthOutOfRange(argument, 5, 10);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfLengthOutOfRange_WhenLengthLessThanMinimum_ThrowsArgumentException()
    {
        // Arrange
        var argument = "hi";

        // Act
        void Act() => ArgumentException.ThrowIfLengthOutOfRange(argument, 5, 10);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public void ThrowIfLengthOutOfRange_WhenLengthGreaterThanMaximum_ThrowsArgumentException()
    {
        // Arrange
        var argument = "This is a very long string";

        // Act
        void Act() => ArgumentException.ThrowIfLengthOutOfRange(argument, 5, 10);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfLengthOutOfRange_WhenLengthEqualsMinimum_DoesNotThrow()
    {
        // Arrange
        var argument = "12345";

        // Act & Assert
        ArgumentException.ThrowIfLengthOutOfRange(argument, 5, 10);
        _ = await Assert.That(argument).HasLength().EqualTo(5);
    }

    [Test]
    public async Task ThrowIfLengthOutOfRange_WhenLengthEqualsMaximum_DoesNotThrow()
    {
        // Arrange
        var argument = "1234567890";

        // Act & Assert
        ArgumentException.ThrowIfLengthOutOfRange(argument, 5, 10);
        _ = await Assert.That(argument).HasLength().EqualTo(10);
    }

    [Test]
    public async Task ThrowIfLengthOutOfRange_WhenLengthWithinRange_DoesNotThrow()
    {
        // Arrange
        var argument = "1234567";

        // Act & Assert
        ArgumentException.ThrowIfLengthOutOfRange(argument, 5, 10);
        _ = await Assert.That(argument.Length).IsGreaterThanOrEqualTo(5).And.IsLessThanOrEqualTo(10);
    }
}
