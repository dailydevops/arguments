namespace NetEvolve.Arguments.Tests.Unit;

using System;
using Xunit;

public sealed partial class ArgumentTests
{
    [Fact]
    public void ThrowIfNullOrEmpty_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? argument = null;

        // Act
        void Act() => Argument.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Fact]
    public void ThrowIfNullOrEmpty_WhenArgumentIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var argument = string.Empty;

        // Act
        void Act() => Argument.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Fact]
    public void ThrowIfNullOrEmpty_WhenArgumentIsNotEmpty_ReturnsArgument()
    {
        // Arrange
        var argument = "argument";

        // Act
        Argument.ThrowIfNullOrEmpty(argument);

        // Assert
    }
}
