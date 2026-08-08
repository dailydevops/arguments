namespace NetEvolve.Arguments.Tests.Unit;

using System;

public sealed class ArgumentException_ThrowIfContainsWhiteSpaceTests
{
    [Test]
    public void ThrowIfContainsWhiteSpace_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfContainsWhiteSpace(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfContainsWhiteSpace_WhenContainsSpace_ThrowsArgumentException()
    {
        // Arrange
        var argument = "hello world";

        // Act
        void Act() => ArgumentException.ThrowIfContainsWhiteSpace(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public void ThrowIfContainsWhiteSpace_WhenContainsTab_ThrowsArgumentException()
    {
        // Arrange
        var argument = "hello\tworld";

        // Act
        void Act() => ArgumentException.ThrowIfContainsWhiteSpace(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public void ThrowIfContainsWhiteSpace_WhenContainsNewline_ThrowsArgumentException()
    {
        // Arrange
        var argument = "hello\nworld";

        // Act
        void Act() => ArgumentException.ThrowIfContainsWhiteSpace(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfContainsWhiteSpace_WhenNoWhiteSpace_DoesNotThrow()
    {
        // Arrange
        var argument = "helloworld";

        // Act & Assert
        ArgumentException.ThrowIfContainsWhiteSpace(argument);
        _ = await Assert.That(argument).IsEqualTo("helloworld");
    }

    [Test]
    public async Task ThrowIfContainsWhiteSpace_WhenEmptyString_DoesNotThrow()
    {
        // Arrange
        var argument = string.Empty;

        // Act & Assert
        ArgumentException.ThrowIfContainsWhiteSpace(argument);
        _ = await Assert.That(argument).IsEmpty();
    }
}
