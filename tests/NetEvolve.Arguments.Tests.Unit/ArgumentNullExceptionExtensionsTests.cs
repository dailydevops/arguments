namespace NetEvolve.Arguments.Tests.Unit;

using System.Diagnostics.CodeAnalysis;
using NetEvolve.Extensions.TUnit;

[ExcludeFromCodeCoverage]
[UnitTest]
public partial class ArgumentNullExceptionExtensionsTests
{
    [Test]
    public void ThrowIfNull_ShouldThrowArgumentNullException_WhenValueIsNull()
    {
        object? value = null;

        _ = Assert.Throws<ArgumentNullException>(() => ArgumentNullException.ThrowIfNull(value));
    }

    [Test]
    public void ThrowIfNull_ShouldNotThrow_WhenValueIsNotNull()
    {
        var value = new object();

        ArgumentNullException.ThrowIfNull(value);
    }

    [Test]
    public unsafe void ThrowIfNull_ShouldThrowArgumentNullException_WhenPointerIsNull()
    {
        void* pointer = null;

        _ = Assert.Throws<ArgumentNullException>(() => ArgumentNullException.ThrowIfNull(pointer));
    }

    [Test]
    public unsafe void ThrowIfNull_ShouldNotThrow_WhenPointerIsNotNull()
    {
        var dummy = 42;
        void* pointer = &dummy;

        ArgumentNullException.ThrowIfNull(pointer);
    }
}
