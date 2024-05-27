// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class HingeConstraintSettings : TwoBodyConstraintSettings
{
    public HingeConstraintSettings()
        : base(JPH_HingeConstraintSettings_Create())
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="HingeConstraintSettings" /> class.
    /// </summary>
    ~HingeConstraintSettings() => Dispose(disposing: false);

    public override TwoBodyConstraint CreateConstraint(in Body body1, in Body body2)
    {
        return new PointConstraint(JPH_HingeConstraintSettings_CreateConstraint(Handle, body1.Handle, body2.Handle));
    }

    public Vector3 Point1
    {
        get
        {
            JPH_HingeConstraintSettings_GetPoint1(Handle, out Vector3 value);
            return value;
        }
        set
        {
            JPH_HingeConstraintSettings_SetPoint1(Handle, value);
        }
    }

    public Vector3 Point2
    {
        get
        {
            JPH_HingeConstraintSettings_GetPoint2(Handle, out Vector3 value);
            return value;
        }
        set
        {
            JPH_HingeConstraintSettings_SetPoint2(Handle, value);
        }
    }

    public void GetPoint1(out Vector3 value)
    {
        JPH_HingeConstraintSettings_GetPoint1(Handle, out value);
    }

    public void GetPoint2(out Vector3 value)
    {
        JPH_HingeConstraintSettings_GetPoint2(Handle, out value);
    }
}

public sealed class HingeConstraint : TwoBodyConstraint
{
    internal HingeConstraint(nint handle)
        : base(handle)
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="HingeConstraint" /> class.
    /// </summary>
    ~HingeConstraint() => Dispose(disposing: false);
}
