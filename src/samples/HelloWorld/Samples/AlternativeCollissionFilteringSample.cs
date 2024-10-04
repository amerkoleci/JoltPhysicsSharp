// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Numerics;
using JoltPhysicsSharp;

namespace HelloWorld;

public class AlternativeCollissionFilteringSample : Sample
{
    // Define the group bits
    const uint GROUP_STATIC = 1;
    const uint GROUP_FLOOR1 = 2;
    const uint GROUP_FLOOR2 = 4;
    const uint GROUP_FLOOR3 = 8;
    const uint GROUP_ALL = GROUP_STATIC | GROUP_FLOOR1 | GROUP_FLOOR2 | GROUP_FLOOR3;

    public AlternativeCollissionFilteringSample()
    {

    }

    public override void Initialize()
    {
        base.Initialize();

        // Create a basic floor that collides with everything
        CreateBox(new Vector3(50, 0.5f, 50), Vector3.Zero, Quaternion.Identity, MotionType.Static, GetObjectLayer(GROUP_STATIC, GROUP_ALL));

        // 1st floor: Part of group static but colliding only with GROUP_FLOOR1
        CreateBox(new Vector3(5, 0.5f, 5), new Vector3(0, 3, 0), Quaternion.Identity, MotionType.Static, GetObjectLayer(GROUP_STATIC, GROUP_FLOOR1));

        // 2nd floor: Part of group static but colliding only with GROUP_FLOOR2
        CreateBox(new Vector3(5, 0.5f, 5), new Vector3(0, 6, 0), Quaternion.Identity, MotionType.Static, GetObjectLayer(GROUP_STATIC, GROUP_FLOOR2));

        // 3rd floor: Part of group static but colliding only with GROUP_FLOOR3
        CreateBox(new Vector3(5, 0.5f, 5), new Vector3(0, 9, 0), Quaternion.Identity, MotionType.Static, GetObjectLayer(GROUP_STATIC, GROUP_FLOOR3));

        // Create dynamic objects that collide only with a specific floor or the ground floor
        for (int x = 0; x < 10; x++)
        {
            for (int z = 0; z < 10; z++)
            {
                for (int l = 0; l < 3; l++)
                {
                    // Create physics body
                    Vector3 pos = new(1.2f * x - 5.4f, 12 + 2 * l, 1.2f * z - 5.4f);
                    Vector3 size = new(0.5f, 0.5f, 0.5f);
                    Shape? shape = null;
                    uint layer = 0;
                    switch (l)
                    {
                        case 0:
                            shape = new BoxShape(new Vector3(0.5f, 0.5f, 0.5f));
                            layer = GROUP_FLOOR1;
                            break;
                        case 1:
                            shape = new SphereShape(0.5f);
                            layer = GROUP_FLOOR2;
                            break;
                        case 2:
                            shape = new CapsuleShape(0.5f, 0.5f);
                            layer = GROUP_FLOOR3;
                            break;
                    }
                    using BodyCreationSettings creationSettings = new(shape!, pos, Quaternion.Identity, MotionType.Dynamic, GetObjectLayer(layer, GROUP_ALL));
                    Body body = BodyInterface.CreateBody(creationSettings);
                    BodyInterface.AddBody(body.ID, Activation.Activate);
                }
            }
        }
    }

    protected override void SetupCollisionFiltering()
    {
        // Layer that objects can be in, determines which other objects it can collide with
        // Typically you at least want to have 1 layer for moving bodies and 1 layer for static bodies, but you can have more
        // layers if you want. E.g. you could have a layer for high detail collision (which is not used by the physics simulation
        // but only if you do collision testing).
        const uint NUM_BROAD_PHASE_LAYERS = 2;

        BroadPhaseLayerInterfaceMask broadPhaseLayerInterface = new(NUM_BROAD_PHASE_LAYERS);
        broadPhaseLayerInterface.ConfigureLayer(BroadPhaseLayers.NonMoving, GROUP_STATIC, 0); // Anything that has the static bit set goes into the static broadphase layer
        broadPhaseLayerInterface.ConfigureLayer(BroadPhaseLayers.Moving, GROUP_FLOOR1 | GROUP_FLOOR2 | GROUP_FLOOR3, 0); // Anything that has one of the floor bits set goes into the dynamic broadphase layer

        _settings.ObjectLayerPairFilter = new ObjectLayerPairFilterMask();
        _settings.BroadPhaseLayerInterface = broadPhaseLayerInterface;
        _settings.ObjectVsBroadPhaseLayerFilter = new ObjectVsBroadPhaseLayerFilterMask(broadPhaseLayerInterface);
    }

    private static ObjectLayer GetObjectLayer(uint group, uint mask)
    {
        return ObjectLayerPairFilterMask.GetObjectLayer(group, mask);
    }

    public override void Dispose()
    {
        base.Dispose();
    }

    public override void Run()
    {
        const float deltaTime = 1.0f / 60.0f;

        // Optional step: Before starting the physics simulation you can optimize the broad phase. This improves collision detection performance (it's pointless here because we only have 2 bodies).
        // You should definitely not call this every frame or when e.g. streaming in a new level section as it is an expensive operation.
        // Instead insert all new objects in batches instead of 1 at a time to keep the broad phase efficient.
        System!.OptimizeBroadPhase();

        uint step = 0;
        while (step < 10000)
        {
            // Next step
            ++step;

            // If you take larger steps than 1 / 60th of a second you need to do multiple collision steps in order to keep the simulation stable. Do 1 collision step per 1 / 60th of a second (round up).
            const int collisionSteps = 1;

            // Step the world
            PhysicsUpdateError error = System.Update(deltaTime, collisionSteps);
            Debug.Assert(error == PhysicsUpdateError.None);
        }
    }
}
