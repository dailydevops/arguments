namespace NetEvolve.Arguments.Tests.Unit;

using System;

public sealed class ArgumentException_ThrowIfDefaultTests
{
    [Test]
    public void ThrowIfDefault_Struct_WhenValueIsDefault_ThrowsArgumentException()
    {
        // Arrange
        int argument = default;

        // Act
        void Act() => ArgumentException.ThrowIfDefault(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfDefault_Struct_WhenValueIsNotDefault_DoesNotThrow()
    {
        // Arrange
        var argument = 42;

        // Act & Assert
        ArgumentException.ThrowIfDefault(argument);
        _ = await Assert.That(argument).IsEqualTo(42);
    }

    [Test]
    public void ThrowIfDefault_Guid_WhenValueIsDefault_ThrowsArgumentException()
    {
        // Arrange
        Guid argument = default;

        // Act
        void Act() => ArgumentException.ThrowIfDefault(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfDefault_Guid_WhenValueIsNotDefault_DoesNotThrow()
    {
        // Arrange
        var argument = Guid.NewGuid();

        // Act & Assert
        ArgumentException.ThrowIfDefault(argument);
        _ = await Assert.That(argument).IsNotEqualTo(Guid.Empty);
    }

    [Test]
    public void ThrowIfEmptyGuid_WhenValueIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var argument = Guid.Empty;

        // Act
        void Act() => ArgumentException.ThrowIfEmptyGuid(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfEmptyGuid_WhenValueIsNotEmpty_DoesNotThrow()
    {
        // Arrange
        var argument = Guid.NewGuid();

        // Act & Assert
        ArgumentException.ThrowIfEmptyGuid(argument);
        _ = await Assert.That(argument).IsNotEqualTo(Guid.Empty);
    }
}
