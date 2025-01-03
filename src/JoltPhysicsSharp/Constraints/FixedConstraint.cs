// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public unsafe class FixedConstraintSettings : TwoBodyConstraintSettings
{
    public FixedConstraintSettings()
    {
        JPH_FixedConstraintSettings native;
        JPH_FixedConstraintSettings_Init(&native);

        FromNative(native);
    }

    internal FixedConstraintSettings(in JPH_FixedConstraintSettings native)
    {
        FromNative(native);
    }

    public ConstraintSpace Space { get; set; }

    public bool AutoDetectPoint { get; set; }

    public Vector3 Point1 { get; set; }

    public Vector3 AxisX1 { get; set; }

    public Vector3 AxisY1 { get; set; }

    public Vector3 Point2 { get; set; }

    public Vector3 AxisX2 { get; set; }

    public Vector3 AxisY2 { get; set; }

    public override TwoBodyConstraint CreateConstraint(in Body body1, in Body body2)
    {
        return new FixedConstraint(CreateConstraintNative(in body1, in body2));
    }

    internal nint CreateConstraintNative(in Body body1, in Body body2)
    {
        JPH_FixedConstraintSettings nativeSettings;
        ToNative(&nativeSettings);

        return JPH_FixedConstraint_Create(&nativeSettings, body1.Handle, body2.Handle);
    }

    private void FromNative(in JPH_FixedConstraintSettings native)
    {
        FromNative(native.baseSettings);

        Space = native.space;
        AutoDetectPoint = native.autoDetectPoint;
        Point1 = native.point1;
        AxisX1 = native.axisX1;
        AxisY1 = native.axisY1;
        Point2 = native.point2;
        AxisX2 = native.axisX2;
        AxisY2 = native.axisY2;
    }

    internal void ToNative(JPH_FixedConstraintSettings* native)
    {
        ToNative(ref native->baseSettings);

        native->space = Space;
        native->autoDetectPoint = AutoDetectPoint;
        native->point1 = Point1;
        native->axisX1 = AxisX1;
        native->axisY1 = AxisY1;
        native->point2 = Point2;
        native->axisX2 = AxisX2;
        native->axisY2 = AxisY2;
    }
}

public unsafe class FixedConstraint : TwoBodyConstraint
{
    internal FixedConstraint(nint handle)
        : base(handle)
    {
    }

    public FixedConstraint(FixedConstraintSettings settings, in Body body1, in Body body2)
        : base(settings.CreateConstraintNative(in body1, in body2))
    {
    }

    public FixedConstraintSettings Settings
    {
        get
        {
            JPH_FixedConstraint_GetSettings(Handle, out JPH_FixedConstraintSettings native);
            return new(native);
        }
    }


    public Vector3 TotalLambdaPosition
    {
        get
        {
            Vector3 result;
            JPH_FixedConstraint_GetTotalLambdaPosition(Handle, &result);
            return result;
        }
    }

    public Vector3 TotalLambdaRotation
    {
        get
        {
            Vector3 result;
            JPH_FixedConstraint_GetTotalLambdaRotation(Handle, &result);
            return result;
        }
    }

    public void GetTotalLambdaPosition(out Vector3 result)
    {
        Unsafe.SkipInit(out result);

        fixed (Vector3* resultPtr = &result)
        {
            JPH_FixedConstraint_GetTotalLambdaPosition(Handle, resultPtr);
        }
    }

    public void GetTotalLambdaRotation(out Vector3 result)
    {
        Unsafe.SkipInit(out result);

        fixed (Vector3* resultPtr = &result)
        {
            JPH_FixedConstraint_GetTotalLambdaRotation(Handle, resultPtr);
        }
    }
}
