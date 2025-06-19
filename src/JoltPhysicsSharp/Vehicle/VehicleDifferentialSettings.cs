// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public struct VehicleDifferentialSettings 
{
    public int LeftWheel { get; set; }
    public int RightWheel { get; set; }
    public float DifferentialRatio { get; set; }
    public float LeftRightSplit { get; set; }
    public float LimitedSlipRatio { get; set; }
    public float EngineTorqueRatio { get; set; }

    public unsafe VehicleDifferentialSettings()
    {
        JPH_VehicleDifferentialSettings native;
        JPH_VehicleDifferentialSettings_Init(&native);

        FromNative(native);
    }

    internal void FromNative(in JPH_VehicleDifferentialSettings native)
    {
        LeftWheel = native.leftWheel;
        RightWheel = native.rightWheel;
        DifferentialRatio = native.differentialRatio;
        LeftRightSplit = native.leftRightSplit;
        LimitedSlipRatio = native.limitedSlipRatio;
        EngineTorqueRatio = native.engineTorqueRatio;
    }

    internal unsafe void ToNative(JPH_VehicleDifferentialSettings* native)
    {
        native->leftWheel = LeftWheel;
        native->rightWheel = RightWheel;
        native->differentialRatio = DifferentialRatio;
        native->leftRightSplit = LeftRightSplit;
        native->limitedSlipRatio = LimitedSlipRatio;
        native->engineTorqueRatio = EngineTorqueRatio;
    }
}
