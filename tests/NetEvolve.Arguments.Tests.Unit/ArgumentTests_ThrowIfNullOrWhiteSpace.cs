namespace NetEvolve.Arguments.Tests.Unit;

using System;
using System.Threading.Tasks;

public sealed partial class ArgumentTests
{
    [Test]
    public void ThrowIfNullOrWhiteSpace_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? argument = null;

        // Act
        void Act() => Argument.ThrowIfNullOrWhiteSpace(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfNullOrWhiteSpace_WhenArgumentIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var argument = string.Empty;

        // Act
        void Act() => Argument.ThrowIfNullOrWhiteSpace(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public void ThrowIfNullOrWhiteSpace_WhenArgumentIsWhiteSpace_ThrowsArgumentException()
    {
        // Arrange
        var argument = " ";

        // Act
        void Act() => Argument.ThrowIfNullOrWhiteSpace(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfNullOrWhiteSpace_WhenArgumentIsNotEmpty_ReturnsArgument()
    {
        // Arrange
        var argument = "argument";

        // Act
        Argument.ThrowIfNullOrWhiteSpace(argument);

        // Assert
        _ = await Assert.That(argument).IsNotNullOrWhiteSpace();
    }
}
