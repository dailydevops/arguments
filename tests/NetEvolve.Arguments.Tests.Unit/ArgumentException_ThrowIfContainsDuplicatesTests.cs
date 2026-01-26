namespace NetEvolve.Arguments.Tests.Unit;

using System;
using System.Collections.Generic;

public sealed class ArgumentException_ThrowIfContainsDuplicatesTests
{
    [Test]
    public void ThrowIfContainsDuplicates_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfContainsDuplicates(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfContainsDuplicates_WhenContainsDuplicates_ThrowsArgumentException()
    {
        // Arrange
        IEnumerable<int> argument = new[] { 1, 2, 3, 2, 4 };

        // Act
        void Act() => ArgumentException.ThrowIfContainsDuplicates(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfContainsDuplicates_WhenNoDuplicates_DoesNotThrow()
    {
        // Arrange
        IEnumerable<int> argument = new[] { 1, 2, 3, 4, 5 };

        // Act & Assert
        ArgumentException.ThrowIfContainsDuplicates(argument);
        _ = await Assert.That(argument).HasCount().EqualTo(5);
    }

    [Test]
    public async Task ThrowIfContainsDuplicates_WhenEmpty_DoesNotThrow()
    {
        // Arrange
        IEnumerable<int> argument = Array.Empty<int>();

        // Act & Assert
        ArgumentException.ThrowIfContainsDuplicates(argument);
        _ = await Assert.That(argument).IsEmpty();
    }

    [Test]
    public void ThrowIfContainsDuplicates_WithComparer_WhenContainsDuplicates_ThrowsArgumentException()
    {
        // Arrange
        IEnumerable<string> argument = new[] { "hello", "HELLO", "world" };

        // Act
        void Act() => ArgumentException.ThrowIfContainsDuplicates(argument, StringComparer.OrdinalIgnoreCase);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfContainsDuplicates_WithComparer_WhenNoDuplicates_DoesNotThrow()
    {
        // Arrange
        IEnumerable<string> argument = new[] { "hello", "HELLO", "world" };

        // Act & Assert
        ArgumentException.ThrowIfContainsDuplicates(argument, StringComparer.Ordinal);
        _ = await Assert.That(argument).HasCount().EqualTo(3);
    }
}
