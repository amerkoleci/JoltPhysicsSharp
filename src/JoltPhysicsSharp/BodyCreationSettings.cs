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

    public BodyCreationSettings(ShapeSettings shapeSettings, in Vector3 position, in Quaternion rotation, MotionType motionType, ObjectLayer objectLayer)
       : base(DoublePrecision ? JPH_BodyCreationSettings_Create2_Double(shapeSettings.Handle, new(position), rotation, motionType, objectLayer) : JPH_BodyCreationSettings_Create2(shapeSettings.Handle, position, rotation, motionType, objectLayer))
    {
    }

    public BodyCreationSettings(Shape shape, in Vector3 position, in Quaternion rotation, MotionType motionType, ObjectLayer objectLayer)
      : base(DoublePrecision ? JPH_BodyCreationSettings_Create3_Double(shape.Handle, new(position), rotation, motionType, objectLayer) : JPH_BodyCreationSettings_Create3(shape.Handle, position, rotation, motionType, objectLayer))
    {
    }

    public BodyCreationSettings(ShapeSettings shapeSettings, in double3 position, in Quaternion rotation, MotionType motionType, ObjectLayer objectLayer)
       : base(DoublePrecision ? JPH_BodyCreationSettings_Create2_Double(shapeSettings.Handle, position, rotation, motionType, objectLayer) : JPH_BodyCreationSettings_Create2(shapeSettings.Handle, (Vector3)position, rotation, motionType, objectLayer))
    {
    }

    public BodyCreationSettings(Shape shape, in double3 position, in Quaternion rotation, MotionType motionType, ObjectLayer objectLayer)
       : base(DoublePrecision ? JPH_BodyCreationSettings_Create3_Double(shape.Handle, position, rotation, motionType, objectLayer) : JPH_BodyCreationSettings_Create3(shape.Handle, (Vector3)position, rotation, motionType, objectLayer))
    {
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
}
