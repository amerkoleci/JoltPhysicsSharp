// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

public readonly struct BodyLockWrite
{
    public readonly BodyLockInterface LockInterface;
    public readonly IntPtr Mutex; /* JPH_SharedMutex */
    public readonly Body Body;

    /// <summary>
    /// Test if the lock was successful (if the body ID was valid)
    /// </summary>
    public bool Succeeded => Body.Handle != 0;

    /// <summary>
    /// Test if the lock was successful (if the body ID was valid) and the body is still in the broad phase
    /// </summary>
    public bool SucceededAndIsInBroadPhase => Body.Handle != 0 && Body.IsInBroadPhase;
}
