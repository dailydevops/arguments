namespace NetEvolve.Arguments.Tests.Unit;

using System;
using Xunit;

public sealed partial class ArgumentTests
{
    [Fact]
    public void ThrowIfLessThan_WhenArgumentIsLessThanMinimum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var argument = 0;
        var minimum = 1;

        // Act
        void Act() => Argument.ThrowIfLessThan(argument, minimum);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
    }

    [Fact]
    public void ThrowIfLessThan_WhenArgumentIsEqualToMinimum_ReturnsArgument()
    {
        // Arrange
        var argument = 1;
        var minimum = 1;

        // Act
        Argument.ThrowIfLessThan(argument, minimum);

        // Assert
        Assert.True(true);
    }
}
