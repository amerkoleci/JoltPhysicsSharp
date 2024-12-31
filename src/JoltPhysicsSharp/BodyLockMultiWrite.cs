// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics.CodeAnalysis;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly struct BodyLockMultiWrite : IEquatable<BodyLockMultiWrite>, IDisposable
{
    public BodyLockMultiWrite(nint handle) { Handle = handle; }
    public nint Handle { get; }
    public bool IsNull => Handle == 0;
    public static BodyLockInterface Null => new(0);
    public static implicit operator BodyLockMultiWrite(nint handle) => new(handle);
    public static bool operator ==(BodyLockMultiWrite left, BodyLockMultiWrite right) => left.Handle == right.Handle;
    public static bool operator !=(BodyLockMultiWrite left, BodyLockMultiWrite right) => left.Handle != right.Handle;
    public static bool operator ==(BodyLockMultiWrite left, nint right) => left.Handle == right;
    public static bool operator !=(BodyLockMultiWrite left, nint right) => left.Handle != right;
    public bool Equals(BodyLockMultiWrite other) => Handle == other.Handle;

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is BodyLockMultiWrite handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => Handle.GetHashCode();

    /// <inheritdoc/>
    public void Dispose() => JPH_BodyLockMultiWrite_Destroy(Handle);

    public Body? GetBody(uint bodyIndex)
    {
        return Body.GetObject(JPH_BodyLockMultiWrite_GetBody(Handle, bodyIndex));
    }
}
