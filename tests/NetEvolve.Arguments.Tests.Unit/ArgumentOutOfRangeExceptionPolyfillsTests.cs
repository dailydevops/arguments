namespace NetEvolve.Arguments.Tests.Unit;

using System;

public sealed class ArgumentOutOfRangeExceptionPolyfillsTests
{
    [Test]
    public void ThrowIfZero_Int_WhenValueIsZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var value = 0;

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfZero(value);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("value", Act);
    }

    [Test]
    [Arguments(1)]
    [Arguments(-1)]
    [Arguments(100)]
    [Arguments(-100)]
    public async Task ThrowIfZero_Int_WhenValueIsNotZero_DoesNotThrow(int value)
    {
        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfZero(value);
        _ = await Assert.That(value).IsNotEqualTo(0);
    }

    [Test]
    public void ThrowIfZero_Double_WhenValueIsZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var value = 0.0;

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfZero(value);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("value", Act);
    }

    [Test]
    [Arguments(1.5)]
    [Arguments(-1.5)]
    public async Task ThrowIfZero_Double_WhenValueIsNotZero_DoesNotThrow(double value)
    {
        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfZero(value);
        _ = await Assert.That(value).IsNotEqualTo(0.0);
    }

    [Test]
    public void ThrowIfNegative_Int_WhenValueIsNegative_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var value = -1;

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfNegative(value);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("value", Act);
    }

    [Test]
    [Arguments(0)]
    [Arguments(1)]
    [Arguments(100)]
    public async Task ThrowIfNegative_Int_WhenValueIsNonNegative_DoesNotThrow(int value)
    {
        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        _ = await Assert.That(value).IsGreaterThanOrEqualTo(0);
    }

    [Test]
    public void ThrowIfNegative_Double_WhenValueIsNegative_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var value = -1.5;

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfNegative(value);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("value", Act);
    }

    [Test]
    [Arguments(0.0)]
    [Arguments(1.5)]
    public async Task ThrowIfNegative_Double_WhenValueIsNonNegative_DoesNotThrow(double value)
    {
        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        _ = await Assert.That(value).IsGreaterThanOrEqualTo(0.0);
    }

    [Test]
    public void ThrowIfNegative_NInt_WhenValueIsNegative_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        nint value = -1;

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfNegative(value);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("value", Act);
    }

    [Test]
    public async Task ThrowIfNegative_NInt_WhenValueIsNonNegative_DoesNotThrow()
    {
        // Arrange
        nint value = 0;

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfNegative(value);
    }

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    [Arguments(-100)]
    public void ThrowIfNegativeOrZero_Int_WhenValueIsNegativeOrZero_ThrowsArgumentOutOfRangeException(int value)
    {
        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>(nameof(value), Act);
    }

    [Test]
    [Arguments(1)]
    [Arguments(100)]
    public async Task ThrowIfNegativeOrZero_Int_WhenValueIsPositive_DoesNotThrow(int value)
    {
        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        _ = await Assert.That(value).IsGreaterThan(0);
    }

    [Test]
    public void ThrowIfNegativeOrZero_Double_WhenValueIsZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var value = 0.0;

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("value", Act);
    }

    [Test]
    public void ThrowIfNegativeOrZero_Double_WhenValueIsNegative_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var value = -1.5;

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("value", Act);
    }

    [Test]
    [Arguments(0.1)]
    [Arguments(1.5)]
    public async Task ThrowIfNegativeOrZero_Double_WhenValueIsPositive_DoesNotThrow(double value)
    {
        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        _ = await Assert.That(value).IsGreaterThan(0.0);
    }

    [Test]
    public void ThrowIfNegativeOrZero_NInt_WhenValueIsZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        nint value = 0;

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("value", Act);
    }

    [Test]
    public void ThrowIfNegativeOrZero_NInt_WhenValueIsNegative_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        nint value = -1;

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("value", Act);
    }

    [Test]
    public async Task ThrowIfNegativeOrZero_NInt_WhenValueIsPositive_DoesNotThrow()
    {
        // Arrange
        nint value = 1;

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
    }

    [Test]
    public void ThrowIfEqual_Int_WhenValuesAreEqual_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var value = 5;
        var other = 5;

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfEqual(value, other);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("value", Act);
    }

    [Test]
    [Arguments(1, 2)]
    [Arguments(10, 5)]
    [Arguments(-1, 1)]
    public async Task ThrowIfEqual_Int_WhenValuesAreNotEqual_DoesNotThrow(int value, int other)
    {
        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfEqual(value, other);
        _ = await Assert.That(value).IsNotEqualTo(other);
    }

    [Test]
    public void ThrowIfEqual_String_WhenValuesAreEqual_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var value = "test";
        var other = "test";

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfEqual(value, other);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("value", Act);
    }

    [Test]
    public async Task ThrowIfEqual_String_WhenValuesAreNotEqual_DoesNotThrow()
    {
        // Arrange
        var value = "test1";
        var other = "test2";

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfEqual(value, other);
        _ = await Assert.That(value).IsNotEqualTo(other);
    }

    [Test]
    [Arguments(1, 2)]
    [Arguments(10, 5)]
    [Arguments(-1, 1)]
    public void ThrowIfNotEqual_Int_WhenValuesAreNotEqual_ThrowsArgumentOutOfRangeException(int value, int other)
    {
        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfNotEqual(value, other);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>(nameof(value), Act);
    }

    [Test]
    public async Task ThrowIfNotEqual_Int_WhenValuesAreEqual_DoesNotThrow()
    {
        // Arrange
        var value = 5;
        var other = 5;

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfNotEqual(value, other);
        _ = await Assert.That(value).IsEqualTo(other);
    }

    [Test]
    public void ThrowIfNotEqual_String_WhenValuesAreNotEqual_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var value = "test1";
        var other = "test2";

        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfNotEqual(value, other);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>("value", Act);
    }

    [Test]
    public async Task ThrowIfNotEqual_String_WhenValuesAreEqual_DoesNotThrow()
    {
        // Arrange
        var value = "test";
        var other = "test";

        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfNotEqual(value, other);
        _ = await Assert.That(value).IsEqualTo(other);
    }

    [Test]
    [Arguments(6, 5)]
    [Arguments(100, 50)]
    public void ThrowIfGreaterThan_Int_WhenValueIsGreater_ThrowsArgumentOutOfRangeException(int value, int other)
    {
        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>(nameof(value), Act);
    }

    [Test]
    [Arguments(5, 5)]
    [Arguments(4, 5)]
    [Arguments(1, 100)]
    public async Task ThrowIfGreaterThan_Int_WhenValueIsNotGreater_DoesNotThrow(int value, int other)
    {
        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other);
        _ = await Assert.That(value).IsLessThanOrEqualTo(other);
    }

    [Test]
    [Arguments(6, 5)]
    [Arguments(5, 5)]
    public void ThrowIfGreaterThanOrEqual_Int_WhenValueIsGreaterOrEqual_ThrowsArgumentOutOfRangeException(
        int value,
        int other
    )
    {
        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, other);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>(nameof(value), Act);
    }

    [Test]
    [Arguments(4, 5)]
    [Arguments(1, 100)]
    public async Task ThrowIfGreaterThanOrEqual_Int_WhenValueIsLess_DoesNotThrow(int value, int other)
    {
        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, other);
        _ = await Assert.That(value).IsLessThan(other);
    }

    [Test]
    [Arguments(4, 5)]
    [Arguments(1, 100)]
    public void ThrowIfLessThan_Int_WhenValueIsLess_ThrowsArgumentOutOfRangeException(int value, int other)
    {
        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfLessThan(value, other);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>(nameof(value), Act);
    }

    [Test]
    [Arguments(5, 5)]
    [Arguments(6, 5)]
    [Arguments(100, 1)]
    public async Task ThrowIfLessThan_Int_WhenValueIsNotLess_DoesNotThrow(int value, int other)
    {
        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfLessThan(value, other);
        _ = await Assert.That(value).IsGreaterThanOrEqualTo(other);
    }

    [Test]
    [Arguments(4, 5)]
    [Arguments(5, 5)]
    public void ThrowIfLessThanOrEqual_Int_WhenValueIsLessOrEqual_ThrowsArgumentOutOfRangeException(
        int value,
        int other
    )
    {
        // Act
        void Act() => ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, other);

        // Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>(nameof(value), Act);
    }

    [Test]
    [Arguments(6, 5)]
    [Arguments(100, 1)]
    public async Task ThrowIfLessThanOrEqual_Int_WhenValueIsGreater_DoesNotThrow(int value, int other)
    {
        // Act & Assert
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, other);
        _ = await Assert.That(value).IsGreaterThan(other);
    }
}
