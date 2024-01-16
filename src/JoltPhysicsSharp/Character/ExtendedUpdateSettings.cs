// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;
public class ExtendedUpdateSettings : NativeObject
{
    public ExtendedUpdateSettings()
        : base(JPH_ExtendedUpdateSettings_Create())
    {
    }

    ~ExtendedUpdateSettings() => Dispose(isDisposing: false);

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            JPH_ExtendedUpdateSettings_Destroy(Handle);
        }
    }

    public void SetStickToFloorStepDown(in Vector3 stickToFloorStepDown)
    {
        JPH_ExtendedUpdateSettings_SetStickToFloorStepDown(Handle, stickToFloorStepDown);
    }

    public void SetWalkStairsStepUp(in Vector3 walkStairsStepUp)
    {
        JPH_ExtendedUpdateSettings_SetWalkStairsStepUp(Handle, walkStairsStepUp);
    }
}
