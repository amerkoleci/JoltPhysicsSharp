// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.CompilerServices;

namespace JoltPhysicsSharp;

/// <summary>An object which is disposable.</summary>
public abstract class DisposableObject : IDisposable
{
#if NET6_0_OR_GREATER
    private volatile uint _isDisposed;
#else
    private volatile int _isDisposed;
#endif

    /// <summary>Initializes a new instance of the <see cref="DisposableObject" /> class.</summary>
    protected DisposableObject()
    {
        _isDisposed = 0;
    }

    /// <summary>Gets <c>true</c> if the object has been disposed; otherwise, <c>false</c>.</summary>
    public bool IsDisposed => _isDisposed != 0;

    /// <inheritdoc />
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <inheritdoc cref="Dispose()" />
    /// <param name="disposing"><c>true</c> if the method was called from <see cref="Dispose()" />; otherwise, <c>false</c>.</param>
    protected abstract void Dispose(bool disposing);

    /// <summary>Throws an exception if the object has been disposed.</summary>
    /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ThrowIfDisposed()
    {
        if (_isDisposed != 0)
        {
            _ = new ObjectDisposedException(GetType().Name);
        }
    }

    /// <summary>Marks the object as being disposed.</summary>
    protected void MarkDisposed()
    {
        _ = Interlocked.Exchange(ref _isDisposed, 1);
    }
}
