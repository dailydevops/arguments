namespace NetEvolve.Arguments.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TUnit.Assertions.Extensions;

public sealed partial class ArgumentTests
{
    [Test]
    public void ThrowIfNullOrEmpty_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? argument = null;

        // Act
        void Act() => Argument.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>("argument", Act);
    }

    [Test]
    public void ThrowIfNullOrEmpty_WhenArgumentIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var argument = string.Empty;

        // Act
        void Act() => Argument.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>("argument", Act);
    }

    [Test]
    public async Task ThrowIfNullOrEmpty_WhenArgumentIsNotEmpty_ReturnsArgument()
    {
        // Arrange
        var argument = "argument";

        // Act
        Argument.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = await Assert.That(argument).IsNotNullOrWhiteSpace();
    }

    [Test]
    public void ThrowIfNullOrEmpty_WhenIEnumerableNull_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<string>? argument = null;

        // Act
        void Act() => Argument.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentNullException>(nameof(argument), Act);
    }

    [Test]
    [MethodDataSource(nameof(ThrowIfNullOrEmptyEnumerableData))]
    public void ThrowIfNullOrEmpty_WhenIEnumerableEmpty_ThrowsArgumentException(IEnumerable<string>? argument)
    {
        // Act
        void Act() => Argument.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = Assert.Throws<ArgumentException>(nameof(argument), Act);
    }

    [Test]
    [MethodDataSource(nameof(ThrowIfNullOrEmptyEnumerableWithData))]
    public async Task ThrowIfNullOrEmpty_WhenIEnumerableEmpty_Expected(IEnumerable<string>? argument)
    {
        // Act
        Argument.ThrowIfNullOrEmpty(argument);

        // Assert
        _ = await Assert.That(argument).IsNotNull().And.IsNotEmpty();
    }

    public static IEnumerable<IEnumerable<string>> ThrowIfNullOrEmptyEnumerableData =>
        [Array.Empty<string>(), new List<string>(), new HashSet<string>()];

    public static IEnumerable<IEnumerable<string>> ThrowIfNullOrEmptyEnumerableWithData =>
        [new[] { "argument" }, new List<string> { "argument" }, new HashSet<string> { "argument" }];
}
