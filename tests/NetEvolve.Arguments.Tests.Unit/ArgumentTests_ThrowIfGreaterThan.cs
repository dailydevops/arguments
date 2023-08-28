namespace NetEvolve.Arguments.Tests.Unit;

using System;
using Xunit;

public sealed partial class ArgumentTests
{
    [Fact]
    public void ThrowIfGreaterThan_WhenArgumentIsGreaterThanMaximum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var argument = 2;
        var maximum = 1;

        // Act
        void Act() => Argument.ThrowIfGreaterThan(argument, maximum);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
    }

    [Fact]
    public void ThrowIfGreaterThan_WhenArgumentIsEqualToMaximum_ReturnsArgument()
    {
        // Arrange
        var argument = 1;
        var maximum = 1;

        // Act
        Argument.ThrowIfGreaterThan(argument, maximum);

        // Assert
        Assert.True(true);
    }
}
