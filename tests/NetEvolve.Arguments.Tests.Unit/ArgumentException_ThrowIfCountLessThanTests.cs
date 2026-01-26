namespace NetEvolve.Arguments.Tests.Unit;

using System;
using System.Collections.Generic;

public sealed class ArgumentException_ThrowIfCountLessThanTests
{
    [Test]
    public void ThrowIfCountLessThan_IEnumerable_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfCountLessThan(argument, 5);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfCountLessThan_IEnumerable_WhenCountLessThanMinimum_ThrowsArgumentException()
    {
        // Arrange
        IEnumerable<int> argument = [1, 2];

        // Act
        void Act() => ArgumentException.ThrowIfCountLessThan(argument, 5);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfCountLessThan_IEnumerable_WhenCountEqualsMinimum_DoesNotThrow()
    {
        // Arrange
        IEnumerable<int> argument = [1, 2, 3, 4, 5];

        // Act & Assert
        ArgumentException.ThrowIfCountLessThan(argument, 5);
        _ = await Assert.That(argument).HasCount().EqualTo(5);
    }

    [Test]
    public async Task ThrowIfCountLessThan_IEnumerable_WhenCountGreaterThanMinimum_DoesNotThrow()
    {
        // Arrange
        IEnumerable<int> argument = [1, 2, 3, 4, 5, 6, 7];

        // Act & Assert
        ArgumentException.ThrowIfCountLessThan(argument, 5);
        _ = await Assert.That(argument.Count()).IsGreaterThan(5);
    }

    [Test]
    public void ThrowIfCountLessThan_ICollection_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        ICollection<int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfCountLessThan(argument, 5);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfCountLessThan_ICollection_WhenCountGreaterThanMinimum_DoesNotThrow()
    {
        // Arrange
        List<int> argument = [1, 2, 3, 4, 5, 6];

        // Act & Assert
        ArgumentException.ThrowIfCountLessThan((ICollection<int>)argument, 5);
        _ = await Assert.That(argument.Count).IsGreaterThan(5);
    }

    [Test]
    public void ThrowIfCountLessThan_IReadOnlyCollection_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        IReadOnlyCollection<int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfCountLessThan(argument, 5);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfCountLessThan_IReadOnlyCollection_WhenCountGreaterThanMinimum_DoesNotThrow()
    {
        // Arrange
        List<int> argument = [1, 2, 3, 4, 5, 6];

        // Act & Assert
        ArgumentException.ThrowIfCountLessThan((IReadOnlyCollection<int>)argument, 5);
        _ = await Assert.That(argument.Count).IsGreaterThan(5);
    }

    [Test]
    public void ThrowIfCountLessThan_Array_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        int[]? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfCountLessThan(argument, 5);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfCountLessThan_Array_WhenCountGreaterThanMinimum_DoesNotThrow()
    {
        // Arrange
        int[] argument = [1, 2, 3, 4, 5, 6];

        // Act & Assert
        ArgumentException.ThrowIfCountLessThan(argument, 5);
        _ = await Assert.That(argument.Length).IsGreaterThan(5);
    }
}
