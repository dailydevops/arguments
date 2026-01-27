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
    public void ThrowIfNullOrEmpty_Array_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        int[]? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfNullOrEmpty_ReadOnly_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        IReadOnlyCollection<int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfNullOrEmpty_Collection_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        ICollection<int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfNullOrEmpty_Enumerable_WhenArgumentIsEmptyArray_ThrowsArgumentException()
    {
        // Arrange
        IEnumerable<int> argument = Array.Empty<int>();

        // Act
        void Act() => ArgumentException.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public void ThrowIfNullOrEmpty_Array_WhenArgumentIsEmptyArray_ThrowsArgumentException()
    {
        // Arrange
        var argument = Array.Empty<int>();

        // Act
        void Act() => ArgumentException.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public void ThrowIfNullOrEmpty_Collection_WhenArgumentIsEmptyArray_ThrowsArgumentException()
    {
        // Arrange
        ICollection<int> argument = Array.Empty<int>();

        // Act
        void Act() => ArgumentException.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public void ThrowIfNullOrEmpty_ReadOnly_WhenArgumentIsEmptyArray_ThrowsArgumentException()
    {
        // Arrange
        IReadOnlyCollection<int> argument = Array.Empty<int>();

        // Act
        void Act() => ArgumentException.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public void ThrowIfNullOrEmpty_Enumerable_WhenArgumentIsEmptyList_ThrowsArgumentException()
    {
        // Arrange
        IEnumerable<string> argument = new List<string>();

        // Act
        void Act() => ArgumentException.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public void ThrowIfNullOrEmpty_Collection_WhenArgumentIsEmptyList_ThrowsArgumentException()
    {
        // Arrange
        ICollection<string> argument = new List<string>();

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
        IEnumerable<string> argument = new List<string> { "item" };

        // Act & Assert
        ArgumentException.ThrowIfNullOrEmpty(argument);
        _ = await Assert.That(argument).HasCount().EqualTo(1);
    }
}
