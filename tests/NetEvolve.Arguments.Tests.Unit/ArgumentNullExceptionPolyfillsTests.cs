namespace NetEvolve.Arguments.Tests.Unit;

using System;

public sealed class ArgumentNullExceptionPolyfillsTests
{
    [Test]
    public void ThrowIfNull_Object_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        object? argument = null;

        // Act
        void Act() => ArgumentNullException.ThrowIfNull(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfNull_Object_WhenArgumentIsNotNull_DoesNotThrow()
    {
        // Arrange
        var argument = new object();

        // Act & Assert
        ArgumentNullException.ThrowIfNull(argument);
    }

    [Test]
    public void ThrowIfNull_String_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? argument = null;

        // Act
        void Act() => ArgumentNullException.ThrowIfNull(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfNull_String_WhenArgumentIsNotNull_DoesNotThrow()
    {
        // Arrange
        var argument = "test";

        // Act & Assert
        ArgumentNullException.ThrowIfNull(argument);
        _ = await Assert.That(argument).IsNotNull();
    }

    [Test]
    public unsafe void ThrowIfNull_Pointer_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        int* argument = null;

        // Act
        void Act() => ArgumentNullException.ThrowIfNull(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public unsafe void ThrowIfNull_Pointer_WhenArgumentIsNotNull_DoesNotThrow()
    {
        // Arrange
        var value = 42;
        var argument = &value;

        // Act & Assert
        ArgumentNullException.ThrowIfNull(argument);
    }

    [Test]
    public async Task ThrowIfNull_ReferenceType_WhenArgumentIsNotNull_DoesNotThrow()
    {
        // Arrange
        var argument = new object();

        // Act & Assert
        ArgumentNullException.ThrowIfNull(argument);
    }

    [Test]
    public async Task ThrowIfNull_ValueType_WhenArgumentHasValue_DoesNotThrow()
    {
        // Arrange
        var argument = (object)123;

        // Act & Assert
        ArgumentNullException.ThrowIfNull(argument);
    }
}
