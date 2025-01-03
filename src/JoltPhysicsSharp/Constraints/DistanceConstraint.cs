// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public unsafe class DistanceConstraintSettings : TwoBodyConstraintSettings
{
    public DistanceConstraintSettings()
    {
        JPH_DistanceConstraintSettings native;
        JPH_DistanceConstraintSettings_Init(&native);

        FromNative(native);
    }

    internal DistanceConstraintSettings(in JPH_DistanceConstraintSettings native)
    {
        FromNative(native);
    }

    public ConstraintSpace Space { get; set; }

    public Vector3 Point1 { get; set; }

    public Vector3 Point2 { get; set; }

    public float MinDistance { get; set; }
    public float MaxDistance { get; set; }

    public SpringSettings LimitsSpringSettings { get; set; }

    public override TwoBodyConstraint CreateConstraint(in Body body1, in Body body2)
    {
        return new DistanceConstraint(CreateConstraintNative(in body1, in body2));
    }

    internal nint CreateConstraintNative(in Body body1, in Body body2)
    {
        JPH_DistanceConstraintSettings nativeSettings;
        ToNative(&nativeSettings);

        return JPH_DistanceConstraint_Create(&nativeSettings, body1.Handle, body2.Handle);
    }

    private void FromNative(in JPH_DistanceConstraintSettings native)
    {
        FromNative(native.baseSettings);

        Space = native.space;
        Point1 = native.point1;
        Point2 = native.point2;
        MinDistance = native.minDistance;
        MaxDistance = native.maxDistance;
        LimitsSpringSettings = native.limitsSpringSettings;
    }

    internal void ToNative(JPH_DistanceConstraintSettings* native)
    {
        ToNative(ref native->baseSettings);

        native->space = Space;
        native->point1 = Point1;
        native->point2 = Point2;
        native->minDistance = MinDistance;
        native->maxDistance = MaxDistance;
        native->limitsSpringSettings = LimitsSpringSettings;
    }
}

public unsafe class DistanceConstraint : TwoBodyConstraint
{
    internal DistanceConstraint(nint handle)
        : base(handle)
    {
    }

    public DistanceConstraint(DistanceConstraintSettings settings, in Body body1, in Body body2)
        : base(settings.CreateConstraintNative(in body1, in body2))
    {
    }

    public DistanceConstraintSettings Settings
    {
        get
        {
            JPH_DistanceConstraint_GetSettings(Handle, out JPH_DistanceConstraintSettings native);
            return new(native);
        }
    }

    public float MinDistance => JPH_DistanceConstraint_GetMinDistance(Handle);
    public float MaxDistance => JPH_DistanceConstraint_GetMaxDistance(Handle);

    public SpringSettings SpringSettings
    {
        get
        {
            SpringSettings result;
            JPH_DistanceConstraint_GetLimitsSpringSettings(Handle, &result);
            return result;
        }
        set
        {
            JPH_DistanceConstraint_SetLimitsSpringSettings(Handle, &value);
        }
    }

    public float TotalLambdaPosition
    {
        get
        {
            return JPH_DistanceConstraint_GetTotalLambdaPosition(Handle);
        }
    }

    public void SetDistance(float minDistance, float maxDistance)
    {
        JPH_DistanceConstraint_SetDistance(Handle, minDistance, maxDistance);
    }
}
