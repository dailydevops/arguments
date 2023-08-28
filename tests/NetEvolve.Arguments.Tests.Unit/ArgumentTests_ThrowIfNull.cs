namespace NetEvolve.Arguments.Tests.Unit;

using System;
using Xunit;

public sealed partial class ArgumentTests
{
    [Fact]
    public void ThrowIfNull_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? argument = null;

        // Act
        void Act() => Argument.ThrowIfNull(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Fact]
    public void ThrowIfNull_WhenArgumentIsNotEmpty_ReturnsArgument()
    {
        // Arrange
        var argument = "argument";

        // Act
        Argument.ThrowIfNull(argument);

        // Assert
        Assert.True(true);
    }
}
