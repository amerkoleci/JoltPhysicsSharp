// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

#include "joltc.h"

#include <Jolt/Jolt.h>
#include <Jolt/RegisterTypes.h>
#include <Jolt/Core/Factory.h>
#include <Jolt/Core/TempAllocator.h>
#include <Jolt/Core/JobSystemThreadPool.h>
#include <Jolt/Physics/PhysicsSettings.h>
#include <Jolt/Physics/PhysicsSystem.h>
#include <Jolt/Physics/Collision/CollideShape.h>
#include <Jolt/Physics/Collision/Shape/BoxShape.h>
#include <Jolt/Physics/Collision/Shape/SphereShape.h>
#include <Jolt/Physics/Collision/Shape/TriangleShape.h>
#include <Jolt/Physics/Collision/Shape/CapsuleShape.h>
#include <Jolt/Physics/Collision/Shape/TaperedCapsuleShape.h>
#include <Jolt/Physics/Collision/Shape/CylinderShape.h>
#include <Jolt/Physics/Collision/Shape/ConvexHullShape.h>
#include <Jolt/Physics/Collision/Shape/MeshShape.h>
#include <Jolt/Physics/Collision/Shape/HeightFieldShape.h>
#include <Jolt/Physics/Body/BodyCreationSettings.h>
#include <Jolt/Physics/Body/BodyActivationListener.h>

#include <iostream>
#include <cstdarg>
#include <thread>

// All Jolt symbols are in the JPH namespace
using namespace JPH;

// Callback for traces, connect this to your own trace function if you have one
static void TraceImpl(const char* inFMT, ...)
{
    // Format the message
    va_list list;
    va_start(list, inFMT);
    char buffer[1024];
    vsnprintf(buffer, sizeof(buffer), inFMT, list);

    // Print to the TTY
    std::cout << buffer << std::endl;
}

#ifdef JPH_ENABLE_ASSERTS

// Callback for asserts, connect this to your own assert handler if you have one
static bool AssertFailedImpl(const char* inExpression, const char* inMessage, const char* inFile, uint inLine)
{
    // Print to the TTY
    std::cout << inFile << ":" << inLine << ": (" << inExpression << ") " << (inMessage != nullptr ? inMessage : "") << std::endl;

    // Breakpoint
    return true;
};

#endif // JPH_ENABLE_ASSERTS

static JPH::Vec3 ToVec3(const JPH_Vec3* vec)
{
    return JPH::Vec3(vec->x, vec->y, vec->z);
}

static JPH::Float3 ToFloat3(const JPH_Vec3* vec)
{
    return JPH::Float3(vec->x, vec->y, vec->z);
}

static void FromVec3(const JPH::Vec3& vec, JPH_Vec3* result)
{
    result->x = vec.GetX();
    result->y = vec.GetY();
    result->z = vec.GetZ();
}

static JPH::RVec3 ToRVec3(const JPH_RVec3* vec)
{
    return JPH::RVec3(vec->x, vec->y, vec->z);
}

static void FromRVec3(const JPH::RVec3& vec, JPH_RVec3* result)
{
    result->x = vec.GetX();
    result->y = vec.GetY();
    result->z = vec.GetZ();
}

static JPH::Quat ToQuat(const JPH_Quat* quat)
{
    return JPH::Quat(quat->x, quat->y, quat->z, quat->w);
}

static JPH::Triangle ToTriangle(const JPH_Triangle* triangle)
{
    return JPH::Triangle(ToFloat3(&triangle->v1), ToFloat3(&triangle->v2), ToFloat3(&triangle->v3), triangle->materialIndex);
}

static JPH::IndexedTriangle ToIndexedTriangle(const JPH_IndexedTriangle* triangle)
{
    return JPH::IndexedTriangle(triangle->i1, triangle->i2, triangle->i3, triangle->materialIndex);
}

bool JPH_Init(void)
{
    JPH::RegisterDefaultAllocator();

    // TODO
    JPH::Trace = TraceImpl;
    JPH_IF_ENABLE_ASSERTS(JPH::AssertFailed = AssertFailedImpl;)

        // Create a factory
        JPH::Factory::sInstance = new JPH::Factory();

    // Register all Jolt physics types
    JPH::RegisterTypes();

    return true;
}

void JPH_Shutdown(void)
{
    // Destroy the factory
    delete JPH::Factory::sInstance;
    JPH::Factory::sInstance = nullptr;
}

JPH_TempAllocator* JPH_TempAllocatorMalloc_Create()
{
    auto impl = new JPH::TempAllocatorMalloc();
    return reinterpret_cast<JPH_TempAllocator*>(impl);
}

JPH_TempAllocator* JPH_TempAllocator_Create(uint32_t size)
{
    auto impl = new JPH::TempAllocatorImpl(size);
    return reinterpret_cast<JPH_TempAllocator*>(impl);
}

void JPH_TempAllocator_Destroy(JPH_TempAllocator* allocator)
{
    if (allocator)
    {
        delete reinterpret_cast<JPH::TempAllocator*>(allocator);
    }
}

JPH_JobSystemThreadPool* JPH_JobSystemThreadPool_Create(uint32_t maxJobs, uint32_t maxBarriers, int inNumThreads)
{
    auto job_system = new JPH::JobSystemThreadPool(maxJobs, maxBarriers, inNumThreads);
    return reinterpret_cast<JPH_JobSystemThreadPool*>(job_system);
}

JPH_CAPI void JPH_JobSystemThreadPool_Destroy(JPH_JobSystemThreadPool* system)
{
    if (system)
    {
        delete reinterpret_cast<JPH::JobSystemThreadPool*>(system);
    }
}

/* JPH_BroadPhaseLayerInterface */
static JPH_BroadPhaseLayerInterface_Procs g_BroadPhaseLayerInterface_Procs;

class ManagedBroadPhaseLayerInterface final : public JPH::BroadPhaseLayerInterface
{
public:
    ManagedBroadPhaseLayerInterface() = default;

    ManagedBroadPhaseLayerInterface(const ManagedBroadPhaseLayerInterface&) = delete;
    ManagedBroadPhaseLayerInterface(const ManagedBroadPhaseLayerInterface&&) = delete;
    ManagedBroadPhaseLayerInterface& operator=(const ManagedBroadPhaseLayerInterface&) = delete;
    ManagedBroadPhaseLayerInterface& operator=(const ManagedBroadPhaseLayerInterface&&) = delete;

    uint GetNumBroadPhaseLayers() const override
    {
        return g_BroadPhaseLayerInterface_Procs.GetNumBroadPhaseLayers(
            reinterpret_cast<const JPH_BroadPhaseLayerInterface*>(this)
        );
    }

    BroadPhaseLayer	GetBroadPhaseLayer(ObjectLayer inLayer) const override
    {
        return (BroadPhaseLayer)g_BroadPhaseLayerInterface_Procs.GetBroadPhaseLayer(
            reinterpret_cast<const JPH_BroadPhaseLayerInterface*>(this),
            static_cast<JPH_ObjectLayer>(inLayer)
        );
    }

#if defined(JPH_EXTERNAL_PROFILE) || defined(JPH_PROFILE_ENABLED)
    const char* GetBroadPhaseLayerName(BroadPhaseLayer inLayer) const override
    {
        if (g_BroadPhaseLayerInterface_Procs.GetBroadPhaseLayerName == nullptr)
            return nullptr;

        return g_BroadPhaseLayerInterface_Procs.GetBroadPhaseLayerName(
            reinterpret_cast<const JPH_BroadPhaseLayerInterface*>(this),
            static_cast<JPH_BroadPhaseLayer>(inLayer)
        );
    }
#endif // JPH_EXTERNAL_PROFILE || JPH_PROFILE_ENABLED
};

void JPH_BroadPhaseLayerInterface_SetProcs(JPH_BroadPhaseLayerInterface_Procs procs)
{
    g_BroadPhaseLayerInterface_Procs = procs;
}

JPH_BroadPhaseLayerInterface* JPH_BroadPhaseLayerInterface_Create()
{
    auto system = new ManagedBroadPhaseLayerInterface();
    return reinterpret_cast<JPH_BroadPhaseLayerInterface*>(system);
}

void JPH_BroadPhaseLayerInterface_Destroy(JPH_BroadPhaseLayerInterface* layer)
{
    if (layer)
    {
        delete reinterpret_cast<ManagedBroadPhaseLayerInterface*>(layer);
    }
}

/* ShapeSettings */
void JPH_ShapeSettings_Destroy(JPH_ShapeSettings* settings)
{
    if (settings)
    {
        delete reinterpret_cast<JPH::ShapeSettings*>(settings);
    }
}

/* BoxShape */
JPH_BoxShapeSettings* JPH_BoxShapeSettings_Create(const JPH_Vec3* halfExtent, float convexRadius)
{
    auto settings = new JPH::BoxShapeSettings(ToVec3(halfExtent), convexRadius);
    return reinterpret_cast<JPH_BoxShapeSettings*>(settings);
}

JPH_BoxShape* JPH_BoxShape_Create(const JPH_Vec3* halfExtent, float convexRadius)
{
    auto shape = new JPH::BoxShape(ToVec3(halfExtent), convexRadius);
    return reinterpret_cast<JPH_BoxShape*>(shape);
}

void JPH_BoxShape_GetHalfExtent(const JPH_BoxShape* shape, JPH_Vec3* halfExtent)
{
    auto joltShape = reinterpret_cast<const JPH::BoxShape*>(shape);
    auto joltVector = joltShape->GetHalfExtent();
    FromVec3(joltVector, halfExtent);
}

float JPH_BoxShape_GetVolume(const JPH_BoxShape* shape)
{
    auto joltShape = reinterpret_cast<const JPH::BoxShape*>(shape);
    return joltShape->GetVolume();
}

float JPH_BoxShape_GetConvexRadius(const JPH_BoxShape* shape)
{
    auto joltShape = reinterpret_cast<const JPH::BoxShape*>(shape);
    return joltShape->GetConvexRadius();
}

/* SphereShapeSettings */
JPH_SphereShapeSettings* JPH_SphereShapeSettings_Create(float radius)
{
    auto settings = new JPH::SphereShapeSettings(radius);
    return reinterpret_cast<JPH_SphereShapeSettings*>(settings);
}

float JPH_SphereShapeSettings_GetRadius(const JPH_SphereShapeSettings* settings)
{
    JPH_ASSERT(settings);
    return reinterpret_cast<const JPH::SphereShapeSettings*>(settings)->mRadius;
}

void JPH_SphereShapeSettings_SetRadius(JPH_SphereShapeSettings* settings, float radius)
{
    JPH_ASSERT(settings);
    reinterpret_cast<JPH::SphereShapeSettings*>(settings)->mRadius = radius;
}

JPH_SphereShape* JPH_SphereShape_Create(float radius)
{
    auto shape = new JPH::SphereShape(radius);
    return reinterpret_cast<JPH_SphereShape*>(shape);
}

float JPH_SphereShape_GetRadius(const JPH_SphereShape* shape)
{
    JPH_ASSERT(shape);
    return reinterpret_cast<const JPH::SphereShape*>(shape)->GetRadius();
}

/* TriangleShapeSettings */
JPH_TriangleShapeSettings* JPH_TriangleShapeSettings_Create(const JPH_Vec3* v1, const JPH_Vec3* v2, const JPH_Vec3* v3, float convexRadius)
{
    auto settings = new JPH::TriangleShapeSettings(ToVec3(v1), ToVec3(v2), ToVec3(v3), convexRadius);
    return reinterpret_cast<JPH_TriangleShapeSettings*>(settings);
}

/* CapsuleShapeSettings */
JPH_CapsuleShapeSettings* JPH_CapsuleShapeSettings_Create(float halfHeightOfCylinder, float radius)
{
    auto settings = new JPH::CapsuleShapeSettings(halfHeightOfCylinder, radius);
    return reinterpret_cast<JPH_CapsuleShapeSettings*>(settings);
}

/* CylinderShapeSettings */
JPH_CylinderShapeSettings* JPH_CylinderShapeSettings_Create(float halfHeight, float radius, float convexRadius)
{
    auto settings = new JPH::CylinderShapeSettings(halfHeight, radius, convexRadius);
    return reinterpret_cast<JPH_CylinderShapeSettings*>(settings);
}

/* ConvexHullShape */
JPH_ConvexHullShapeSettings* JPH_ConvexHullShapeSettings_Create(const JPH_Vec3* points, uint32_t pointsCount, float maxConvexRadius)
{
    Array<Vec3> joltPoints;
    joltPoints.resize(pointsCount);

    for (uint32_t i = 0; i < pointsCount; i++)
    {
        joltPoints[i] = ToVec3(&points[i]);
    }

    auto settings = new JPH::ConvexHullShapeSettings(joltPoints, maxConvexRadius);
    return reinterpret_cast<JPH_ConvexHullShapeSettings*>(settings);
}

/* MeshShapeSettings */
JPH_MeshShapeSettings* JPH_MeshShapeSettings_Create(const JPH_Triangle* triangles, uint32_t triangleCount)
{
    TriangleList jolTriangles;
    jolTriangles.resize(triangleCount);
    for (uint32_t i = 0; i < triangleCount; ++i)
    {
        jolTriangles[i] = ToTriangle(&triangles[i]);
    }

    auto settings = new JPH::MeshShapeSettings(jolTriangles);
    return reinterpret_cast<JPH_MeshShapeSettings*>(settings);
}

JPH_MeshShapeSettings* JPH_MeshShapeSettings_Create2(const JPH_Vec3* vertices, uint32_t verticesCount, const JPH_IndexedTriangle* triangles, uint32_t triangleCount)
{
    VertexList joltVertices;
    IndexedTriangleList joltTriangles;

    joltVertices.resize(verticesCount);
    joltTriangles.resize(triangleCount);

    for (uint32_t i = 0; i < verticesCount; ++i)
    {
        joltVertices[i] = ToFloat3(&vertices[i]);
    }

    for (uint32_t i = 0; i < triangleCount; ++i)
    {
        joltTriangles[i] = ToIndexedTriangle(&triangles[i]);
    }

    auto settings = new JPH::MeshShapeSettings(joltVertices, joltTriangles);
    return reinterpret_cast<JPH_MeshShapeSettings*>(settings);
}

void JPH_MeshShapeSettings_Sanitize(JPH_MeshShapeSettings* settings)
{
    JPH_ASSERT(settings != nullptr);

    reinterpret_cast<JPH::MeshShapeSettings*>(settings)->Sanitize();
}

/* MeshShapeSettings */
JPH_HeightFieldShapeSettings* JPH_HeightFieldShapeSettings_Create(const float* samples, const JPH_Vec3* offset, const JPH_Vec3* scale, uint32_t sampleCount)
{
    auto settings = new JPH::HeightFieldShapeSettings(samples, ToVec3(offset), ToVec3(scale), sampleCount);
    return reinterpret_cast<JPH_HeightFieldShapeSettings*>(settings);
}

void JPH_MeshShapeSettings_DetermineMinAndMaxSample(const JPH_HeightFieldShapeSettings* settings, float* pOutMinValue, float* pOutMaxValue, float* pOutQuantizationScale)
{
    auto joltSettings = reinterpret_cast<const JPH::HeightFieldShapeSettings*>(settings);
    float outMinValue, outMaxValue, outQuantizationScale;
    joltSettings->DetermineMinAndMaxSample(outMinValue, outMaxValue, outQuantizationScale);
    if (pOutMinValue)
        *pOutMinValue = outMinValue;
    if (pOutMaxValue)
        *pOutMaxValue = outMaxValue;
    if (pOutQuantizationScale)
        *pOutQuantizationScale = outQuantizationScale;
}

uint32_t JPH_MeshShapeSettings_CalculateBitsPerSampleForError(const JPH_HeightFieldShapeSettings* settings, float maxError)
{
    JPH_ASSERT(settings != nullptr);

    return reinterpret_cast<const JPH::HeightFieldShapeSettings*>(settings)->CalculateBitsPerSampleForError(maxError);
}

/* TaperedCapsuleShapeSettings */
JPH_TaperedCapsuleShapeSettings* JPH_TaperedCapsuleShapeSettings_Create(float halfHeightOfTaperedCylinder, float topRadius, float bottomRadius)
{
    auto settings = new JPH::TaperedCapsuleShapeSettings(halfHeightOfTaperedCylinder, topRadius, bottomRadius);
    return reinterpret_cast<JPH_TaperedCapsuleShapeSettings*>(settings);
}

/* Shape */
void JPH_Shape_Destroy(JPH_Shape* shape)
{
    if (shape)
    {
        delete reinterpret_cast<JPH::Shape*>(shape);
    }
}

/* JPH_BodyCreationSettings */
JPH_BodyCreationSettings* JPH_BodyCreationSettings_Create()
{
    auto bodyCreationSettings = new JPH::BodyCreationSettings();
    return reinterpret_cast<JPH_BodyCreationSettings*>(bodyCreationSettings);
}

JPH_BodyCreationSettings* JPH_BodyCreationSettings_Create2(
    JPH_ShapeSettings* shapeSettings,
    const JPH_Vec3* position,
    const JPH_Quat* rotation,
    JPH_MotionType motionType,
    JPH_ObjectLayer objectLayer)
{
    JPH::ShapeSettings* joltShapeSettings = reinterpret_cast<JPH::ShapeSettings*>(shapeSettings);
    auto bodyCreationSettings = new JPH::BodyCreationSettings(
        joltShapeSettings,
        ToVec3(position),
        ToQuat(rotation),
        (JPH::EMotionType)motionType,
        objectLayer
    );
    return reinterpret_cast<JPH_BodyCreationSettings*>(bodyCreationSettings);
}

JPH_BodyCreationSettings* JPH_BodyCreationSettings_Create3(
    JPH_Shape* shape,
    const JPH_Vec3* position,
    const JPH_Quat* rotation,
    JPH_MotionType motionType,
    JPH_ObjectLayer objectLayer)
{
    JPH::Shape* joltShape = reinterpret_cast<JPH::Shape*>(shape);
    auto bodyCreationSettings = new JPH::BodyCreationSettings(
        joltShape,
        ToVec3(position),
        ToQuat(rotation),
        (JPH::EMotionType)motionType,
        objectLayer
    );
    return reinterpret_cast<JPH_BodyCreationSettings*>(bodyCreationSettings);
}
void JPH_BodyCreationSettings_Destroy(JPH_BodyCreationSettings* settings)
{
    if (settings)
    {
        delete reinterpret_cast<JPH::BodyCreationSettings*>(settings);
    }
}

/* JPH_PhysicsSystem */
JPH_PhysicsSystem* JPH_PhysicsSystem_Create(void)
{
    auto system = new JPH::PhysicsSystem();
    return reinterpret_cast<JPH_PhysicsSystem*>(system);
}

void JPH_PhysicsSystem_Destroy(JPH_PhysicsSystem* system)
{
    if (system)
    {
        delete reinterpret_cast<JPH::PhysicsSystem*>(system);
    }
}

static JPH_ObjectVsBroadPhaseLayerFilter s_objectVsBroadPhaseLayerFilter;

static bool JPH_BroadPhaseCanCollide(ObjectLayer inLayer1, BroadPhaseLayer inLayer2)
{
    JPH_ASSERT(s_objectVsBroadPhaseLayerFilter);

    return s_objectVsBroadPhaseLayerFilter(
        static_cast<JPH_ObjectLayer>(inLayer1),
        static_cast<JPH_BroadPhaseLayer>(inLayer2)
    );
}

void JPH_PhysicsSystem_Init(JPH_PhysicsSystem* system,
    uint32_t maxBodies, uint32_t numBodyMutexes, uint32_t maxBodyPairs, uint32_t maxContactConstraints,
    JPH_BroadPhaseLayer* layer,
    JPH_ObjectVsBroadPhaseLayerFilter objectVsBroadPhaseLayerFilter,
    JPH_ObjectLayerPairFilter objectLayerPairFilter)
{
    JPH_ASSERT(system);
    s_objectVsBroadPhaseLayerFilter = objectVsBroadPhaseLayerFilter;

    reinterpret_cast<JPH::PhysicsSystem*>(system)->Init(
        maxBodies, numBodyMutexes, maxBodyPairs, maxContactConstraints,
        *reinterpret_cast<const JPH::BroadPhaseLayerInterface*>(layer),
        JPH_BroadPhaseCanCollide,
        reinterpret_cast<JPH::ObjectLayerPairFilter>(objectLayerPairFilter)
    );
}

void JPH_PhysicsSystem_OptimizeBroadPhase(JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    reinterpret_cast<JPH::PhysicsSystem*>(system)->OptimizeBroadPhase();
}

void JPH_PhysicsSystem_Update(JPH_PhysicsSystem* system, float deltaTime, int collisionSteps, int integrationSubSteps,
    JPH_TempAllocator* tempAlocator,
    JPH_JobSystemThreadPool* jobSystem)
{
    JPH_ASSERT(system);

    auto joltSystem = reinterpret_cast<JPH::PhysicsSystem*>(system);
    auto joltTempAlocator = reinterpret_cast<JPH::TempAllocator*>(tempAlocator);
    auto joltJobSystem = reinterpret_cast<JPH::JobSystemThreadPool*>(jobSystem);
    joltSystem->Update(deltaTime, collisionSteps, integrationSubSteps, joltTempAlocator, joltJobSystem);
}

JPH_BodyInterface* JPH_PhysicsSystem_GetBodyInterface(JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    auto joltSystem = reinterpret_cast<JPH::PhysicsSystem*>(system);
    return reinterpret_cast<JPH_BodyInterface*>(&joltSystem->GetBodyInterface());
}

void JPH_PhysicsSystem_SetContactListener(JPH_PhysicsSystem* system, JPH_ContactListener* listener)
{
    JPH_ASSERT(system);

    auto joltSystem = reinterpret_cast<JPH::PhysicsSystem*>(system);
    auto joltListener = reinterpret_cast<JPH::ContactListener*>(listener);
    joltSystem->SetContactListener(joltListener);
}

void JPH_PhysicsSystem_SetBodyActivationListener(JPH_PhysicsSystem* system, JPH_BodyActivationListener* listener)
{
    JPH_ASSERT(system);

    auto joltSystem = reinterpret_cast<JPH::PhysicsSystem*>(system);
    auto joltListener = reinterpret_cast<JPH::BodyActivationListener*>(listener);
    joltSystem->SetBodyActivationListener(joltListener);
}

uint32_t JPH_PhysicsSystem_GetNumBodies(const JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    auto joltSystem = reinterpret_cast<const JPH::PhysicsSystem*>(system);
    return joltSystem->GetNumBodies();
}

uint32_t JPH_PhysicsSystem_GetNumActiveBodies(const JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    auto joltSystem = reinterpret_cast<const JPH::PhysicsSystem*>(system);
    return joltSystem->GetNumActiveBodies();
}

uint32_t JPH_PhysicsSystem_GetMaxBodies(const JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    auto joltSystem = reinterpret_cast<const JPH::PhysicsSystem*>(system);
    return joltSystem->GetMaxBodies();
}

JPH_Body* JPH_BodyInterface_CreateBody(JPH_BodyInterface* interface, JPH_BodyCreationSettings* settings)
{
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    auto body = joltBodyInterface->CreateBody(
        *reinterpret_cast<const JPH::BodyCreationSettings*>(settings)
    );

    return reinterpret_cast<JPH_Body*>(body);
}

JPH_Body* JPH_BodyInterface_CreateBodyWithID(JPH_BodyInterface* interface, JPH_BodyID bodyID, JPH_BodyCreationSettings* settings)
{
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    auto body = joltBodyInterface->CreateBodyWithID(
        JPH::BodyID(bodyID),
        *reinterpret_cast<const JPH::BodyCreationSettings*>(settings)
    );

    return reinterpret_cast<JPH_Body*>(body);
}

JPH_Body* JPH_BodyInterface_CreateBodyWithoutID(JPH_BodyInterface* interface, JPH_BodyCreationSettings* settings)
{
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    auto body = joltBodyInterface->CreateBodyWithoutID(
        *reinterpret_cast<const JPH::BodyCreationSettings*>(settings)
    );

    return reinterpret_cast<JPH_Body*>(body);
}

void JPH_BodyInterface_DestroyBodyWithoutID(JPH_BodyInterface* interface, JPH_Body* body)
{
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    auto joltBody = reinterpret_cast<JPH::Body*>(body);

    joltBodyInterface->DestroyBodyWithoutID(joltBody);
}

bool JPH_BodyInterface_AssignBodyID(JPH_BodyInterface* interface, JPH_Body* body)
{
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    auto joltBody = reinterpret_cast<JPH::Body*>(body);

    return joltBodyInterface->AssignBodyID(joltBody);
}

bool JPH_BodyInterface_AssignBodyID2(JPH_BodyInterface* interface, JPH_Body* body, JPH_BodyID bodyID)
{
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    auto joltBody = reinterpret_cast<JPH::Body*>(body);

    return joltBodyInterface->AssignBodyID(joltBody, JPH::BodyID(bodyID));
}

JPH_Body* JPH_BodyInterface_UnassignBodyID(JPH_BodyInterface* interface, JPH_BodyID bodyID)
{
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    auto body = joltBodyInterface->UnassignBodyID(JPH::BodyID(bodyID));
    return reinterpret_cast<JPH_Body*>(body);
}

JPH_BodyID JPH_BodyInterface_CreateAndAddBody(JPH_BodyInterface* interface, JPH_BodyCreationSettings* settings, JPH_ActivationMode activation)
{
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    JPH::BodyID bodyID = joltBodyInterface->CreateAndAddBody(
        *reinterpret_cast<const JPH::BodyCreationSettings*>(settings),
        (JPH::EActivation)activation
    );

    return bodyID.GetIndexAndSequenceNumber();
}

void JPH_BodyInterface_DestroyBody(JPH_BodyInterface* interface, JPH_BodyID bodyID)
{
    JPH_ASSERT(interface);

    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    joltBodyInterface->DestroyBody(JPH::BodyID(bodyID));
}

void JPH_BodyInterface_AddBody(JPH_BodyInterface* interface, JPH_BodyID bodyID, JPH_ActivationMode activation)
{
    JPH_ASSERT(interface);

    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    joltBodyInterface->AddBody(JPH::BodyID(bodyID), (JPH::EActivation)activation);
}

void JPH_BodyInterface_RemoveBody(JPH_BodyInterface* interface, JPH_BodyID bodyID)
{
    JPH_ASSERT(interface);

    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    joltBodyInterface->RemoveBody(JPH::BodyID(bodyID));
}

bool JPH_BodyInterface_IsActive(JPH_BodyInterface* interface, JPH_BodyID bodyID)
{
    JPH_ASSERT(interface);

    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    return joltBodyInterface->IsActive(JPH::BodyID(bodyID));
}

bool JPH_BodyInterface_IsAdded(JPH_BodyInterface* interface, JPH_BodyID bodyID)
{
    JPH_ASSERT(interface);

    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    return joltBodyInterface->IsAdded(JPH::BodyID(bodyID));
}

void JPH_BodyInterface_SetLinearVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyID, const JPH_Vec3* velocity)
{
    JPH_ASSERT(interface);

    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    joltBodyInterface->SetLinearVelocity(JPH::BodyID(bodyID), ToVec3(velocity));
}

void JPH_BodyInterface_GetLinearVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyID, JPH_Vec3* velocity)
{
    JPH_ASSERT(interface);

    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    auto joltVector = joltBodyInterface->GetLinearVelocity(JPH::BodyID(bodyID));
    FromVec3(joltVector, velocity);
}

void JPH_BodyInterface_GetCenterOfMassPosition(JPH_BodyInterface* interface, JPH_BodyID bodyID, JPH_RVec3* position)
{
    JPH_ASSERT(interface);

    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    auto joltVector = joltBodyInterface->GetCenterOfMassPosition(JPH::BodyID(bodyID));
    FromRVec3(joltVector, position);
}

JPH_MotionType JPH_BodyInterface_GetMotionType(JPH_BodyInterface* interface, JPH_BodyID bodyID)
{
    JPH_ASSERT(interface);

    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    return static_cast<JPH_MotionType>(joltBodyInterface->GetMotionType(JPH::BodyID(bodyID)));
}

void JPH_BodyInterface_SetMotionType(JPH_BodyInterface* interface, JPH_BodyID bodyID, JPH_MotionType motionType, JPH_ActivationMode activationMode)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->SetMotionType(
        JPH::BodyID(bodyID),
        static_cast<JPH::EMotionType>(motionType),
        static_cast<JPH::EActivation>(activationMode)
    );
}

float JPH_BodyInterface_GetRestitution(const JPH_BodyInterface* interface, JPH_BodyID bodyID)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<const JPH::BodyInterface*>(interface);

    return joltBodyInterface->GetRestitution(JPH::BodyID(bodyID));
}

void JPH_BodyInterface_SetRestitution(JPH_BodyInterface* interface, JPH_BodyID bodyID, float restitution)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->SetRestitution(JPH::BodyID(bodyID), restitution);
}

float JPH_BodyInterface_GetFriction(const JPH_BodyInterface* interface, JPH_BodyID bodyID)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<const JPH::BodyInterface*>(interface);

    return joltBodyInterface->GetFriction(JPH::BodyID(bodyID));
}

void JPH_BodyInterface_SetFriction(JPH_BodyInterface* interface, JPH_BodyID bodyID, float friction)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->SetFriction(JPH::BodyID(bodyID), friction);
}

/* Body */
JPH_BodyID JPH_Body_GetID(const JPH_Body* body)
{
    auto joltBody = reinterpret_cast<const JPH::Body*>(body);
    return joltBody->GetID().GetIndexAndSequenceNumber();
}

bool JPH_Body_IsActive(const JPH_Body* body)
{
    return reinterpret_cast<const JPH::Body*>(body)->IsActive();
}

bool JPH_Body_IsStatic(const JPH_Body* body)
{
    return reinterpret_cast<const JPH::Body*>(body)->IsStatic();
}

bool JPH_Body_IsKinematic(JPH_Body* body)
{
    return reinterpret_cast<const JPH::Body*>(body)->IsKinematic();
}

bool JPH_Body_IsDynamic(const JPH_Body* body)
{
    return reinterpret_cast<const JPH::Body*>(body)->IsDynamic();
}

bool JPH_Body_IsSensor(const JPH_Body* body)
{
    return reinterpret_cast<const JPH::Body*>(body)->IsSensor();
}

JPH_MotionType JPH_Body_GetMotionType(const JPH_Body* body)
{
    return static_cast<JPH_MotionType>(reinterpret_cast<const JPH::Body*>(body)->GetMotionType());
}

void JPH_Body_SetMotionType(JPH_Body* body, JPH_MotionType motionType)
{
    reinterpret_cast<JPH::Body*>(body)->SetMotionType(static_cast<JPH::EMotionType>(motionType));
}

float JPH_Body_GetFriction(const JPH_Body* body)
{
    return reinterpret_cast<const JPH::Body*>(body)->GetFriction();
}

void JPH_Body_SetFriction(JPH_Body* body, float friction)
{
    reinterpret_cast<JPH::Body*>(body)->SetFriction(friction);
}

float JPH_Body_GetRestitution(const JPH_Body* body)
{
    return reinterpret_cast<const JPH::Body*>(body)->GetRestitution();
}

void JPH_Body_SetRestitution(JPH_Body* body, float restitution)
{
    reinterpret_cast<JPH::Body*>(body)->SetRestitution(restitution);
}

void JPH_Body_GetLinearVelocity(JPH_Body* body, JPH_Vec3* velocity)
{
    auto joltVector = reinterpret_cast<JPH::Body*>(body)->GetLinearVelocity();
    FromVec3(joltVector, velocity);
}

void JPH_Body_SetLinearVelocity(JPH_Body* body, const JPH_Vec3* velocity)
{
    reinterpret_cast<JPH::Body*>(body)->SetLinearVelocity(ToVec3(velocity));
}

void JPH_Body_GetAngularVelocity(JPH_Body* body, JPH_Vec3* velocity)
{
    auto joltVector = reinterpret_cast<JPH::Body*>(body)->GetAngularVelocity();
    FromVec3(joltVector, velocity);
}

void JPH_Body_SetAngularVelocity(JPH_Body* body, const JPH_Vec3* velocity)
{
    reinterpret_cast<JPH::Body*>(body)->SetAngularVelocity(ToVec3(velocity));
}

void JPH_Body_AddForce(JPH_Body* body, const JPH_Vec3* force)
{
    reinterpret_cast<JPH::Body*>(body)->AddForce(ToVec3(force));
}

void JPH_Body_AddForceAtPosition(JPH_Body* body, const JPH_Vec3* force, const JPH_RVec3* position)
{
    reinterpret_cast<JPH::Body*>(body)->AddForce(ToVec3(force), ToRVec3(position));
}

void JPH_Body_AddTorque(JPH_Body* body, const JPH_Vec3* force)
{
    reinterpret_cast<JPH::Body*>(body)->AddTorque(ToVec3(force));
}

void JPH_Body_GetAccumulatedForce(JPH_Body* body, JPH_Vec3* force)
{
    auto joltVector = reinterpret_cast<JPH::Body*>(body)->GetAccumulatedForce();
    FromVec3(joltVector, force);
}

void JPH_Body_GetAccumulatedTorque(JPH_Body* body, JPH_Vec3* force)
{
    auto joltVector = reinterpret_cast<JPH::Body*>(body)->GetAccumulatedTorque();
    FromVec3(joltVector, force);
}

void JPH_Body_AddImpulse(JPH_Body* body, const JPH_Vec3* impulse)
{
    reinterpret_cast<JPH::Body*>(body)->AddImpulse(ToVec3(impulse));
}

void JPH_Body_AddImpulseAtPosition(JPH_Body* body, const JPH_Vec3* impulse, const JPH_RVec3* position)
{
    reinterpret_cast<JPH::Body*>(body)->AddImpulse(ToVec3(impulse), ToRVec3(position));
}

void JPH_Body_AddAngularImpulse(JPH_Body* body, const JPH_Vec3* angularImpulse)
{
    reinterpret_cast<JPH::Body*>(body)->AddAngularImpulse(ToVec3(angularImpulse));
}

/* Contact Listener */
static JPH_ContactListener_Procs g_ContactListener_Procs;

class ManagedContactListener final : public JPH::ContactListener
{
public:
    ValidateResult OnContactValidate(const Body& inBody1, const Body& inBody2, RVec3Arg inBaseOffset, const CollideShapeResult& inCollisionResult) override
    {
        JPH_UNUSED(inCollisionResult);
        JPH_RVec3 baseOffset;
        FromRVec3(inBaseOffset, &baseOffset);

        JPH_ValidateResult result = g_ContactListener_Procs.OnContactValidate(
            reinterpret_cast<JPH_ContactListener*>(this),
            reinterpret_cast<const JPH_Body*>(&inBody1),
            reinterpret_cast<const JPH_Body*>(&inBody2),
            &baseOffset,
            nullptr
        );

        return (JPH::ValidateResult)result;
    }

    void OnContactAdded(const Body& inBody1, const Body& inBody2, const ContactManifold& inManifold, ContactSettings& ioSettings) override
    {
        JPH_UNUSED(inManifold);
        JPH_UNUSED(ioSettings);

        g_ContactListener_Procs.OnContactAdded(
            reinterpret_cast<JPH_ContactListener*>(this),
            reinterpret_cast<const JPH_Body*>(&inBody1),
            reinterpret_cast<const JPH_Body*>(&inBody2)
        );
    }

    void OnContactPersisted(const Body& inBody1, const Body& inBody2, const ContactManifold& inManifold, ContactSettings& ioSettings) override
    {
        JPH_UNUSED(inManifold);
        JPH_UNUSED(ioSettings);

        g_ContactListener_Procs.OnContactPersisted(
            reinterpret_cast<JPH_ContactListener*>(this),
            reinterpret_cast<const JPH_Body*>(&inBody1),
            reinterpret_cast<const JPH_Body*>(&inBody2)
        );
    }

    void OnContactRemoved(const SubShapeIDPair& inSubShapePair) override
    {
        g_ContactListener_Procs.OnContactRemoved(
            reinterpret_cast<JPH_ContactListener*>(this),
            reinterpret_cast<const JPH_SubShapeIDPair*>(&inSubShapePair)
        );
    }
};

void JPH_ContactListener_SetProcs(JPH_ContactListener_Procs procs)
{
    g_ContactListener_Procs = procs;
}

JPH_ContactListener* JPH_ContactListener_Create()
{
    auto impl = new ManagedContactListener();
    return reinterpret_cast<JPH_ContactListener*>(impl);
}

void JPH_ContactListener_Destroy(JPH_ContactListener* listener)
{
    if (listener)
    {
        delete reinterpret_cast<ManagedContactListener*>(listener);
    }
}

/* BodyActivationListener */
static JPH_BodyActivationListener_Procs g_BodyActivationListener_Procs;

class ManagedBodyActivationListener final : public JPH::BodyActivationListener
{
public:
    void OnBodyActivated(const BodyID& inBodyID, uint64 inBodyUserData) override
    {
        g_BodyActivationListener_Procs.OnBodyActivated(
            reinterpret_cast<JPH_BodyActivationListener*>(this),
            inBodyID.GetIndexAndSequenceNumber(),
            inBodyUserData
        );
    }

    void OnBodyDeactivated(const BodyID& inBodyID, uint64 inBodyUserData) override
    {
        g_BodyActivationListener_Procs.OnBodyDeactivated(
            reinterpret_cast<JPH_BodyActivationListener*>(this),
            inBodyID.GetIndexAndSequenceNumber(),
            inBodyUserData
        );
    }
};

void JPH_BodyActivationListener_SetProcs(JPH_BodyActivationListener_Procs procs)
{
    g_BodyActivationListener_Procs = procs;
}

JPH_BodyActivationListener* JPH_BodyActivationListener_Create()
{
    auto impl = new ManagedBodyActivationListener();
    return reinterpret_cast<JPH_BodyActivationListener*>(impl);
}

void JPH_BodyActivationListener_Destroy(JPH_BodyActivationListener* listener)
{
    if (listener)
    {
        delete reinterpret_cast<ManagedBodyActivationListener*>(listener);
    }
}
