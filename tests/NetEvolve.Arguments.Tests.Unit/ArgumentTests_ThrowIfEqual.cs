namespace NetEvolve.Arguments.Tests.Unit;

using System;
using System.Threading.Tasks;

public sealed partial class ArgumentTests
{
    [Test]
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

    [Test]
    public async Task ThrowIfEqual_WhenArgumentIsNotEqualToMaximum_ReturnsArgument()
    {
        // Arrange
        var argument = 2;
        var maximum = 1;

        // Act
        Argument.ThrowIfEqual(argument, maximum);

        _ = await Assert.That(maximum).IsEqualTo(1);
    }
}
