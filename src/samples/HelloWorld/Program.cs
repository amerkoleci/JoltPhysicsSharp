// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using JoltPhysicsSharp;

namespace HelloWorld;

public static class Program
{
    private const uint MaxBodies = 1024;

    private const uint NumBodyMutexes = 0;
    private const uint MaxBodyPairs = 1024;
    private const uint MaxContactConstraints = 1024;

    static class Layers
    {
        public static readonly byte NonMoving = 0;
        public static readonly byte Moving = 1;
        public static readonly byte NumLayers = 2;
    };

    public static unsafe void Main()
    {
        if (!Foundation.Init())
            return;

        {
            using TempAllocator tempAllocator = new(10 * 1024 * 1024);
            using JobSystemThreadPool jobSystem = new(Foundation.MaxPhysicsJobs, Foundation.MaxPhysicsBarriers);
            using BroadPhaseLayer broadPhaseLayer = new();
            using PhysicsSystem physicsSystem = new();
            physicsSystem.Init(MaxBodies, NumBodyMutexes, MaxBodyPairs, MaxContactConstraints, broadPhaseLayer);

            BodyInterface bodyInterface = physicsSystem.BodyInterface;

            // Next we can create a rigid body to serve as the floor, we make a large box
            // Create the settings for the collision volume (the shape). 
            // Note that for simple shapes (like boxes) you can also directly construct a BoxShape.
            BoxShapeSettings floorShapeSettings = new(new Vector3(100.0f, 1.0f, 100.0f));

            BodyCreationSettings floorSettings = new(floorShapeSettings, new Vector3(0.0f, -1.0f, 0.0f), Quaternion.Identity, MotionType.Static, Layers.NonMoving);

            // Create the actual rigid body
            Body floor = bodyInterface.CreateBody(floorSettings);

            // Add it to the world
            bodyInterface.AddBody(floor, ActivationMode.DontActivate);

            BodyCreationSettings spherSettings = new(new SphereShapeSettings(0.5f), new Vector3(0.0f, 2.0f, 0.0f), Quaternion.Identity, MotionType.Dynamic, Layers.Moving);
            BodyID sphereID = bodyInterface.CreateAndAddBody(spherSettings, ActivationMode.Activate);

            // Now you can interact with the dynamic body, in this case we're going to give it a velocity.
            // (note that if we had used CreateBody then we could have set the velocity straight on the body before adding it to the physics system)
            bodyInterface.SetLinearVelocity(sphereID, new Vector3(0.0f, -5.0f, 0.0f));

            // We simulate the physics world in discrete time steps. 60 Hz is a good rate to update the physics system.
            const float deltaTime = 1.0f / 60.0f;

            // Optional step: Before starting the physics simulation you can optimize the broad phase. This improves collision detection performance (it's pointless here because we only have 2 bodies).
            // You should definitely not call this every frame or when e.g. streaming in a new level section as it is an expensive operation.
            // Instead insert all new objects in batches instead of 1 at a time to keep the broad phase efficient.
            physicsSystem.OptimizeBroadPhase();

            uint step = 0;
            while (bodyInterface.IsActive(sphereID))
            {
                // Next step
                ++step;

                // Output current position and velocity of the sphere
                Vector3 position = bodyInterface.GetCenterOfMassPosition(sphereID);
                Vector3 velocity = bodyInterface.GetLinearVelocity(sphereID);
                Console.WriteLine($"Step {step} : Position = ({position}), Velocity = ({velocity})");

                // If you take larger steps than 1 / 60th of a second you need to do multiple collision steps in order to keep the simulation stable. Do 1 collision step per 1 / 60th of a second (round up).
                const int collisionSteps = 1;

                // If you want more accurate step results you can do multiple sub steps within a collision step. Usually you would set this to 1.
                const int integrationSubSteps = 1;

                // Step the world
                physicsSystem.Update(deltaTime, collisionSteps, integrationSubSteps, tempAllocator, jobSystem);
            }

            // Remove the sphere from the physics system. Note that the sphere itself keeps all of its state and can be re-added at any time.
            bodyInterface.RemoveBody(sphereID);

            // Destroy the sphere. After this the sphere ID is no longer valid.
            bodyInterface.DestroyBody(sphereID);

            // Remove and destroy the floor
            bodyInterface.RemoveBody(floor.ID);
            bodyInterface.DestroyBody(floor.ID);
        }

        Foundation.Shutdown();
    }
}
