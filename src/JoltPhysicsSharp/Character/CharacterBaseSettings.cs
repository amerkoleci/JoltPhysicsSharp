// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;
public abstract class CharacterBaseSettings : NativeObject
{
    protected CharacterBaseSettings()
        : base()
    {
    }

    protected CharacterBaseSettings(nint handle)
        : base(handle)
    {
    }

    ~CharacterBaseSettings() => Dispose(isDisposing: false);

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            JPH_CharacterBaseSettings_Destroy(Handle);
        }
    }

    public void SetMaxSlopeAngle(float maxSlopeAngle)
    {
        JPH_CharacterBaseSettings_SetMaxSlopeAngle(Handle, maxSlopeAngle);
    }

    public void SetShape(Shape shape)
    {
        JPH_CharacterBaseSettings_SetShape(Handle, shape.Handle);
    }

    public void SetSupportingVolume(in Vector3 normal, float constant)
    {
        JPH_CharacterBaseSettings_SetSupportingVolume(Handle, normal, constant);
    }
}
