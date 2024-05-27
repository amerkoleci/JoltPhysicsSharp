// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly record struct ExtendedUpdateSettings
{
    public ExtendedUpdateSettings()
    {
        StickToFloorStepDown = new Vector3(0, -0.5f, 0);
        WalkStairsStepUp = new Vector3(0, 0.4f, 0);
        WalkStairsMinStepForward = 0.02f;
        WalkStairsStepForwardTest = 0.15f;
        WalkStairsCosAngleForwardContact  = MathF.Cos(MathUtil.DegreesToRadians(75.0f));
        WalkStairsStepDownExtra = Vector3.Zero;
    }

    public Vector3 StickToFloorStepDown { get; init; }
    public Vector3 WalkStairsStepUp { get; init; }
    public float WalkStairsMinStepForward { get; init; }
    public float WalkStairsStepForwardTest { get; init; }
    public float WalkStairsCosAngleForwardContact { get; init; }
    public Vector3 WalkStairsStepDownExtra { get; init; }
}
