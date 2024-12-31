// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics.CodeAnalysis;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly struct BodyLockMultiRead : IEquatable<BodyLockMultiRead>, IDisposable
{
    public BodyLockMultiRead(nint handle) { Handle = handle; }
    public nint Handle { get; }
    public bool IsNull => Handle == 0;
    public static BodyLockInterface Null => new(0);
    public static implicit operator BodyLockMultiRead(nint handle) => new(handle);
    public static bool operator ==(BodyLockMultiRead left, BodyLockMultiRead right) => left.Handle == right.Handle;
    public static bool operator !=(BodyLockMultiRead left, BodyLockMultiRead right) => left.Handle != right.Handle;
    public static bool operator ==(BodyLockMultiRead left, nint right) => left.Handle == right;
    public static bool operator !=(BodyLockMultiRead left, nint right) => left.Handle != right;
    public bool Equals(BodyLockMultiRead other) => Handle == other.Handle;

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is BodyLockMultiRead handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => Handle.GetHashCode();

    /// <inheritdoc/>
    public void Dispose() => JPH_BodyLockMultiRead_Destroy(Handle);

    public Body? GetBody(uint bodyIndex)
    {
        return Body.GetObject(JPH_BodyLockMultiRead_GetBody(Handle, bodyIndex));
    }
}
