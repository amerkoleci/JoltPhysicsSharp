// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class Character : CharacterBase
{
    public Character(CharacterVirtualSettings settings, in Vector3 position, in Quaternion rotation, ulong userData, PhysicsSystem physicsSystem)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use constructor with Double3");

        Handle = JPH_Character_Create(settings.Handle, position, rotation, userData, physicsSystem.Handle);
    }

    public Character(CharacterVirtualSettings settings, in Double3 position, in Quaternion rotation, ulong userData, PhysicsSystem physicsSystem)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use constructor with Vector3");

        Handle = JPH_Character_Create(settings.Handle, position, rotation, userData, physicsSystem.Handle);
    }

    public void AddToPhysicsSystem(Activation activationMode = Activation.Activate, bool lockBodies = true)
    {
        JPH_Character_AddToPhysicsSystem(Handle, activationMode, lockBodies);
    }

    public void RemoveFromPhysicsSystem(bool lockBodies = true)
    {
        JPH_Character_RemoveFromPhysicsSystem(Handle, lockBodies);
    }

    public void Activate(bool lockBodies = true)
    {
        JPH_Character_Activate(Handle, lockBodies);
    }

    public void PostSimulation(float maxSeparationDistance, bool lockBodies = true)
    {
        JPH_Character_PostSimulation(Handle, maxSeparationDistance, lockBodies);
    }
}
