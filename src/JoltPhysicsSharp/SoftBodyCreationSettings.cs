// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class SoftBodyCreationSettings : NativeObject
{
    public SoftBodyCreationSettings()
        : base(JPH_SoftBodyCreationSettings_Create())
    {
    }

    public SoftBodyCreationSettings(SoftBodySharedSettings settings, in Vector3 position, in Quaternion rotation, ObjectLayer objectLayer)
        : base(JPH_SoftBodyCreationSettings_Create2(settings.Handle, position, rotation, objectLayer))
    {
    }

    protected override void DisposeNative()
    {
        JPH_SoftBodyCreationSettings_Destroy(Handle);
    }

    public SoftBodySharedSettings? Settings
    {
        get => SoftBodySharedSettings.GetObject(JPH_SoftBodyCreationSettings_GetSettings(Handle));
        set => JPH_SoftBodyCreationSettings_SetSettings(Handle, value is not null ? value.Handle : 0);  
    }

    public Vector3 Position
    {
        get
        {
            JPH_SoftBodyCreationSettings_GetPosition(Handle, out Vector3 result);
            return result;
        }
        set => JPH_SoftBodyCreationSettings_SetPosition(Handle, value);
    }

    public Quaternion Rotation
    {
        get
        {
            JPH_SoftBodyCreationSettings_GetRotation(Handle, out Quaternion result);
            return result;
        }
        set => JPH_SoftBodyCreationSettings_SetRotation(Handle, value);
    }

    public ulong UserData
    {
        get => JPH_SoftBodyCreationSettings_GetUserData(Handle);
        set => JPH_SoftBodyCreationSettings_SetUserData(Handle, value);
    }

    public ObjectLayer ObjectLayer
    {
        get => JPH_SoftBodyCreationSettings_GetObjectLayer(Handle);
        set => JPH_SoftBodyCreationSettings_SetObjectLayer(Handle, value);
    }

    public unsafe CollisionGroup CollisionGroup
    {
        get
        {
            JPH_CollisionGroup groupNative = default;
            JPH_SoftBodyCreationSettings_GetCollisionGroup(Handle, &groupNative);
            return CollisionGroup.FromNative(groupNative);
        }
        set
        {
            value.ToNative(out JPH_CollisionGroup groupNative);
            JPH_SoftBodyCreationSettings_SetCollisionGroup(Handle, &groupNative);
        }
    }

    public uint NumIterations
    {
        get => JPH_SoftBodyCreationSettings_GetNumIterations(Handle);
        set => JPH_SoftBodyCreationSettings_SetNumIterations(Handle, value);
    }

    public float LinearDamping
    {
        get => JPH_SoftBodyCreationSettings_GetLinearDamping(Handle);
        set => JPH_SoftBodyCreationSettings_SetLinearDamping(Handle, value);
    }

    public float MaxLinearVelocity
    {
        get => JPH_SoftBodyCreationSettings_GetMaxLinearVelocity(Handle);
        set => JPH_SoftBodyCreationSettings_SetMaxLinearVelocity(Handle, value);
    }

    public float Restitution
    {
        get => JPH_SoftBodyCreationSettings_GetRestitution(Handle);
        set => JPH_SoftBodyCreationSettings_SetRestitution(Handle, value);
    }

    public float Friction
    {
        get => JPH_SoftBodyCreationSettings_GetFriction(Handle);
        set => JPH_SoftBodyCreationSettings_SetFriction(Handle, value);
    }

    public float Pressure
    {
        get => JPH_SoftBodyCreationSettings_GetPressure(Handle);
        set => JPH_SoftBodyCreationSettings_SetPressure(Handle, value);
    }

    public float GravityFactor
    {
        get => JPH_SoftBodyCreationSettings_GetGravityFactor(Handle);
        set => JPH_SoftBodyCreationSettings_SetGravityFactor(Handle, value);
    }

    public float VertexRadius
    {
        get => JPH_SoftBodyCreationSettings_GetVertexRadius(Handle);
        set => JPH_SoftBodyCreationSettings_SetVertexRadius(Handle, value);
    }

    public bool UpdatePosition
    {
        get => JPH_SoftBodyCreationSettings_GetUpdatePosition(Handle);
        set => JPH_SoftBodyCreationSettings_SetUpdatePosition(Handle, value);
    }

    public bool MakeRotationIdentity
    {
        get => JPH_SoftBodyCreationSettings_GetMakeRotationIdentity(Handle);
        set => JPH_SoftBodyCreationSettings_SetMakeRotationIdentity(Handle, value);
    }

    public bool AllowSleeping
    {
        get => JPH_SoftBodyCreationSettings_GetAllowSleeping(Handle);
        set => JPH_SoftBodyCreationSettings_SetAllowSleeping(Handle, value);
    }

    public bool FacesDoubleSided
    {
        get => JPH_SoftBodyCreationSettings_GetFacesDoubleSided(Handle);
        set => JPH_SoftBodyCreationSettings_SetFacesDoubleSided(Handle, value);
    }
}
