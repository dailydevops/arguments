namespace NetEvolve.Arguments.Tests.Unit;

using System;
using System.Threading.Tasks;

public sealed partial class ArgumentTests
{
    [Test]
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

    [Test]
    public async Task ThrowIfGreaterThan_WhenArgumentIsEqualToMaximum_ReturnsArgument()
    {
        // Arrange
        var argument = 1;
        var maximum = 1;

        // Act
        Argument.ThrowIfGreaterThan(argument, maximum);

        // Assert
        _ = await Assert.That(maximum).IsEqualTo(1);
    }
}
