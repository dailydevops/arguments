namespace NetEvolve.Arguments.Tests.Unit;

using System;
using Xunit;

public sealed partial class ArgumentTests
{
    [Fact]
    public void ThrowIfLessThanOrEqual_WhenArgumentIsLessThanMinimum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var argument = 0;
        var minimum = 1;

        // Act
        void Act() => Argument.ThrowIfLessThanOrEqual(argument, minimum);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
    }

    [Fact]
    public void ThrowIfLessThanOrEqual_WhenArgumentIsEqualToMinimum_ReturnsArgument()
    {
        // Arrange
        var argument = 1;
        var minimum = 1;

        // Act
        void Act() => Argument.ThrowIfLessThanOrEqual(argument, minimum);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
    }

    [Fact]
    public void ThrowIfLessThanOrEqual_WhenArgumentIsGreaterThanMinimum_ReturnsArgument()
    {
        // Arrange
        var argument = 2;
        var minimum = 1;

        // Act
        Argument.ThrowIfLessThanOrEqual(argument, minimum);

        // Assert
    }
}
