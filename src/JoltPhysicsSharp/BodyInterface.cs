// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly partial struct BodyInterface : IEquatable<BodyInterface>
{
    public BodyInterface(IntPtr handle) { Handle = handle; }
    public IntPtr Handle { get; }
    public bool IsNull => Handle == IntPtr.Zero;
    public static BodyInterface Null => new(IntPtr.Zero);
    public static implicit operator BodyInterface(IntPtr handle) => new(handle);
    public static bool operator ==(BodyInterface left, BodyInterface right) => left.Handle == right.Handle;
    public static bool operator !=(BodyInterface left, BodyInterface right) => left.Handle != right.Handle;
    public static bool operator ==(BodyInterface left, IntPtr right) => left.Handle == right;
    public static bool operator !=(BodyInterface left, IntPtr right) => left.Handle != right;
    public bool Equals(BodyInterface other) => Handle == other.Handle;
    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is BodyInterface handle && Equals(handle);
    /// <inheritdoc/>
    public override int GetHashCode() => Handle.GetHashCode();

    public Body CreateBody(BodyCreationSettings settings)
    {
        IntPtr bodyHandle = JPH_BodyInterface_CreateBody(Handle, settings.Handle);
        return new Body(bodyHandle);
    }

    public BodyID CreateAndAddBody(BodyCreationSettings settings, ActivationMode activationMode)
    {
        uint bodyID = JPH_BodyInterface_CreateAndAddBody(Handle, settings.Handle, activationMode);
        return bodyID;
    }

    public void DestroyBody(in Body body) => DestroyBody(body.ID);

    public void DestroyBody(in BodyID bodyID)
    {
        JPH_BodyInterface_DestroyBody(Handle, bodyID);
    }

    public void AddBody(in Body body, ActivationMode activationMode) => AddBody(body.ID, activationMode);

    public void AddBody(in BodyID bodyID, ActivationMode activationMode)
    {
        JPH_BodyInterface_AddBody(Handle, bodyID, activationMode);
    }

    public void RemoveBody(in Body body)
    {
        JPH_BodyInterface_RemoveBody(Handle, body.ID);
    }

    public void RemoveBody(in BodyID bodyID)
    {
        JPH_BodyInterface_RemoveBody(Handle, bodyID);
    }

    public unsafe void SetLinearVelocity(in Body body, Vector3 velocity)
    {
        JPH_BodyInterface_SetLinearVelocity(Handle, body.ID, &velocity);
    }

    public unsafe void SetLinearVelocity(in BodyID bodyID, Vector3 velocity)
    {
        JPH_BodyInterface_SetLinearVelocity(Handle, bodyID, &velocity);
    }

    public unsafe Vector3 GetLinearVelocity(in BodyID bodyID)
    {
        Vector3 velocity;
        JPH_BodyInterface_GetLinearVelocity(Handle, bodyID, &velocity);
        return velocity;
    }

    public unsafe void GetLinearVelocity(in BodyID bodyID, Vector3* velocity)
    {
        JPH_BodyInterface_GetLinearVelocity(Handle, bodyID, velocity);
    }

    public unsafe Vector3 GetCenterOfMassPosition(in BodyID bodyID)
    {
        Vector3 velocity;
        JPH_BodyInterface_GetCenterOfMassPosition(Handle, bodyID, &velocity);
        return velocity;
    }

    public unsafe void GetCenterOfMassPosition(in BodyID bodyID, Vector3* velocity)
    {
        JPH_BodyInterface_GetCenterOfMassPosition(Handle, bodyID, velocity);
    }

    public bool IsActive(in Body body) => IsActive(body.ID);

    public bool IsActive(in BodyID bodyID)
    {
        return JPH_BodyInterface_IsActive(Handle, bodyID);
    }
}
