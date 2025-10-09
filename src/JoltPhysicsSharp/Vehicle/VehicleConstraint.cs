// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class VehicleConstraint : Constraint, IPhysicsStepListener
{
    public VehicleConstraint(Body body, VehicleConstraintSettings settings)
        : base(settings.CreateConstraintNative(body))
    {
    }

    internal VehicleConstraint(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    nint IPhysicsStepListener.Handle => JPH_VehicleConstraint_AsPhysicsStepListener(Handle);

    public Body VehicleBody
    {
        get => Body.GetObject(JPH_VehicleConstraint_GetVehicleBody(Handle))!;
    }

    public T GetController<T>() where T : VehicleController
    {
        return VehicleController.GetObject<T>(JPH_VehicleConstraint_GetController(Handle))!;
    }

    public int WheelsCount => JPH_VehicleConstraint_GetWheelsCount(Handle);

    public Wheel GetWheel(int index)
    {
        if (index < 0 || index >= WheelsCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }

        return Wheel.GetObject(JPH_VehicleConstraint_GetWheel(Handle, index))!;
    }

    public TWheel GetWheel<TWheel>(int index)
        where TWheel : Wheel
    {
        if (index < 0 || index >= WheelsCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }

        return Wheel.GetObject<TWheel>(JPH_VehicleConstraint_GetWheel(Handle, index))!;
    }

    public IEnumerable<Wheel> GetWheels()
    {
        for (int i = 0; i < WheelsCount; i++)
        {
            yield return GetWheel(i);
        }
    }

    public IEnumerable<TWheel> GetWheels<TWheel>()
        where TWheel : Wheel
    {
        for (int i = 0; i < WheelsCount; i++)
        {
            yield return GetWheel<TWheel>(i);
        }
    }

    public void GetWheelLocalBasis(Wheel wheel, out Vector3 forward, out Vector3 up, out Vector3 right)
    {
        JPH_VehicleConstraint_GetWheelLocalBasis(Handle, wheel.Handle, out forward, out up, out right);
    }

    public unsafe Matrix4x4 GetWheelLocalTransform(int wheelIndex, in Vector3 wheelRight, in Vector3 wheelUp)
    {
        Mat4 joltMatrix;
        JPH_VehicleConstraint_GetWheelLocalTransform(Handle, wheelIndex, in wheelRight, in wheelUp, &joltMatrix);
        return joltMatrix.FromJolt();
    }

    public unsafe Matrix4x4 GetWheelWorldTransform(int wheelIndex, in Vector3 wheelRight, in Vector3 wheelUp)
    {
        Mat4 joltMatrix;
        JPH_VehicleConstraint_GetWheelWorldTransform(Handle, wheelIndex, in wheelRight, in wheelUp, &joltMatrix);
        return joltMatrix.FromJolt();
    }

    public void SetMaxPitchRollAngle(float maxPitchRollAngle)
    {
        JPH_VehicleConstraint_SetMaxPitchRollAngle(Handle, maxPitchRollAngle);
    }

    public void SetVehicleCollisionTester(VehicleCollisionTester tester)
    {
        JPH_VehicleConstraint_SetVehicleCollisionTester(Handle, tester.Handle);
    }

    public void OverrideGravity(in Vector3 gravity)
    {
        JPH_VehicleConstraint_OverrideGravity(Handle, in gravity);
    }

    public bool IsGravityOverridden() => JPH_VehicleConstraint_IsGravityOverridden(Handle);

    public Vector3 GetGravityOverride()
    {
        JPH_VehicleConstraint_GetGravityOverride(Handle, out Vector3 gravity);
        return gravity;
    }

    public void GetGravityOverride(out Vector3 gravity)
    {
        JPH_VehicleConstraint_GetGravityOverride(Handle, out gravity);
    }

    public void ResetGravityOverride()
    {
        JPH_VehicleConstraint_ResetGravityOverride(Handle);
    }

    internal static VehicleConstraint? GetObject(nint handle) => GetOrAddObject(handle, (nint h) => new VehicleConstraint(h, false));
}
