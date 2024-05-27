// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public unsafe class ConeConstraintSettings : TwoBodyConstraintSettings
{
    internal ConeConstraintSettings(nint handle)
        : base(handle)
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="DistanceConstraintSettings" /> class.
    /// </summary>
    ~ConeConstraintSettings() => Dispose(disposing: false);

    public Vector3 Point1
    {
        get
        {
            Vector3 result;
            JPH_ConeConstraintSettings_GetPoint1(Handle, &result);
            return result;
        }
        set
        {
            JPH_ConeConstraintSettings_SetPoint1(Handle, &value);
        }
    }

    public Vector3 Point2
    {
        get
        {
            Vector3 result;
            JPH_ConeConstraintSettings_GetPoint2(Handle, &result);
            return result;
        }
        set
        {
            JPH_ConeConstraintSettings_SetPoint2(Handle, &value);
        }
    }

    public Vector3 TwistAxis1
    {
        get
        {
            Vector3 result;
            JPH_ConeConstraintSettings_GetTwistAxis1(Handle, &result);
            return result;
        }
        set
        {
            JPH_ConeConstraintSettings_SetTwistAxis1(Handle, &value);
        }
    }

    public Vector3 TwistAxis2
    {
        get
        {
            Vector3 result;
            JPH_ConeConstraintSettings_GetTwistAxis2(Handle, &result);
            return result;
        }
        set
        {
            JPH_ConeConstraintSettings_SetTwistAxis2(Handle, &value);
        }
    }

    public float CosHalfConeAngle
    {
        get => JPH_ConeConstraintSettings_GetHalfConeAngle(Handle);
        set => JPH_ConeConstraintSettings_SetHalfConeAngle(Handle, value);
    }

    public override TwoBodyConstraint CreateConstraint(in Body body1, in Body body2)
    {
        return new ConeConstraint(JPH_ConeConstraintSettings_CreateConstraint(Handle, body1.Handle, body2.Handle));
    }
}

public unsafe class ConeConstraint : TwoBodyConstraint
{
    internal ConeConstraint(nint handle)
        : base(handle)
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="ConeConstraint" /> class.
    /// </summary>
    ~ConeConstraint() => Dispose(disposing: false);

    public float CosHalfConeAngle
    {
        get => JPH_ConeConstraint_GetCosHalfConeAngle(Handle);
        set => JPH_ConeConstraint_SetHalfConeAngle(Handle, value);
    }

    public Vector3 TotalLambdaPosition
    {
        get
        {
            Vector3 result;
            JPH_ConeConstraint_GetTotalLambdaPosition(Handle, &result);
            return result;
        }
    }

    public float TotalLambdaRotation
    {
        get
        {
            return JPH_ConeConstraint_GetTotalLambdaRotation(Handle);
        }
    }
}
