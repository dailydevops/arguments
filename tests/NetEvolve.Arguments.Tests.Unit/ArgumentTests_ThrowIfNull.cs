namespace NetEvolve.Arguments.Tests.Unit;

using System;

public sealed partial class ArgumentTests
{
    [Test]
    public void ThrowIfNull_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? argument = null;

        // Act
        void Act() => Argument.ThrowIfNull(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfNull_WhenArgumentIsNotEmpty_ReturnsArgument()
    {
        // Arrange
        var argument = "argument";

        // Act
        Argument.ThrowIfNull(argument);

        // Assert
        _ = await Assert.That(argument).IsNotNullOrWhiteSpace();
    }

    [Test]
    public unsafe void ThrowIfNull_WhenArgumentIsNullPointer_ThrowsArgumentNullException()
    {
        // Arrange
        int* argument = null;

        // Act
        void Act() => Argument.ThrowIfNull(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }
}
