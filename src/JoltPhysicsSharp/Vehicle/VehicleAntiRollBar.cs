// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public struct VehicleAntiRollBar
{
    public int LeftWheel { get; set; }
    public int RightWheel { get; set; }
    public float Stiffness { get; set; }

    public unsafe VehicleAntiRollBar()
    {
        JPH_VehicleAntiRollBar native;
        JPH_VehicleAntiRollBar_Init(&native);

        FromNative(native);
    }

    internal void FromNative(in JPH_VehicleAntiRollBar native)
    {
        LeftWheel = native.leftWheel;
        RightWheel = native.rightWheel;
        Stiffness = native.stiffness;
    }

    internal unsafe void ToNative(JPH_VehicleAntiRollBar* native)
    {
        native->leftWheel = LeftWheel;
        native->rightWheel = RightWheel;
        native->stiffness = Stiffness;
    }
}
