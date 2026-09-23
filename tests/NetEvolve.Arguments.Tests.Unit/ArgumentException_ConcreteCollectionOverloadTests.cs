namespace NetEvolve.Arguments.Tests.Unit;

using System;
using System.Collections.Generic;

/// <summary>
/// Verifies that concrete collection types implementing both <see cref="ICollection{T}"/> and
/// <see cref="IReadOnlyCollection{T}"/> resolve to a single overload without casting.
/// </summary>
public sealed class ArgumentException_ConcreteCollectionOverloadTests
{
    [Test]
    public void ThrowIfNullOrEmpty_List_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        List<int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfNullOrEmpty_List_WhenEmpty_ThrowsArgumentException()
    {
        // Arrange
        List<int> argument = [];

        // Act
        void Act() => ArgumentException.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfNullOrEmpty_List_WhenValid_DoesNotThrow()
    {
        // Arrange
        List<int> argument = [1, 2, 3];

        // Act & Assert
        ArgumentException.ThrowIfNullOrEmpty(argument);
        _ = await Assert.That(argument.Count).IsEqualTo(3);
    }

    [Test]
    public void ThrowIfNullOrEmpty_HashSet_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        HashSet<int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfNullOrEmpty_HashSet_WhenEmpty_ThrowsArgumentException()
    {
        // Arrange
        HashSet<int> argument = [];

        // Act
        void Act() => ArgumentException.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfNullOrEmpty_HashSet_WhenValid_DoesNotThrow()
    {
        // Arrange
        HashSet<int> argument = [1, 2, 3];

        // Act & Assert
        ArgumentException.ThrowIfNullOrEmpty(argument);
        _ = await Assert.That(argument.Count).IsEqualTo(3);
    }

    [Test]
    public void ThrowIfNullOrEmpty_Dictionary_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        Dictionary<int, int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfNullOrEmpty_Dictionary_WhenEmpty_ThrowsArgumentException()
    {
        // Arrange
        Dictionary<int, int> argument = [];

        // Act
        void Act() => ArgumentException.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfNullOrEmpty_Dictionary_WhenValid_DoesNotThrow()
    {
        // Arrange
        Dictionary<int, int> argument = new()
        {
            [1] = 1,
            [2] = 2,
            [3] = 3,
        };

        // Act & Assert
        ArgumentException.ThrowIfNullOrEmpty(argument);
        _ = await Assert.That(argument.Count).IsEqualTo(3);
    }

    [Test]
    public void ThrowIfCountGreaterThan_List_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        List<int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfCountGreaterThan(argument, 2);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfCountGreaterThan_List_WhenCountExceedsMaximum_ThrowsArgumentException()
    {
        // Arrange
        List<int> argument = [1, 2, 3];

        // Act
        void Act() => ArgumentException.ThrowIfCountGreaterThan(argument, 2);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfCountGreaterThan_List_WhenValid_DoesNotThrow()
    {
        // Arrange
        List<int> argument = [1, 2, 3];

        // Act & Assert
        ArgumentException.ThrowIfCountGreaterThan(argument, 3);
        _ = await Assert.That(argument.Count).IsEqualTo(3);
    }

    [Test]
    public void ThrowIfCountGreaterThan_HashSet_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        HashSet<int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfCountGreaterThan(argument, 2);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfCountGreaterThan_HashSet_WhenCountExceedsMaximum_ThrowsArgumentException()
    {
        // Arrange
        HashSet<int> argument = [1, 2, 3];

        // Act
        void Act() => ArgumentException.ThrowIfCountGreaterThan(argument, 2);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfCountGreaterThan_HashSet_WhenValid_DoesNotThrow()
    {
        // Arrange
        HashSet<int> argument = [1, 2, 3];

        // Act & Assert
        ArgumentException.ThrowIfCountGreaterThan(argument, 3);
        _ = await Assert.That(argument.Count).IsEqualTo(3);
    }

    [Test]
    public void ThrowIfCountGreaterThan_Dictionary_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        Dictionary<int, int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfCountGreaterThan(argument, 2);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfCountGreaterThan_Dictionary_WhenCountExceedsMaximum_ThrowsArgumentException()
    {
        // Arrange
        Dictionary<int, int> argument = new()
        {
            [1] = 1,
            [2] = 2,
            [3] = 3,
        };

        // Act
        void Act() => ArgumentException.ThrowIfCountGreaterThan(argument, 2);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfCountGreaterThan_Dictionary_WhenValid_DoesNotThrow()
    {
        // Arrange
        Dictionary<int, int> argument = new()
        {
            [1] = 1,
            [2] = 2,
            [3] = 3,
        };

        // Act & Assert
        ArgumentException.ThrowIfCountGreaterThan(argument, 3);
        _ = await Assert.That(argument.Count).IsEqualTo(3);
    }

    [Test]
    public void ThrowIfCountLessThan_List_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        List<int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfCountLessThan(argument, 4);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfCountLessThan_List_WhenCountBelowMinimum_ThrowsArgumentException()
    {
        // Arrange
        List<int> argument = [1, 2, 3];

        // Act
        void Act() => ArgumentException.ThrowIfCountLessThan(argument, 4);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfCountLessThan_List_WhenValid_DoesNotThrow()
    {
        // Arrange
        List<int> argument = [1, 2, 3];

        // Act & Assert
        ArgumentException.ThrowIfCountLessThan(argument, 3);
        _ = await Assert.That(argument.Count).IsEqualTo(3);
    }

    [Test]
    public void ThrowIfCountLessThan_HashSet_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        HashSet<int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfCountLessThan(argument, 4);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfCountLessThan_HashSet_WhenCountBelowMinimum_ThrowsArgumentException()
    {
        // Arrange
        HashSet<int> argument = [1, 2, 3];

        // Act
        void Act() => ArgumentException.ThrowIfCountLessThan(argument, 4);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfCountLessThan_HashSet_WhenValid_DoesNotThrow()
    {
        // Arrange
        HashSet<int> argument = [1, 2, 3];

        // Act & Assert
        ArgumentException.ThrowIfCountLessThan(argument, 3);
        _ = await Assert.That(argument.Count).IsEqualTo(3);
    }

    [Test]
    public void ThrowIfCountLessThan_Dictionary_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        Dictionary<int, int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfCountLessThan(argument, 4);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfCountLessThan_Dictionary_WhenCountBelowMinimum_ThrowsArgumentException()
    {
        // Arrange
        Dictionary<int, int> argument = new()
        {
            [1] = 1,
            [2] = 2,
            [3] = 3,
        };

        // Act
        void Act() => ArgumentException.ThrowIfCountLessThan(argument, 4);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfCountLessThan_Dictionary_WhenValid_DoesNotThrow()
    {
        // Arrange
        Dictionary<int, int> argument = new()
        {
            [1] = 1,
            [2] = 2,
            [3] = 3,
        };

        // Act & Assert
        ArgumentException.ThrowIfCountLessThan(argument, 3);
        _ = await Assert.That(argument.Count).IsEqualTo(3);
    }

    [Test]
    public void ThrowIfCountOutOfRange_List_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        List<int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfCountOutOfRange(argument, 4, 5);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfCountOutOfRange_List_WhenCountOutOfRange_ThrowsArgumentException()
    {
        // Arrange
        List<int> argument = [1, 2, 3];

        // Act
        void Act() => ArgumentException.ThrowIfCountOutOfRange(argument, 4, 5);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfCountOutOfRange_List_WhenValid_DoesNotThrow()
    {
        // Arrange
        List<int> argument = [1, 2, 3];

        // Act & Assert
        ArgumentException.ThrowIfCountOutOfRange(argument, 1, 3);
        _ = await Assert.That(argument.Count).IsEqualTo(3);
    }

    [Test]
    public void ThrowIfCountOutOfRange_HashSet_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        HashSet<int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfCountOutOfRange(argument, 4, 5);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfCountOutOfRange_HashSet_WhenCountOutOfRange_ThrowsArgumentException()
    {
        // Arrange
        HashSet<int> argument = [1, 2, 3];

        // Act
        void Act() => ArgumentException.ThrowIfCountOutOfRange(argument, 4, 5);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfCountOutOfRange_HashSet_WhenValid_DoesNotThrow()
    {
        // Arrange
        HashSet<int> argument = [1, 2, 3];

        // Act & Assert
        ArgumentException.ThrowIfCountOutOfRange(argument, 1, 3);
        _ = await Assert.That(argument.Count).IsEqualTo(3);
    }

    [Test]
    public void ThrowIfCountOutOfRange_Dictionary_WhenNull_ThrowsArgumentNullException()
    {
        // Arrange
        Dictionary<int, int>? argument = null;

        // Act
        void Act() => ArgumentException.ThrowIfCountOutOfRange(argument, 4, 5);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfCountOutOfRange_Dictionary_WhenCountOutOfRange_ThrowsArgumentException()
    {
        // Arrange
        Dictionary<int, int> argument = new()
        {
            [1] = 1,
            [2] = 2,
            [3] = 3,
        };

        // Act
        void Act() => ArgumentException.ThrowIfCountOutOfRange(argument, 4, 5);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfCountOutOfRange_Dictionary_WhenValid_DoesNotThrow()
    {
        // Arrange
        Dictionary<int, int> argument = new()
        {
            [1] = 1,
            [2] = 2,
            [3] = 3,
        };

        // Act & Assert
        ArgumentException.ThrowIfCountOutOfRange(argument, 1, 3);
        _ = await Assert.That(argument.Count).IsEqualTo(3);
    }
}
