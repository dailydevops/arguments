namespace NetEvolve.Arguments.Tests.Unit;

using System;
using Xunit;

public sealed partial class ArgumentTests
{
    [Fact]
    public void ThrowIfEqual_WhenArgumentIsEqualToMaximum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var argument = 1;
        var maximum = 1;

        // Act
        void Act() => Argument.ThrowIfEqual(argument, maximum);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
    }

    [Fact]
    public void ThrowIfEqual_WhenArgumentIsNotEqualToMaximum_ReturnsArgument()
    {
        // Arrange
        var argument = 2;
        var maximum = 1;

        // Act
        Argument.ThrowIfEqual(argument, maximum);

        // Assert
        Assert.True(true);
    }
}
