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

    public unsafe BodyCreationSettings(ShapeSettings shapeSettings, Vector3 position, Quaternion rotation, MotionType motionType, ushort objectLayer)
       : base(JPH_BodyCreationSettings_Create2(shapeSettings.Handle, &position, &rotation, motionType, objectLayer))
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
