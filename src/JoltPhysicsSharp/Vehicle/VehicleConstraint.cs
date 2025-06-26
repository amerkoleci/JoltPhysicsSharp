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
