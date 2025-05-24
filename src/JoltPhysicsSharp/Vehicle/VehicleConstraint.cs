// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class VehicleConstraint : Constraint
{
    public VehicleConstraint(Body body, VehicleConstraintSettings constraintSettings)
        : this(JPH_VehicleConstraint_Create(body.Handle, constraintSettings.Handle))
    {
        OwnedObjects.TryAdd(constraintSettings.Handle, constraintSettings);
    }

    protected VehicleConstraint(nint handle)
        : base(handle)
    {
    }

    public WheeledVehicleController WheeledVehicleController
    {
        // NOTE: BGE: we exposed and used the ctor overload to specify that the given handle isn't owned in this new object.
        get { return new WheeledVehicleController(JPH_VehicleConstraint_GetWheeledVehicleController(Handle), false); }
    }

    public PhysicsStepListener AsPhysicsStepListener
    {
        get { return new PhysicsStepListener(JPH_VehicleConstraint_AsPhysicsStepListener(Handle), false); }
    }

    public void SetVehicleCollisionTester(VehicleCollisionTester tester)
    {
        JPH_VehicleConstraint_SetVehicleCollisionTester(Handle, tester.Handle);
    }
}
