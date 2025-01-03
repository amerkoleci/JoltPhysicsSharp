// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class TwoBodyConstraintSettings : ConstraintSettings
{
    public abstract TwoBodyConstraint CreateConstraint(in Body body1, in Body body2);
}

public abstract unsafe class TwoBodyConstraint : Constraint
{
    protected TwoBodyConstraint(nint handle)
        : base(handle)
    {
    }

    public Body Body1
    {
        get => Body.GetObject(JPH_TwoBodyConstraint_GetBody1(Handle))!;
    }

    public Body Body2
    {
        get => Body.GetObject(JPH_TwoBodyConstraint_GetBody2(Handle))!;
    }

    public Matrix4x4 ConstraintToBody1Matrix
    {
        get
        {
            JPH_TwoBodyConstraint_GetConstraintToBody1Matrix(Handle, out Matrix4x4 result);
            return result;
        }
    }

    public Matrix4x4 ConstraintToBody2Matrix
    {
        get
        {
            JPH_TwoBodyConstraint_GetConstraintToBody2Matrix(Handle, out Matrix4x4 result);
            return result;
        }
    }

    public void GetConstraintToBody1Matrix(out Matrix4x4 result)
    {
        JPH_TwoBodyConstraint_GetConstraintToBody1Matrix(Handle, out result);
    }

    public void GetConstraintToBody2Matrix(out Matrix4x4 result)
    {
        JPH_TwoBodyConstraint_GetConstraintToBody2Matrix(Handle, out result);
    }
}
