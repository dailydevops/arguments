namespace NetEvolve.Arguments.Tests.Unit;

using System;
using System.Collections.Generic;

public sealed class ArgumentException_ThrowIfCountOutOfRangeTests
{
    [Test]
    public void ThrowIfCountOutOfRange_IEnumerable_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfCountOutOfRange(argument, 3, 7);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfCountOutOfRange_IEnumerable_WhenCountBelowMinimum_ThrowsArgumentException()
    {
        // Arrange
        IEnumerable<int> argument = [1, 2];

        // Act
        void Act() => ArgumentException.ThrowIfCountOutOfRange(argument, 3, 7);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public void ThrowIfCountOutOfRange_IEnumerable_WhenCountAboveMaximum_ThrowsArgumentException()
    {
        // Arrange
        IEnumerable<int> argument = [1, 2, 3, 4, 5, 6, 7, 8, 9];

        // Act
        void Act() => ArgumentException.ThrowIfCountOutOfRange(argument, 3, 7);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfCountOutOfRange_IEnumerable_WhenCountEqualsMinimum_DoesNotThrow()
    {
        // Arrange
        IEnumerable<int> argument = [1, 2, 3];

        // Act & Assert
        ArgumentException.ThrowIfCountOutOfRange(argument, 3, 7);
        _ = await Assert.That(argument).HasCount().EqualTo(3);
    }

    [Test]
    public async Task ThrowIfCountOutOfRange_IEnumerable_WhenCountEqualsMaximum_DoesNotThrow()
    {
        // Arrange
        IEnumerable<int> argument = [1, 2, 3, 4, 5, 6, 7];

        // Act & Assert
        ArgumentException.ThrowIfCountOutOfRange(argument, 3, 7);
        _ = await Assert.That(argument).HasCount().EqualTo(7);
    }

    [Test]
    public async Task ThrowIfCountOutOfRange_IEnumerable_WhenCountWithinRange_DoesNotThrow()
    {
        // Arrange
        IEnumerable<int> argument = [1, 2, 3, 4, 5];

        // Act & Assert
        ArgumentException.ThrowIfCountOutOfRange(argument, 3, 7);
        _ = await Assert.That(argument.Count()).IsGreaterThanOrEqualTo(3).And.IsLessThanOrEqualTo(7);
    }

    [Test]
    public void ThrowIfCountOutOfRange_ICollection_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        ICollection<int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfCountOutOfRange(argument, 3, 7);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfCountOutOfRange_ICollection_WhenCountWithinRange_DoesNotThrow()
    {
        // Arrange
        List<int> argument = [1, 2, 3, 4, 5];

        // Act & Assert
        ArgumentException.ThrowIfCountOutOfRange((ICollection<int>)argument, 3, 7);
        _ = await Assert.That(argument.Count).IsGreaterThanOrEqualTo(3).And.IsLessThanOrEqualTo(7);
    }

    [Test]
    public void ThrowIfCountOutOfRange_IReadOnlyCollection_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        IReadOnlyCollection<int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfCountOutOfRange(argument, 3, 7);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfCountOutOfRange_IReadOnlyCollection_WhenCountWithinRange_DoesNotThrow()
    {
        // Arrange
        List<int> argument = [1, 2, 3, 4, 5];

        // Act & Assert
        ArgumentException.ThrowIfCountOutOfRange((IReadOnlyCollection<int>)argument, 3, 7);
        _ = await Assert.That(argument.Count).IsGreaterThanOrEqualTo(3).And.IsLessThanOrEqualTo(7);
    }

    [Test]
    public void ThrowIfCountOutOfRange_Array_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        int[]? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfCountOutOfRange(argument, 3, 7);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfCountOutOfRange_Array_WhenCountWithinRange_DoesNotThrow()
    {
        // Arrange
        int[] argument = [1, 2, 3, 4, 5];

        // Act & Assert
        ArgumentException.ThrowIfCountOutOfRange(argument, 3, 7);
        _ = await Assert.That(argument.Length).IsGreaterThanOrEqualTo(3).And.IsLessThanOrEqualTo(7);
    }
}
