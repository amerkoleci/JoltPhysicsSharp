// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public unsafe class DistanceConstraintSettings : TwoBodyConstraintSettings
{
    internal DistanceConstraintSettings(nint handle)
        : base(handle)
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="DistanceConstraintSettings" /> class.
    /// </summary>
    ~DistanceConstraintSettings() => Dispose(disposing: false);

    public ConstraintSpace Space
    {
        get => JPH_DistanceConstraintSettings_GetSpace(Handle);
        set => JPH_DistanceConstraintSettings_SetSpace(Handle, value);
    }

    public Vector3 Point1
    {
        get
        {
            JPH_DistanceConstraintSettings_GetPoint1(Handle, out Vector3 result);
            return result;
        }
        set
        {
            JPH_DistanceConstraintSettings_SetPoint1(Handle, in value);
        }
    }

    public Vector3 Point2
    {
        get
        {
            JPH_DistanceConstraintSettings_GetPoint2(Handle, out Vector3 result);
            return result;
        }
        set
        {
            JPH_DistanceConstraintSettings_SetPoint2(Handle, in value);
        }
    }

    public override TwoBodyConstraint CreateConstraint(in Body body1, in Body body2)
    {
        return new DistanceConstraint(JPH_DistanceConstraintSettings_CreateConstraint(Handle, body1.Handle, body2.Handle));
    }
}

public unsafe class DistanceConstraint : TwoBodyConstraint
{
    internal DistanceConstraint(nint handle)
        : base(handle)
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="DistanceConstraint" /> class.
    /// </summary>
    ~DistanceConstraint() => Dispose(disposing: false);

    public float MinDisptance => JPH_DistanceConstraint_GetMinDistance(Handle);
    public float MaxDisptance => JPH_DistanceConstraint_GetMaxDistance(Handle);

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
