// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;
public abstract class CharacterBaseSettings : NativeObject
{
    protected CharacterBaseSettings(nint handle)
        : base(handle)
    {
    }

    ~CharacterBaseSettings() => Dispose(disposing: false);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            JPH_CharacterBaseSettings_Destroy(Handle);
        }
    }

    public Vector3 Up
    {
        get
        {
            JPH_CharacterBaseSettings_GetUp(Handle, out Vector3 result);
            return result;
        }
        set
        {
            JPH_CharacterBaseSettings_SetUp(Handle, in value);
        }
    }

    public Plane SupportingVolume
    {
        get
        {
            JPH_CharacterBaseSettings_GetSupportingVolume(Handle, out Plane result);
            return result;
        }
        set
        {
            JPH_CharacterBaseSettings_SetSupportingVolume(Handle, in value);
        }
    }

    public float MaxSlopeAngle
    {
        get
        {
            return JPH_CharacterBaseSettings_GetMaxSlopeAngle(Handle);
        }
        set
        {
            JPH_CharacterBaseSettings_SetMaxSlopeAngle(Handle, value);
        }
    }

    public bool EnhancedInternalEdgeRemoval
    {
        get
        {
            return JPH_CharacterBaseSettings_GetEnhancedInternalEdgeRemoval(Handle);
        }
        set
        {
            JPH_CharacterBaseSettings_SetEnhancedInternalEdgeRemoval(Handle, value);
        }
    }

    public Shape Shape
    {
        set => JPH_CharacterBaseSettings_SetShape(Handle, value.Handle);
    }
}
