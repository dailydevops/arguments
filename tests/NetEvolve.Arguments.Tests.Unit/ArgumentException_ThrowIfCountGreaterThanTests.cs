namespace NetEvolve.Arguments.Tests.Unit;

using System;
using System.Collections.Generic;

public sealed class ArgumentException_ThrowIfCountGreaterThanTests
{
    [Test]
    public void ThrowIfCountGreaterThan_IEnumerable_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfCountGreaterThan(argument, 5);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfCountGreaterThan_IEnumerable_WhenCountExceedsMaximum_ThrowsArgumentException()
    {
        // Arrange
        IEnumerable<int> argument = new[] { 1, 2, 3, 4, 5, 6 };

        // Act
        void Act() => ArgumentException.ThrowIfCountGreaterThan(argument, 5);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfCountGreaterThan_IEnumerable_WhenCountEqualsMaximum_DoesNotThrow()
    {
        // Arrange
        IEnumerable<int> argument = new[] { 1, 2, 3, 4, 5 };

        // Act & Assert
        ArgumentException.ThrowIfCountGreaterThan(argument, 5);
        _ = await Assert.That(argument).HasCount().EqualTo(5);
    }

    [Test]
    public async Task ThrowIfCountGreaterThan_IEnumerable_WhenCountLessThanMaximum_DoesNotThrow()
    {
        // Arrange
        IEnumerable<int> argument = new[] { 1, 2, 3 };

        // Act & Assert
        ArgumentException.ThrowIfCountGreaterThan(argument, 5);
        _ = await Assert.That(argument).HasCount().LessThan(5);
    }

    [Test]
    public void ThrowIfCountGreaterThan_ICollection_WhenCountExceedsMaximum_ThrowsArgumentException()
    {
        // Arrange
        ICollection<int> argument = new List<int> { 1, 2, 3, 4, 5, 6 };

        // Act
        void Act() => ArgumentException.ThrowIfCountGreaterThan(argument, 5);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public void ThrowIfCountGreaterThan_IReadOnlyCollection_WhenCountExceedsMaximum_ThrowsArgumentException()
    {
        // Arrange
        IReadOnlyCollection<int> argument = new[] { 1, 2, 3, 4, 5, 6 };

        // Act
        void Act() => ArgumentException.ThrowIfCountGreaterThan(argument, 5);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public void ThrowIfCountGreaterThan_Array_WhenCountExceedsMaximum_ThrowsArgumentException()
    {
        // Arrange
        var argument = new[] { 1, 2, 3, 4, 5, 6 };

        // Act
        void Act() => ArgumentException.ThrowIfCountGreaterThan(argument, 5);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }
}
