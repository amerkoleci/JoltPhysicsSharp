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

public unsafe class TwoBodyConstraint : Constraint
{
    internal TwoBodyConstraint(nint handle, bool owns = true)
       : base(handle, owns)
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
            Mat4 joltMatrix;
            JPH_TwoBodyConstraint_GetConstraintToBody1Matrix(Handle, &joltMatrix);
            return joltMatrix.FromJolt();
        }
    }

    public Matrix4x4 ConstraintToBody2Matrix
    {
        get
        {
            Mat4 joltMatrix;
            JPH_TwoBodyConstraint_GetConstraintToBody2Matrix(Handle, &joltMatrix);
            return joltMatrix.FromJolt();
        }
    }

    public void GetConstraintToBody1Matrix(out Matrix4x4 result)
    {
        Mat4 joltMatrix;
        JPH_TwoBodyConstraint_GetConstraintToBody1Matrix(Handle, &joltMatrix);
        result = joltMatrix.FromJolt();
    }

    public void GetConstraintToBody2Matrix(out Matrix4x4 result)
    {
        Mat4 joltMatrix;
        JPH_TwoBodyConstraint_GetConstraintToBody2Matrix(Handle, &joltMatrix);
        result = joltMatrix.FromJolt();
    }


    internal static TwoBodyConstraint? GetObject(nint handle) => GetOrAddObject(handle, h => new TwoBodyConstraint(h, false));
}
