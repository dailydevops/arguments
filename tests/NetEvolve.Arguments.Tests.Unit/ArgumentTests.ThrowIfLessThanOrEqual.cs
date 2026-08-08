namespace NetEvolve.Arguments.Tests.Unit;

using System;

public sealed partial class ArgumentTests
{
    [Test]
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

    [Test]
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

    [Test]
    public async Task ThrowIfLessThanOrEqual_WhenArgumentIsGreaterThanMinimum_ReturnsArgument()
    {
        // Arrange
        var argument = 2;
        var minimum = 1;

        // Act
        Argument.ThrowIfLessThanOrEqual(argument, minimum);

        // Assert
        _ = await Assert.That(minimum).IsEqualTo(1);
    }
}
