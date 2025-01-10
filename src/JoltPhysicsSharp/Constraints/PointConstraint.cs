// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed unsafe class PointConstraintSettings : TwoBodyConstraintSettings
{
    public PointConstraintSettings()
    {
        JPH_PointConstraintSettings native;
        JPH_PointConstraintSettings_Init(&native);

        FromNative(native);

    }

    internal PointConstraintSettings(in JPH_PointConstraintSettings native)
    {
        FromNative(native);
    }

    public ConstraintSpace Space { get; set; }
    public Vector3 Point1 { get; set; }
    public Vector3 Point2 { get; set; }

    public override TwoBodyConstraint CreateConstraint(in Body body1, in Body body2)
    {
        return new PointConstraint(CreateConstraintNative(in body1, in body2));
    }

    internal nint CreateConstraintNative(in Body body1, in Body body2)
    {
        JPH_PointConstraintSettings nativeSettings;
        ToNative(&nativeSettings);

        return JPH_PointConstraint_Create(&nativeSettings, body1.Handle, body2.Handle);
    }

    private void FromNative(in JPH_PointConstraintSettings native)
    {
        FromNative(native.baseSettings);

        Space = native.space;
        Point1 = native.point1;
        Point2 = native.point2;
    }

    internal void ToNative(JPH_PointConstraintSettings* native)
    {
        ToNative(ref native->baseSettings);

        native->space = Space;
        native->point1 = Point1;
        native->point2 = Point2;
    }
}

public sealed unsafe class PointConstraint : TwoBodyConstraint
{
    internal PointConstraint(nint handle)
        : base(handle)
    {
    }

    public PointConstraint(PointConstraintSettings settings, in Body body1, in Body body2)
        : base(settings.CreateConstraintNative(in body1, in body2))
    {
    }

    public PointConstraintSettings Settings
    {
        get
        {
            JPH_PointConstraint_GetSettings(Handle, out JPH_PointConstraintSettings native);
            return new(native);
        }
    }

    public void SetPoint1(ConstraintSpace space, in Vector3 value)
    {
        fixed (Vector3* valuePtr = &value)
            JPH_PointConstraint_SetPoint1(Handle, space, valuePtr);
    }

    public void SetPoint2(ConstraintSpace space, in Vector3 value)
    {
        fixed (Vector3* valuePtr = &value)
            JPH_PointConstraint_SetPoint2(Handle, space, valuePtr);
    }

    public Vector3 TotalLambdaPosition
    {
        get
        {
            JPH_PointConstraint_GetTotalLambdaPosition(Handle, out Vector3 result);
            return result;
        }
    }

    public void GetTotalLambdaPosition(out Vector3 result)
    {
        JPH_PointConstraint_GetTotalLambdaPosition(Handle, out result);
    }
}
