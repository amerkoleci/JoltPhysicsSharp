// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class CharacterBase : NativeObject
{
    protected CharacterBase()
    {
    }

    protected CharacterBase(IntPtr handle)
        : base(handle) { }

    ~CharacterBase() => Dispose(isDisposing: false);

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            JPH_CharacterBase_Destroy(Handle);
        }
    }

    public GroundState GetGroundState()
    {
        return JPH_CharacterBase_GetGroundState(Handle);
    }

    public bool IsSupported()
    {
        return JPH_CharacterBase_IsSupported(Handle);
    }

    public Double3 GetGroundPosition()
    {
        JPH_CharacterBase_GetGroundPosition(Handle, out Double3 position);
        return position;
    }

    public Vector3 GetGroundNormal()
    {
        JPH_CharacterBase_GetGroundNormal(Handle, out Vector3 normal);
        return normal;
    }

    public Vector3 GetGroundVelocity()
    {
        JPH_CharacterBase_GetGroundVelocity(Handle, out Vector3 velocity);
        return velocity;
    }

    public uint GetGroundBodyId()
    {
        return JPH_CharacterBase_GetGroundBodyId(Handle);
    }

    public uint GetGroundSubShapeId()
    {
        return JPH_CharacterBase_GetGroundSubShapeId(Handle);
    }
}
