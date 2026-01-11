namespace NetEvolve.Arguments.Tests.Unit;

using System;

public sealed class ObjectDisposedExceptionPolyfillsTests
{
    [Test]
    public async Task ThrowIf_WithObjectInstance_WhenConditionIsTrue_ThrowsObjectDisposedException()
    {
        using var instance = new DisposableTestClass();

        void Act() => ObjectDisposedException.ThrowIf(true, instance);

        var exception = Assert.Throws<ObjectDisposedException>(Act);
        _ = await Assert.That(exception.ObjectName).IsEqualTo(typeof(DisposableTestClass).FullName);
    }

    [Test]
    public async Task ThrowIf_WithObjectInstance_WhenConditionIsFalse_DoesNotThrow()
    {
        using var instance = new DisposableTestClass();

        ObjectDisposedException.ThrowIf(false, instance);

        _ = await Assert.That(instance).IsNotNull();
    }

    [Test]
    public async Task ThrowIf_WithType_WhenConditionIsTrue_ThrowsObjectDisposedException()
    {
        var type = typeof(DisposableTestClass);

        void Act() => ObjectDisposedException.ThrowIf(true, type);

        var exception = Assert.Throws<ObjectDisposedException>(Act);
        _ = await Assert.That(exception.ObjectName).IsEqualTo(typeof(DisposableTestClass).FullName);
    }

    [Test]
    public async Task ThrowIf_WithType_WhenConditionIsFalse_DoesNotThrow()
    {
        var type = typeof(DisposableTestClass);

        ObjectDisposedException.ThrowIf(false, type);

        _ = await Assert.That(type).IsNotNull();
    }

    [Test]
    public async Task ThrowIf_WithNullInstance_WhenConditionIsTrue_ThrowsObjectDisposedException()
    {
        object? instance = null;

        void Act() => ObjectDisposedException.ThrowIf(true, instance!);

        var exception = Assert.Throws<ObjectDisposedException>(Act);
        _ = await Assert.That(exception.ObjectName).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task ThrowIf_WithNullType_WhenConditionIsTrue_ThrowsObjectDisposedException()
    {
        Type? type = null;

        void Act() => ObjectDisposedException.ThrowIf(true, type!);

        var exception = Assert.Throws<ObjectDisposedException>(Act);
        _ = await Assert.That(exception.ObjectName).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task ThrowIf_WithDisposedFlag_WhenNotDisposed_DoesNotThrow()
    {
        using var disposable = new DisposableTestClass();

        ObjectDisposedException.ThrowIf(disposable.IsDisposed, disposable);

        _ = await Assert.That(disposable.IsDisposed).IsFalse();
    }

    [Test]
    public void ThrowIf_WithDisposedFlag_WhenDisposed_ThrowsObjectDisposedException()
    {
        using var disposable = new DisposableTestClass();
        disposable.Dispose();

        void Act() => ObjectDisposedException.ThrowIf(disposable.IsDisposed, disposable);

        _ = Assert.Throws<ObjectDisposedException>(Act);
    }

    private sealed class DisposableTestClass : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose() => IsDisposed = true;
    }
}
