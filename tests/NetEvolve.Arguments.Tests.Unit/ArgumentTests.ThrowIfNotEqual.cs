namespace NetEvolve.Arguments.Tests.Unit;

using System;

public sealed partial class ArgumentTests
{
    [Test]
    public void ThrowIfNotEqual_WhenArgumentIsNotEqualToMaximum_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var argument = 2;
        var maximum = 1;

        // Act
        void Act() => Argument.ThrowIfNotEqual(argument, maximum);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfNotEqual_WhenArgumentIsEqualToMaximum_ReturnsArgument()
    {
        // Arrange
        var argument = 1;
        var maximum = 1;

        // Act
        Argument.ThrowIfNotEqual(argument, maximum);

        // Assert
        _ = await Assert.That(maximum).IsEqualTo(1);
    }
}
