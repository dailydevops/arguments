namespace NetEvolve.Arguments.Tests.Unit;

using System;

public sealed class ArgumentException_ThrowIfLengthGreaterThanTests
{
    [Test]
    public void ThrowIfLengthGreaterThan_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfLengthGreaterThan(argument, 10);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfLengthGreaterThan_WhenLengthExceedsMaximum_ThrowsArgumentException()
    {
        // Arrange
        var argument = "This is a long string";

        // Act
        void Act() => ArgumentException.ThrowIfLengthGreaterThan(argument, 10);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfLengthGreaterThan_WhenLengthEqualsMaximum_DoesNotThrow()
    {
        // Arrange
        var argument = "1234567890";

        // Act & Assert
        ArgumentException.ThrowIfLengthGreaterThan(argument, 10);
        _ = await Assert.That(argument).HasLength().EqualTo(10);
    }

    [Test]
    public async Task ThrowIfLengthGreaterThan_WhenLengthLessThanMaximum_DoesNotThrow()
    {
        // Arrange
        var argument = "short";

        // Act & Assert
        ArgumentException.ThrowIfLengthGreaterThan(argument, 10);
        _ = await Assert.That(argument.Length).IsLessThan(10);
    }

    [Test]
    public async Task ThrowIfLengthGreaterThan_WhenEmptyString_DoesNotThrow()
    {
        // Arrange
        var argument = string.Empty;

        // Act & Assert
        ArgumentException.ThrowIfLengthGreaterThan(argument, 10);
        _ = await Assert.That(argument).HasLength().EqualTo(0);
    }
}
