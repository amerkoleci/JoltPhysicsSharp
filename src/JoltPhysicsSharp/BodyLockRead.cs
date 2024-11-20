// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

public readonly struct BodyLockRead
{
    public readonly BodyLockInterface LockInterface;
    public readonly nint Mutex; /* JPH_SharedMutex */
    public readonly nint BodyHandle;

    public Body? Body => Body.GetObject(BodyHandle);

    /// <summary>
    /// Test if the lock was successful (if the body ID was valid)
    /// </summary>
    public bool Succeeded => BodyHandle != 0;

    /// <summary>
    /// Test if the lock was successful (if the body ID was valid) and the body is still in the broad phase
    /// </summary>
    public bool SucceededAndIsInBroadPhase => BodyHandle != 0 && Body!.IsInBroadPhase;
}
