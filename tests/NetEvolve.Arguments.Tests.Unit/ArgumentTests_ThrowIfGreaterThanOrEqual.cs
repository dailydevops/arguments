namespace NetEvolve.Arguments.Tests.Unit;

using System;
using Xunit;

public sealed partial class ArgumentTests
{
    [Fact]
    public void ThrowIfGreaterThanOrEqual_WhenArgumentIsGreaterThanOrEqualToMaximum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var argument = 2;
        var maximum = 1;

        // Act
        void Act() => Argument.ThrowIfGreaterThanOrEqual(argument, maximum);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
    }

    [Fact]
    public void ThrowIfGreaterThanOrEqual_WhenArgumentIsEqualToMaximum_ReturnsArgument()
    {
        // Arrange
        var argument = 1;
        var maximum = 1;

        // Act
        void Act() => Argument.ThrowIfGreaterThanOrEqual(argument, maximum);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
    }

    [Fact]
    public void ThrowIfGreaterThanOrEqual_WhenArgumentIsLessThanMaximum_ReturnsArgument()
    {
        // Arrange
        var argument = 0;
        var maximum = 1;

        // Act
        Argument.ThrowIfGreaterThanOrEqual(argument, maximum);

        // Assert
    }
}
