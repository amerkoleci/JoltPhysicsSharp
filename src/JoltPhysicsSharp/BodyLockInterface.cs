// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly struct BodyLockInterface : IEquatable<BodyLockInterface>
{
    public BodyLockInterface(nint handle) { Handle = handle; }
    public nint Handle { get; }
    public bool IsNull => Handle == 0;
    public static BodyLockInterface Null => new(0);
    public static implicit operator BodyLockInterface(nint handle) => new(handle);
    public static bool operator ==(BodyLockInterface left, BodyLockInterface right) => left.Handle == right.Handle;
    public static bool operator !=(BodyLockInterface left, BodyLockInterface right) => left.Handle != right.Handle;
    public static bool operator ==(BodyLockInterface left, nint right) => left.Handle == right;
    public static bool operator !=(BodyLockInterface left, nint right) => left.Handle != right;
    public bool Equals(BodyLockInterface other) => Handle == other.Handle;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is BodyLockInterface handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => Handle.GetHashCode();

    public void LockRead(in BodyID bodyID, out BodyLockRead @lock)
    {
        JPH_BodyLockInterface_LockRead(Handle, bodyID, out @lock);
    }

    public void UnlockRead(in BodyLockRead @lock)
    {
        JPH_BodyLockInterface_UnlockRead(Handle, in @lock);
    }

    public void LockWrite(in BodyID bodyID, out BodyLockWrite @lock)
    {
        JPH_BodyLockInterface_LockWrite(Handle, bodyID, out @lock);
    }

    public void UnlockWrite(in BodyLockWrite @lock)
    {
        JPH_BodyLockInterface_UnlockWrite(Handle, in @lock);
    }

    public unsafe BodyLockMultiRead LockMultiRead(Span<BodyID> bodyIDs)
    {
        uint* bodyIDsPtr = stackalloc uint[bodyIDs.Length];
        for (int i = 0; i < bodyIDs.Length; i++)
        {
            bodyIDsPtr[i] = bodyIDs[i];
        }

        return JPH_BodyLockInterface_LockMultiRead(Handle, bodyIDsPtr, (uint)bodyIDs.Length);
    }

    public unsafe BodyLockMultiWrite LockMultiWrite(Span<BodyID> bodyIDs)
    {
        uint* bodyIDsPtr = stackalloc uint[bodyIDs.Length];
        for (int i = 0; i < bodyIDs.Length; i++)
        {
            bodyIDsPtr[i] = bodyIDs[i];
        }

        return JPH_BodyLockInterface_LockMultiWrite(Handle, bodyIDsPtr, (uint)bodyIDs.Length);
    }
}
