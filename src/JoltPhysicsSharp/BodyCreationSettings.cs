// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class BodyCreationSettings : NativeObject
{
    public BodyCreationSettings()
        : base(JPH_BodyCreationSettings_Create())
    {
    }

    public BodyCreationSettings(ShapeSettings shapeSettings, in Double3 position, in Quaternion rotation, MotionType motionType, ObjectLayer objectLayer)
       : base(JPH_BodyCreationSettings_Create2(shapeSettings.Handle, position, rotation, motionType, objectLayer))
    {
    }

    public BodyCreationSettings(ShapeSettings shapeSettings, in Vector3 position, in Quaternion rotation, MotionType motionType, ObjectLayer objectLayer)
       : base(JPH_BodyCreationSettings_Create2(shapeSettings.Handle, new(position), rotation, motionType, objectLayer))
    {
    }

    public BodyCreationSettings(Shape shape, in Double3 position, in Quaternion rotation, MotionType motionType, ObjectLayer objectLayer)
      : base(JPH_BodyCreationSettings_Create3(shape.Handle, position, rotation, motionType, objectLayer))
    {
    }

    public BodyCreationSettings(Shape shape, in Vector3 position, in Quaternion rotation, MotionType motionType, ObjectLayer objectLayer)
      : base(JPH_BodyCreationSettings_Create3(shape.Handle, new(position), rotation, motionType, objectLayer))
    {
    }

    public Vector3 LinearVelocity
    {
        get
        {
            JPH_BodyCreationSettings_GetLinearVelocity(Handle, out Vector3 velocity);
            return velocity;
        }
        set => JPH_BodyCreationSettings_SetLinearVelocity(Handle, value);
    }

    public Vector3 AngularVelocity
    {
        get
        {
            JPH_BodyCreationSettings_GetAngularVelocity(Handle, out Vector3 velocity);
            return velocity;
        }
        set => JPH_BodyCreationSettings_SetAngularVelocity(Handle, value);
    }

    public MotionType MotionType
    {
        get => JPH_BodyCreationSettings_GetMotionType(Handle);
        set => JPH_BodyCreationSettings_SetMotionType(Handle, value);
    }

    public AllowedDOFs AllowedDOFs
    {
        get => JPH_BodyCreationSettings_GetAllowedDOFs(Handle);
        set => JPH_BodyCreationSettings_SetAllowedDOFs(Handle, value);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="BodyCreationSettings" /> class.
    /// </summary>
    ~BodyCreationSettings() => Dispose(isDisposing: false);

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            JPH_BodyCreationSettings_Destroy(Handle);
        }
    }

    public void GetLinearVelocity(out Vector3 velocity)
    {
        JPH_BodyCreationSettings_GetLinearVelocity(Handle, out velocity);
    }

    public void GetAngularVelocity(out Vector3 velocity)
    {
        JPH_BodyCreationSettings_GetAngularVelocity(Handle, out velocity);
    }
}
