// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly struct TempAllocator : IEquatable<TempAllocator>, IDisposable
{
    public TempAllocator()
    {
        Handle = JPH_TempAllocatorMalloc_Create();
    }

    public TempAllocator(int size)
    {
        Handle = JPH_TempAllocator_Create((uint)size);
    }

    public void Dispose()
    {
        JPH_TempAllocator_Destroy(Handle);
    }

    public TempAllocator(IntPtr handle) { Handle = handle; }
    public IntPtr Handle { get; }
    public bool IsNull => Handle == IntPtr.Zero;
    public static TempAllocator Null => new(IntPtr.Zero);
    public static implicit operator TempAllocator(IntPtr handle) => new(handle);
    public static bool operator ==(TempAllocator left, TempAllocator right) => left.Handle == right.Handle;
    public static bool operator !=(TempAllocator left, TempAllocator right) => left.Handle != right.Handle;
    public static bool operator ==(TempAllocator left, IntPtr right) => left.Handle == right;
    public static bool operator !=(TempAllocator left, IntPtr right) => left.Handle != right;
    public bool Equals(TempAllocator other) => Handle == other.Handle;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is TempAllocator handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => Handle.GetHashCode();
}
