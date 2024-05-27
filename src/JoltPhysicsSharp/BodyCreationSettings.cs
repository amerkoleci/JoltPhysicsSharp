// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed unsafe class BodyCreationSettings : NativeObject
{
    public BodyCreationSettings()
        : base(JPH_BodyCreationSettings_Create())
    {
    }

    public BodyCreationSettings(ShapeSettings shapeSettings, in Vector3 position, in Quaternion rotation, MotionType motionType, ObjectLayer objectLayer)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use constructor with Double3");

        fixed (Vector3* positionPtr = &position)
        fixed (Quaternion* rotationPtr = &rotation)
        {
            Handle = JPH_BodyCreationSettings_Create2(shapeSettings.Handle, positionPtr, rotationPtr, motionType, objectLayer);
        }
    }

    public BodyCreationSettings(ShapeSettings shapeSettings, in Double3 position, in Quaternion rotation, MotionType motionType, ObjectLayer objectLayer)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use constructor with Vector3");

        fixed (Double3* positionPtr = &position)
        fixed (Quaternion* rotationPtr = &rotation)
        {
            Handle = JPH_BodyCreationSettings_Create2Double(shapeSettings.Handle, positionPtr, rotationPtr, motionType, objectLayer);
        }
    }

    public BodyCreationSettings(Shape shape, in Vector3 position, in Quaternion rotation, MotionType motionType, ObjectLayer objectLayer)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use constructor with Double3");

        fixed (Vector3* positionPtr = &position)
        fixed (Quaternion* rotationPtr = &rotation)
        {
            Handle = JPH_BodyCreationSettings_Create3(shape.Handle, positionPtr, rotationPtr, motionType, objectLayer);
        }
    }

    public BodyCreationSettings(Shape shape, in Double3 position, in Quaternion rotation, MotionType motionType, ObjectLayer objectLayer)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use constructor with Vector3");

        fixed (Double3* positionPtr = &position)
        fixed (Quaternion* rotationPtr = &rotation)
        {
            Handle = JPH_BodyCreationSettings_Create3Double(shape.Handle, positionPtr, rotationPtr, motionType, objectLayer);
        }
    }

    public Vector3 LinearVelocity
    {
        get
        {
            Vector3 velocity;
            JPH_BodyCreationSettings_GetLinearVelocity(Handle, &velocity);
            return velocity;
        }
        set => JPH_BodyCreationSettings_SetLinearVelocity(Handle, &value);
    }

    public Vector3 AngularVelocity
    {
        get
        {
            Vector3 velocity;
            JPH_BodyCreationSettings_GetAngularVelocity(Handle, &velocity);
            return velocity;
        }
        set => JPH_BodyCreationSettings_SetAngularVelocity(Handle, &value);
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
    ~BodyCreationSettings() => Dispose(disposing: false);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            JPH_BodyCreationSettings_Destroy(Handle);
        }
    }

    public void GetLinearVelocity(out Vector3 velocity)
    {
        Unsafe.SkipInit(out velocity);
        fixed (Vector3* velocityPtr = &velocity)
        {
            JPH_BodyCreationSettings_GetLinearVelocity(Handle, velocityPtr);
        }
    }

    public void GetAngularVelocity(out Vector3 velocity)
    {
        Unsafe.SkipInit(out velocity);
        fixed (Vector3* velocityPtr = &velocity)
        {
            JPH_BodyCreationSettings_GetAngularVelocity(Handle, velocityPtr);
        }
    }
}
