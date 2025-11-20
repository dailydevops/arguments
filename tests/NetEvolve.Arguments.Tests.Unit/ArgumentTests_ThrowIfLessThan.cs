namespace NetEvolve.Arguments.Tests.Unit;

using System;

public sealed partial class ArgumentTests
{
    [Test]
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

    [Test]
    public async Task ThrowIfLessThan_WhenArgumentIsEqualToMinimum_ReturnsArgument()
    {
        // Arrange
        var argument = 1;
        var minimum = 1;

        // Act
        Argument.ThrowIfLessThan(argument, minimum);

        // Assert
        _ = await Assert.That(minimum).IsEqualTo(1);
    }
}
