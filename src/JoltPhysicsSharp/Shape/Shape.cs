// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class ShapeSettings : NativeObject
{
    protected ShapeSettings()
    {
    }

    protected ShapeSettings(nint handle)
        : base(handle)
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="ShapeSettings" /> class.
    /// </summary>
    ~ShapeSettings() => Dispose(disposing: false);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            JPH_ShapeSettings_Destroy(Handle);
        }
    }

    //public abstract Shape Create();
}


public abstract class Shape : NativeObject
{
    protected Shape(nint handle)
        : base(handle)
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="Shape" /> class.
    /// </summary>
    ~Shape() => Dispose(isDisposing: false);

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            JPH_Shape_Destroy(Handle);
        }
    }

    public unsafe BoundingBox LocalBounds
    {
        get
        {
            BoundingBox result = default;
            JPH_Shape_GetLocalBounds(Handle, &result);
            return result;
        }
    }

    public MassProperties MassProperties
    {
        get
        {
            JPH_Shape_GetMassProperties(Handle, out MassProperties properties);
            return properties;
        }
    }

    public float InnerRadius => JPH_Shape_GetInnerRadius(Handle);

    public Vector3 CenterOfMass
    {
        get
        {
            JPH_Shape_GetCenterOfMass(Handle, out Vector3 value);
            return value;
        }
    }

    public void GetCenterOfMass(out Vector3 result)
    {
        JPH_Shape_GetCenterOfMass(Handle, out result);
    }
}
