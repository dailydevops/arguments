namespace NetEvolve.Arguments.Tests.Unit;

using System;
using Xunit;

public sealed partial class ArgumentTests
{
    [Fact]
    public void ThrowIfNullOrWhiteSpace_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? argument = null;

        // Act
        void Act() => Argument.ThrowIfNullOrWhiteSpace(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WhenArgumentIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var argument = string.Empty;

        // Act
        void Act() => Argument.ThrowIfNullOrWhiteSpace(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WhenArgumentIsWhiteSpace_ThrowsArgumentException()
    {
        // Arrange
        var argument = " ";

        // Act
        void Act() => Argument.ThrowIfNullOrWhiteSpace(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_WhenArgumentIsNotEmpty_ReturnsArgument()
    {
        // Arrange
        var argument = "argument";

        // Act
        Argument.ThrowIfNullOrWhiteSpace(argument);

        // Assert
        Assert.True(true);
    }
}
