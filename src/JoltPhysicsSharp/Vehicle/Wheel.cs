// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class WheelSettings : NativeObject
{
    public WheelSettings()
        : base(JPH_WheelSettings_Create())
    {
    }

    internal WheelSettings(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_WheelSettings_Destroy(Handle);
    }

    public Vector3 Position
    {
        get
        {
            JPH_WheelSettings_GetPosition(Handle, out Vector3 result);
            return result;
        }
        set => JPH_WheelSettings_SetPosition(Handle, value);
    }

    public Vector3 SuspensionForcePoint
    {
        get
        {
            JPH_WheelSettings_GetSuspensionForcePoint(Handle, out Vector3 result);
            return result;
        }
        set => JPH_WheelSettings_SetSuspensionForcePoint(Handle, value);
    }

    public Vector3 SuspensionDirection
    {
        get
        {
            JPH_WheelSettings_GetSuspensionDirection(Handle, out Vector3 result);
            return result;
        }
        set => JPH_WheelSettings_SetSuspensionDirection(Handle, value);
    }

    public Vector3 SteeringAxis
    {
        get
        {
            JPH_WheelSettings_GetSteeringAxis(Handle, out Vector3 result);
            return result;
        }
        set => JPH_WheelSettings_SetSteeringAxis(Handle, value);
    }

    public Vector3 WheelUp
    {
        get
        {
            JPH_WheelSettings_GetWheelUp(Handle, out Vector3 result);
            return result;
        }
        set => JPH_WheelSettings_SetWheelUp(Handle, value);
    }

    public Vector3 WheelForward
    {
        get
        {
            JPH_WheelSettings_GetWheelForward(Handle, out Vector3 result);
            return result;
        }
        set => JPH_WheelSettings_SetWheelForward(Handle, value);
    }

    public float SuspensionMinLength
    {
        get => JPH_WheelSettings_GetSuspensionMinLength(Handle);
        set => JPH_WheelSettings_SetSuspensionMinLength(Handle, value);
    }

    public float SuspensionMaxLength
    {
        get => JPH_WheelSettings_GetSuspensionMaxLength(Handle);
        set => JPH_WheelSettings_SetSuspensionMaxLength(Handle, value);
    }

    public float SuspensionPreloadLength
    {
        get => JPH_WheelSettings_GetSuspensionPreloadLength(Handle);
        set => JPH_WheelSettings_SetSuspensionPreloadLength(Handle, value);
    }

    public SpringSettings SuspensionSpring
    {
        get
        {
            JPH_WheelSettings_GetSuspensionSpring(Handle, out SpringSettings result);
            return result;
        }
        set => JPH_WheelSettings_SetSuspensionSpring(Handle, value);
    }

    public float Radius
    {
        get => JPH_WheelSettings_GetRadius(Handle);
        set => JPH_WheelSettings_SetRadius(Handle, value);
    }

    public float Width
    {
        get => JPH_WheelSettings_GetWidth(Handle);
        set => JPH_WheelSettings_SetWidth(Handle, value);
    }

    public bool EnableSuspensionForcePoint
    {
        get => JPH_WheelSettings_GetEnableSuspensionForcePoint(Handle);
        set => JPH_WheelSettings_SetEnableSuspensionForcePoint(Handle, value);
    }

    internal static WheelSettings? GetObject(nint handle) => GetOrAddObject(handle, (nint h) => new WheelSettings(h, false));
}

public class Wheel : NativeObject
{
    public Wheel(WheelSettings settings)
        : base(JPH_Wheel_Create(settings.Handle))
    {
    }

    internal Wheel(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_Wheel_Destroy(Handle);
    }

    public WheelSettings Settings => WheelSettings.GetObject(JPH_Wheel_GetSettings(Handle))!;

    public float AngularVelocity
    {
        get => JPH_Wheel_GetAngularVelocity(Handle);
        set => JPH_Wheel_SetAngularVelocity(Handle, value);
    }

    public float RotationAngle
    {
        get => JPH_Wheel_GetRotationAngle(Handle);
        set => JPH_Wheel_SetRotationAngle(Handle, value);
    }

    public float SteerAngle
    {
        get => JPH_Wheel_GetSteerAngle(Handle);
        set => JPH_Wheel_SetSteerAngle(Handle, value);
    }

    public bool HasContact => JPH_Wheel_HasContact(Handle);
    public bool HasHitHardPoint => JPH_Wheel_HasHitHardPoint(Handle);
    public BodyID ContactBodyID => JPH_Wheel_GetContactBodyID(Handle);
    public SubShapeID ContactSubShapeID => JPH_Wheel_GetContactSubShapeID(Handle);

    public Vector3 ContactPosition
    {
        get
        {
            JPH_Wheel_GetContactPosition(Handle, out Vector3 result);
            return result;
        }
    }

    public Vector3 ContactPointVelocity
    {
        get
        {
            JPH_Wheel_GetContactPointVelocity(Handle, out Vector3 result);
            return result;
        }
    }

    public Vector3 ContactNormal
    {
        get
        {
            JPH_Wheel_GetContactNormal(Handle, out Vector3 result);
            return result;
        }
    }

    public Vector3 ContactLongitudinal
    {
        get
        {
            JPH_Wheel_GetContactLongitudinal(Handle, out Vector3 result);
            return result;
        }
    }

    public Vector3 ContactLateral
    {
        get
        {
            JPH_Wheel_GetContactLateral(Handle, out Vector3 result);
            return result;
        }
    }

    public float SuspensionLength => JPH_Wheel_GetSuspensionLength(Handle);
    public float SuspensionLambda => JPH_Wheel_GetSuspensionLambda(Handle);
    public float LongitudinalLambda => JPH_Wheel_GetLongitudinalLambda(Handle);
    public float LateralLambda => JPH_Wheel_GetLateralLambda(Handle);

    internal static Wheel? GetObject(nint handle) => GetOrAddObject(handle, (nint h) => new Wheel(h, false));

    internal static T? GetObject<T>(nint handle) where T : Wheel
    {
        return GetOrAddObject<T>(handle);
    }
}
