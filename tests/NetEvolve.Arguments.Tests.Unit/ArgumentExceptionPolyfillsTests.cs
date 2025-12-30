namespace NetEvolve.Arguments.Tests.Unit;

using System;
using System.Collections.Generic;

public sealed class ArgumentExceptionPolyfillsTests
{
    [Test]
    public void ThrowIfNullOrEmpty_String_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfNullOrEmpty_String_WhenArgumentIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var argument = string.Empty;

        // Act
        void Act() => ArgumentException.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfNullOrEmpty_String_WhenArgumentIsValid_DoesNotThrow()
    {
        // Arrange
        var argument = "valid";

        // Act & Assert
        ArgumentException.ThrowIfNullOrEmpty(argument);
        _ = await Assert.That(argument).IsNotNullOrWhiteSpace();
    }

    [Test]
    public void ThrowIfNullOrWhiteSpace_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfNullOrWhiteSpace(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfNullOrWhiteSpace_WhenArgumentIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var argument = string.Empty;

        // Act
        void Act() => ArgumentException.ThrowIfNullOrWhiteSpace(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public void ThrowIfNullOrWhiteSpace_WhenArgumentIsWhiteSpace_ThrowsArgumentException()
    {
        // Arrange
        var argument = "   ";

        // Act
        void Act() => ArgumentException.ThrowIfNullOrWhiteSpace(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    [Arguments("valid")]
    [Arguments("test")]
    [Arguments("a")]
    public async Task ThrowIfNullOrWhiteSpace_WhenArgumentIsValid_DoesNotThrow(string argument)
    {
        // Act & Assert
        ArgumentException.ThrowIfNullOrWhiteSpace(argument);
        _ = await Assert.That(argument).IsNotNullOrWhiteSpace();
    }

    [Test]
    public void ThrowIfNullOrEmpty_Enumerable_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfNullOrEmpty_Enumerable_WhenArgumentIsEmptyArray_ThrowsArgumentException()
    {
        // Arrange
        var argument = Array.Empty<int>();

        // Act
        void Act() => ArgumentException.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public void ThrowIfNullOrEmpty_Enumerable_WhenArgumentIsEmptyList_ThrowsArgumentException()
    {
        // Arrange
        var argument = new List<string>();

        // Act
        void Act() => ArgumentException.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfNullOrEmpty_Enumerable_WhenArgumentHasElements_DoesNotThrow()
    {
        // Arrange
        var argument = new[] { 1, 2, 3 };

        // Act & Assert
        ArgumentException.ThrowIfNullOrEmpty(argument);
        _ = await Assert.That(argument).HasCount().EqualTo(3);
    }

    [Test]
    public async Task ThrowIfNullOrEmpty_Enumerable_WhenArgumentIsSingleElement_DoesNotThrow()
    {
        // Arrange
        var argument = new List<string> { "item" };

        // Act & Assert
        ArgumentException.ThrowIfNullOrEmpty(argument);
        _ = await Assert.That(argument).HasCount().EqualTo(1);
    }
}
