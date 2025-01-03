// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public unsafe class ConeConstraintSettings : TwoBodyConstraintSettings
{
    public ConeConstraintSettings()
    {
        JPH_ConeConstraintSettings native;
        JPH_ConeConstraintSettings_Init(&native);

        FromNative(native);
    }

    internal ConeConstraintSettings(in JPH_ConeConstraintSettings native)
    {
        FromNative(native);
    }

    public ConstraintSpace Space { get; set; }

    public Vector3 Point1 { get; set; }
    public Vector3 TwistAxis1 { get; set; }
    public Vector3 Point2 { get; set; }
    public Vector3 TwistAxis2 { get; set; }
    public float HalfConeAngle { get; set; }

    public override TwoBodyConstraint CreateConstraint(in Body body1, in Body body2)
    {
        return new ConeConstraint(CreateConstraintNative(in body1, in body2));
    }

    internal nint CreateConstraintNative(in Body body1, in Body body2)
    {
        JPH_ConeConstraintSettings nativeSettings;
        ToNative(&nativeSettings);

        return JPH_ConeConstraint_Create(&nativeSettings, body1.Handle, body2.Handle);
    }

    private void FromNative(in JPH_ConeConstraintSettings native)
    {
        FromNative(native.baseSettings);

        Space = native.space;
        Point1 = native.point1;
        TwistAxis1 = native.twistAxis1;
        Point2 = native.point2;
        TwistAxis2 = native.twistAxis2;
        HalfConeAngle = native.halfConeAngle;
    }

    internal void ToNative(JPH_ConeConstraintSettings* native)
    {
        ToNative(ref native->baseSettings);

        native->space = Space;
        native->point1 = Point1;
        native->twistAxis1 = TwistAxis1;
        native->point2 = Point2;
        native->twistAxis2 = TwistAxis2;
        native->halfConeAngle = HalfConeAngle;
    }
}

public unsafe class ConeConstraint : TwoBodyConstraint
{
    internal ConeConstraint(nint handle)
        : base(handle)
    {
    }

    public ConeConstraint(ConeConstraintSettings settings, in Body body1, in Body body2)
        : base(settings.CreateConstraintNative(in body1, in body2))
    {
    }

    public ConeConstraintSettings Settings
    {
        get
        {
            JPH_ConeConstraint_GetSettings(Handle, out JPH_ConeConstraintSettings native);
            return new(native);
        }
    }

    public float CosHalfConeAngle
    {
        get => JPH_ConeConstraint_GetCosHalfConeAngle(Handle);
        set => JPH_ConeConstraint_SetHalfConeAngle(Handle, value);
    }

    public Vector3 TotalLambdaPosition
    {
        get
        {
            JPH_ConeConstraint_GetTotalLambdaPosition(Handle, out Vector3 result);
            return result;
        }
    }

    public float TotalLambdaRotation => JPH_ConeConstraint_GetTotalLambdaRotation(Handle);
}
