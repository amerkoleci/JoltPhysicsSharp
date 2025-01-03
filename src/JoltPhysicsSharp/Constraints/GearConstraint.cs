// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public unsafe class GearConstraintSettings : TwoBodyConstraintSettings
{
    public GearConstraintSettings()
    {
        JPH_GearConstraintSettings native;
        JPH_GearConstraintSettings_Init(&native);

        FromNative(native);
    }

    internal GearConstraintSettings(in JPH_GearConstraintSettings native)
    {
        FromNative(native);
    }

    public ConstraintSpace Space { get; set; }
    public Vector3 HingeAxis1 { get; set; }
    public Vector3 HingeAxis2 { get; set; }
    public float Ratio { get; set; }

    public override TwoBodyConstraint CreateConstraint(in Body body1, in Body body2)
    {
        return new GearConstraint(CreateConstraintNative(in body1, in body2));
    }

    internal nint CreateConstraintNative(in Body body1, in Body body2)
    {
        JPH_GearConstraintSettings nativeSettings;
        ToNative(&nativeSettings);

        return JPH_GearConstraint_Create(&nativeSettings, body1.Handle, body2.Handle);
    }

    private void FromNative(in JPH_GearConstraintSettings native)
    {
        FromNative(native.baseSettings);

        Space = native.space;
        HingeAxis1 = native.hingeAxis1;
        HingeAxis2 = native.hingeAxis2;
        Ratio = native.ratio;
    }

    internal void ToNative(JPH_GearConstraintSettings* native)
    {
        ToNative(ref native->baseSettings);

        native->space = Space;
        native->hingeAxis1 = HingeAxis1;
        native->hingeAxis2 = HingeAxis2;
        native->ratio = Ratio;
    }
}

public sealed class GearConstraint : TwoBodyConstraint
{
    internal GearConstraint(nint handle)
        : base(handle)
    {
    }

    public GearConstraint(GearConstraintSettings settings, in Body body1, in Body body2)
        : base(settings.CreateConstraintNative(in body1, in body2))
    {
    }

    public GearConstraintSettings Settings
    {
        get
        {
            JPH_GearConstraint_GetSettings(Handle, out JPH_GearConstraintSettings native);
            return new(native);
        }
    }

    public float TotalLambda => JPH_GearConstraint_GetTotalLambda(Handle);
}
