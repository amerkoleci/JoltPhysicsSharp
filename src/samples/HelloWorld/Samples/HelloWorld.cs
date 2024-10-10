// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Numerics;
using JoltPhysicsSharp;

namespace HelloWorld;

public class HelloWorld : Sample
{
    private Body _floor;
    private Body _sphere;

    public HelloWorld()
    {

    }

    public override void Initialize()
    {
        base.Initialize();

        _floor = CreateFloor(100, Layers.NonMoving);
        _sphere = CreateSphere(0.5f, new Vector3(0.0f, 2.0f, 0.0f), Quaternion.Identity, MotionType.Dynamic, Layers.Moving);

        BodyInterface.SetPosition(_sphere.ID, new Vector3(100.0f, 2.0f, 0.0f), Activation.Activate);
        BodyInterface.SetLinearVelocity(_sphere.ID, new Vector3(0.0f, -5.0f, 0.0f));
    }

    public override void Run()
    {
        const float deltaTime = 1.0f / 60.0f;

        // Optional step: Before starting the physics simulation you can optimize the broad phase. This improves collision detection performance (it's pointless here because we only have 2 bodies).
        // You should definitely not call this every frame or when e.g. streaming in a new level section as it is an expensive operation.
        // Instead insert all new objects in batches instead of 1 at a time to keep the broad phase efficient.
        System!.OptimizeBroadPhase();

        uint step = 0;
        while (BodyInterface.IsActive(_sphere.ID))
        {
            // Next step
            ++step;

            // Output current position and velocity of the sphere
            Vector3 position = BodyInterface.GetCenterOfMassPosition(_sphere.ID);
            Vector3 velocity = BodyInterface.GetLinearVelocity(_sphere.ID);
            //Matrix4x4 transform = bodyInterface.GetWorldTransform(sphereID);
            //Vector3 translation = bodyInterface.GetWorldTransform(sphereID).Translation;
            //Matrix4x4 centerOfMassTransform = bodyInterface.GetCenterOfMassTransform(sphereID);
            Console.WriteLine($"Step {step} : Position = ({position}), Velocity = ({velocity})");

            // If you take larger steps than 1 / 60th of a second you need to do multiple collision steps in order to keep the simulation stable. Do 1 collision step per 1 / 60th of a second (round up).
            const int collisionSteps = 1;

            // Step the world
            PhysicsUpdateError error = System.Update(deltaTime, collisionSteps);
            Debug.Assert(error == PhysicsUpdateError.None);
        }
    }
}
