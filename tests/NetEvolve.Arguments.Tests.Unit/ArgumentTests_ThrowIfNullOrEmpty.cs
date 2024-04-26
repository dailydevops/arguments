namespace NetEvolve.Arguments.Tests.Unit;

using System;
using System.Collections.Generic;
using Xunit;

public sealed partial class ArgumentTests
{
    [Fact]
    public void ThrowIfNullOrEmpty_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? argument = null;

        // Act
        void Act() => Argument.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Fact]
    public void ThrowIfNullOrEmpty_WhenArgumentIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var argument = string.Empty;

        // Act
        void Act() => Argument.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Fact]
    public void ThrowIfNullOrEmpty_WhenArgumentIsNotEmpty_ReturnsArgument()
    {
        // Arrange
        var argument = "argument";

        // Act
        Argument.ThrowIfNullOrEmpty(argument);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void ThrowIfNullOrEmpty_WhenIEnumerableNull_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<string>? argument = null;

        // Act
        void Act() => Argument.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>(nameof(argument), Act);
    }

    [Theory]
    [MemberData(nameof(ThrowIfNullOrEmptyEnumerableData))]
    public void ThrowIfNullOrEmpty_WhenIEnumerableIsEmpty_ThrowsArgumentException(
        IEnumerable<string> argument
    )
    {
        // Act
        void Act() => Argument.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>(nameof(argument), Act);
    }

    [Theory]
    [MemberData(nameof(ThrowIfNullOrEmptyEnumerableWithData))]
    public void ThrowIfNullOrEmpty_WhenIEnumerableIsNotEmpty_ReturnsArgument(
        IEnumerable<string> argument
    )
    {
        // Act
        Argument.ThrowIfNullOrEmpty(argument);

        // Assert
        Assert.True(true);
    }

    public static TheoryData<IEnumerable<string>> ThrowIfNullOrEmptyEnumerableData
    {
        get
        {
            var data = new TheoryData<IEnumerable<string>>
            {
                Array.Empty<string>(),
                new List<string>(),
                new HashSet<string>()
            };

            return data;
        }
    }

    public static TheoryData<IEnumerable<string>> ThrowIfNullOrEmptyEnumerableWithData
    {
        get
        {
            var data = new TheoryData<IEnumerable<string>>
            {
#pragma warning disable CA1861 // Avoid constant arrays as arguments
                new[] { "argument" },
#pragma warning restore CA1861 // Avoid constant arrays as arguments
                new List<string> { "argument" },
                new HashSet<string> { "argument" }
            };

            return data;
        }
    }
}
