namespace NetEvolve.Arguments.Tests.Unit;

using System;

public sealed partial class ArgumentTests
{
    [Test]
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

    [Test]
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

    [Test]
    public async Task ThrowIfGreaterThanOrEqual_WhenArgumentIsLessThanMaximum_ReturnsArgument()
    {
        // Arrange
        var argument = 0;
        var maximum = 1;

        // Act
        Argument.ThrowIfGreaterThanOrEqual(argument, maximum);

        // Assert
        _ = await Assert.That(maximum).IsEqualTo(1);
    }
}
