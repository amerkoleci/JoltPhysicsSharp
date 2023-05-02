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
#include <Jolt/Physics/Collision/Shape/StaticCompoundShape.h>
#include <Jolt/Physics/Collision/Shape/MutableCompoundShape.h>
#include <Jolt/Physics/Body/BodyCreationSettings.h>
#include <Jolt/Physics/Body/BodyActivationListener.h>
#include <Jolt/Physics/Collision/RayCast.h>
#include <Jolt/Physics/Collision/NarrowPhaseQuery.h>
#include <Jolt/Physics/Constraints/PointConstraint.h>

#ifdef _MSC_VER
#	pragma warning(push)
#	pragma warning(disable : 5045) // Compiler will insert Spectre mitigation for memory load if /Qspectre switch specified
#endif

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

static JPH::Vec3 ToVec3(const JPH_Vec3& vec)
{
    return JPH::Vec3(vec.x, vec.y, vec.z);
}

static JPH::Float3 ToFloat3(const JPH_Vec3& vec)
{
    return JPH::Float3(vec.x, vec.y, vec.z);
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

static void FromJolt(const JPH::Quat& quat, JPH_Quat* result)
{
    result->x = quat.GetX();
    result->y = quat.GetY();
    result->z = quat.GetZ();
    result->w = quat.GetW();
}

static void FromJolt(const JPH::RMat44& matrix, JPH_Matrix4x4* result)
{
    JPH::Vec4 column0 = matrix.GetColumn4(0);
    JPH::Vec4 column1 = matrix.GetColumn4(1);
    JPH::Vec4 column2 = matrix.GetColumn4(2);
    JPH::Vec4 column3 = matrix.GetColumn4(3);

    result->m11 = column0.GetX();
    result->m12 = column0.GetY();
    result->m13 = column0.GetZ();
    result->m14 = column0.GetW();

    result->m21 = column1.GetX();
    result->m22 = column1.GetY();
    result->m23 = column1.GetZ();
    result->m24 = column1.GetW();

    result->m31 = column2.GetX();
    result->m32 = column2.GetY();
    result->m33 = column2.GetZ();
    result->m34 = column2.GetW();

    result->m41 = column3.GetX();
    result->m42 = column3.GetY();
    result->m43 = column3.GetZ();
    result->m44 = column3.GetW();
}

static JPH::Triangle ToTriangle(const JPH_Triangle& triangle)
{
    return JPH::Triangle(ToFloat3(triangle.v1), ToFloat3(triangle.v2), ToFloat3(triangle.v3), triangle.materialIndex);
}

static JPH::IndexedTriangle ToIndexedTriangle(const JPH_IndexedTriangle& triangle)
{
    return JPH::IndexedTriangle(triangle.i1, triangle.i2, triangle.i3, triangle.materialIndex);
}

JPH_Bool32 JPH_Init(void)
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
    // Unregisters all types with the factory and cleans up the default material
    JPH::UnregisterTypes();

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

/* JPH_ObjectVsBroadPhaseLayerFilter */
static JPH_ObjectVsBroadPhaseLayerFilter_Procs g_ObjectVsBroadPhaseLayerFilter_Procs;

class ManagedObjectVsBroadPhaseLayerFilter final : public JPH::ObjectVsBroadPhaseLayerFilter
{
public:
    ManagedObjectVsBroadPhaseLayerFilter() = default;

    ManagedObjectVsBroadPhaseLayerFilter(const ManagedObjectVsBroadPhaseLayerFilter&) = delete;
    ManagedObjectVsBroadPhaseLayerFilter(const ManagedObjectVsBroadPhaseLayerFilter&&) = delete;
    ManagedObjectVsBroadPhaseLayerFilter& operator=(const ManagedObjectVsBroadPhaseLayerFilter&) = delete;
    ManagedObjectVsBroadPhaseLayerFilter& operator=(const ManagedObjectVsBroadPhaseLayerFilter&&) = delete;

    bool ShouldCollide(JPH::ObjectLayer inLayer1, JPH::BroadPhaseLayer inLayer2) const override
    {
        return g_ObjectVsBroadPhaseLayerFilter_Procs.ShouldCollide(
            reinterpret_cast<const JPH_ObjectVsBroadPhaseLayerFilter*>(this),
            static_cast<JPH_ObjectLayer>(inLayer1),
            static_cast<JPH_BroadPhaseLayer>(inLayer2)
        ) == 1;
    }
};

void JPH_ObjectVsBroadPhaseLayerFilter_SetProcs(JPH_ObjectVsBroadPhaseLayerFilter_Procs procs)
{
    g_ObjectVsBroadPhaseLayerFilter_Procs = procs;
}

JPH_ObjectVsBroadPhaseLayerFilter* JPH_ObjectVsBroadPhaseLayerFilter_Create()
{
    auto filter = new ManagedObjectVsBroadPhaseLayerFilter();
    return reinterpret_cast<JPH_ObjectVsBroadPhaseLayerFilter*>(filter);
}

void JPH_ObjectVsBroadPhaseLayerFilter_Destroy(JPH_ObjectVsBroadPhaseLayerFilter* filter)
{
    if (filter)
    {
        delete reinterpret_cast<ManagedObjectVsBroadPhaseLayerFilter*>(filter);
    }
}

/* JPH_ObjectLayerPairFilter */
static JPH_ObjectLayerPairFilter_Procs g_ObjectLayerPairFilter_Procs;

class ManagedObjectLayerPairFilter final : public JPH::ObjectLayerPairFilter
{
public:
    ManagedObjectLayerPairFilter() = default;

    ManagedObjectLayerPairFilter(const ManagedObjectLayerPairFilter&) = delete;
    ManagedObjectLayerPairFilter(const ManagedObjectLayerPairFilter&&) = delete;
    ManagedObjectLayerPairFilter& operator=(const ManagedObjectLayerPairFilter&) = delete;
    ManagedObjectLayerPairFilter& operator=(const ManagedObjectLayerPairFilter&&) = delete;

    bool ShouldCollide(JPH::ObjectLayer inObject1, JPH::ObjectLayer inObject2) const override
    {
        return g_ObjectLayerPairFilter_Procs.ShouldCollide(
            reinterpret_cast<const JPH_ObjectLayerPairFilter*>(this),
            static_cast<JPH_ObjectLayer>(inObject1),
            static_cast<JPH_ObjectLayer>(inObject2)
        ) == 1;
    }
};

void JPH_ObjectLayerPairFilter_SetProcs(JPH_ObjectLayerPairFilter_Procs procs)
{
    g_ObjectLayerPairFilter_Procs = procs;
}

JPH_ObjectLayerPairFilter* JPH_ObjectLayerPairFilter_Create()
{
    auto filter = new ManagedObjectLayerPairFilter();
    return reinterpret_cast<JPH_ObjectLayerPairFilter*>(filter);
}

void JPH_ObjectLayerPairFilter_Destroy(JPH_ObjectLayerPairFilter* filter)
{
    if (filter)
    {
        delete reinterpret_cast<ManagedObjectLayerPairFilter*>(filter);
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
    joltPoints.reserve(pointsCount);

    for (size_t i = 0; i < joltPoints.size(); i++)
    {
        joltPoints.push_back(ToVec3(points[i]));
    }

    auto settings = new JPH::ConvexHullShapeSettings(joltPoints, maxConvexRadius);
    return reinterpret_cast<JPH_ConvexHullShapeSettings*>(settings);
}

/* MeshShapeSettings */
JPH_MeshShapeSettings* JPH_MeshShapeSettings_Create(const JPH_Triangle* triangles, uint32_t triangleCount)
{
    TriangleList jolTriangles;
    jolTriangles.reserve(triangleCount);

    for (uint32_t i = 0; i < triangleCount; ++i)
    {
        jolTriangles.push_back(ToTriangle(triangles[i]));
    }

    auto settings = new JPH::MeshShapeSettings(jolTriangles);
    return reinterpret_cast<JPH_MeshShapeSettings*>(settings);
}

JPH_MeshShapeSettings* JPH_MeshShapeSettings_Create2(const JPH_Vec3* vertices, uint32_t verticesCount, const JPH_IndexedTriangle* triangles, uint32_t triangleCount)
{
    VertexList joltVertices;
    IndexedTriangleList joltTriangles;

    joltVertices.reserve(verticesCount);
    joltTriangles.reserve(triangleCount);

    for (size_t i = 0; i < joltVertices.size(); ++i)
    {
        joltVertices.push_back(ToFloat3(vertices[i]));
    }

    for (uint32_t i = 0; i < triangleCount; ++i)
    {
        joltTriangles.push_back(ToIndexedTriangle(triangles[i]));
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

/* CompoundShape */
void JPH_CompoundShapeSettings_AddShape(JPH_CompoundShapeSettings* settings, const JPH_Vec3* position, const JPH_Quat* rotation, const JPH_ShapeSettings* shape, uint32_t userData)
{
    auto joltShapeSettings = reinterpret_cast<const JPH::ShapeSettings*>(shape);
    auto joltSettings = reinterpret_cast<JPH::CompoundShapeSettings*>(settings);
    joltSettings->AddShape(ToVec3(position), ToQuat(rotation), joltShapeSettings, userData);
}

void JPH_CompoundShapeSettings_AddShape2(JPH_CompoundShapeSettings* settings, const JPH_Vec3* position, const JPH_Quat* rotation, const JPH_Shape* shape, uint32_t userData)
{
    auto joltShape = reinterpret_cast<const JPH::Shape*>(shape);
    auto joltSettings = reinterpret_cast<JPH::CompoundShapeSettings*>(settings);
    joltSettings->AddShape(ToVec3(position), ToQuat(rotation), joltShape, userData);
}

JPH_StaticCompoundShapeSettings* JPH_StaticCompoundShapeSettings_Create()
{
    auto settings = new JPH::StaticCompoundShapeSettings();
    return reinterpret_cast<JPH_StaticCompoundShapeSettings*>(settings);
}

JPH_CAPI JPH_MutableCompoundShapeSettings* JPH_MutableCompoundShapeSettings_Create()
{
    auto settings = new JPH::MutableCompoundShapeSettings();
    return reinterpret_cast<JPH_MutableCompoundShapeSettings*>(settings);
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
    const JPH_RVec3* position,
    const JPH_Quat* rotation,
    JPH_MotionType motionType,
    JPH_ObjectLayer objectLayer)
{
    JPH::ShapeSettings* joltShapeSettings = reinterpret_cast<JPH::ShapeSettings*>(shapeSettings);
    auto bodyCreationSettings = new JPH::BodyCreationSettings(
        joltShapeSettings,
        ToRVec3(position),
        ToQuat(rotation),
        (JPH::EMotionType)motionType,
        objectLayer
    );
    return reinterpret_cast<JPH_BodyCreationSettings*>(bodyCreationSettings);
}

JPH_BodyCreationSettings* JPH_BodyCreationSettings_Create3(
    const JPH_Shape* shape,
    const JPH_RVec3* position,
    const JPH_Quat* rotation,
    JPH_MotionType motionType,
    JPH_ObjectLayer objectLayer)
{
    const JPH::Shape* joltShape = reinterpret_cast<const JPH::Shape*>(shape);
    auto bodyCreationSettings = new JPH::BodyCreationSettings(
        joltShape,
        ToRVec3(position),
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

/* JPH_ConstraintSettings */
void JPH_ConstraintSettings_Destroy(JPH_ConstraintSettings* settings)
{
    if (settings)
    {
        delete reinterpret_cast<JPH::ConstraintSettings*>(settings);
    }
}

void JPH_Constraint_Destroy(JPH_Constraint* contraint)
{
    if (contraint)
    {
        delete reinterpret_cast<JPH::Constraint*>(contraint);
    }
}

/* JPH_TwoBodyConstraintSettings */

/* JPH_PointConstraintSettings */
JPH_PointConstraintSettings* JPH_PointConstraintSettings_Create(void)
{
    auto settings = new JPH::PointConstraintSettings();
    return reinterpret_cast<JPH_PointConstraintSettings*>(settings);
}

JPH_ConstraintSpace JPH_PointConstraintSettings_GetSpace(JPH_PointConstraintSettings* settings)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::PointConstraintSettings*>(settings);

    return static_cast<JPH_ConstraintSpace>(joltSettings->mSpace);
}

void JPH_PointConstraintSettings_SetSpace(JPH_PointConstraintSettings* settings, JPH_ConstraintSpace space)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::PointConstraintSettings*>(settings);

    joltSettings->mSpace = static_cast<JPH::EConstraintSpace>(space);
}

void JPH_PointConstraintSettings_GetPoint1(JPH_PointConstraintSettings* settings, JPH_RVec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::PointConstraintSettings*>(settings);

    auto joltVector = joltSettings->mPoint1;
    FromRVec3(joltVector, result);
}

void JPH_PointConstraintSettings_SetPoint1(JPH_PointConstraintSettings* settings, const JPH_RVec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::PointConstraintSettings*>(settings);

    joltSettings->mPoint1 = ToRVec3(value);
}

void JPH_PointConstraintSettings_GetPoint2(JPH_PointConstraintSettings* settings, JPH_RVec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::PointConstraintSettings*>(settings);

    auto joltVector = joltSettings->mPoint2;
    FromRVec3(joltVector, result);
}

void JPH_PointConstraintSettings_SetPoint2(JPH_PointConstraintSettings* settings, const JPH_RVec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::PointConstraintSettings*>(settings);

    joltSettings->mPoint2 = ToRVec3(value);
}

JPH_PointConstraint* JPH_PointConstraintSettings_CreateConstraint(JPH_PointConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2)
{
    auto joltBody1 = reinterpret_cast<JPH::Body*>(body1);
    auto joltBody2 = reinterpret_cast<JPH::Body*>(body2);
    JPH::TwoBodyConstraint* constraint = reinterpret_cast<JPH::PointConstraintSettings*>(settings)->Create(*joltBody1, *joltBody2);
    return reinterpret_cast<JPH_PointConstraint*>(static_cast<JPH::PointConstraint*>(constraint));
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

void JPH_PhysicsSystem_Init(JPH_PhysicsSystem* system,
    uint32_t maxBodies, uint32_t numBodyMutexes, uint32_t maxBodyPairs, uint32_t maxContactConstraints,
    JPH_BroadPhaseLayer* layer,
    JPH_ObjectVsBroadPhaseLayerFilter* objectVsBroadPhaseLayerFilter,
    JPH_ObjectLayerPairFilter* objectLayerPairFilter)
{
    JPH_ASSERT(system);
    JPH_ASSERT(layer);
    JPH_ASSERT(objectVsBroadPhaseLayerFilter);
    JPH_ASSERT(objectLayerPairFilter);

    reinterpret_cast<JPH::PhysicsSystem*>(system)->Init(
        maxBodies, numBodyMutexes, maxBodyPairs, maxContactConstraints,
        *reinterpret_cast<const JPH::BroadPhaseLayerInterface*>(layer),
        *reinterpret_cast<const JPH::ObjectVsBroadPhaseLayerFilter*>(objectVsBroadPhaseLayerFilter),
        *reinterpret_cast<const JPH::ObjectLayerPairFilter*>(objectLayerPairFilter)
    );
}

void JPH_PhysicsSystem_OptimizeBroadPhase(JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    reinterpret_cast<JPH::PhysicsSystem*>(system)->OptimizeBroadPhase();
}

JPH_PhysicsUpdateError JPH_PhysicsSystem_Update(JPH_PhysicsSystem* system, float deltaTime, int collisionSteps, int integrationSubSteps,
    JPH_TempAllocator* tempAlocator,
    JPH_JobSystemThreadPool* jobSystem)
{
    JPH_ASSERT(system);

    auto joltSystem = reinterpret_cast<JPH::PhysicsSystem*>(system);
    auto joltTempAlocator = reinterpret_cast<JPH::TempAllocator*>(tempAlocator);
    auto joltJobSystem = reinterpret_cast<JPH::JobSystemThreadPool*>(jobSystem);
    return static_cast<JPH_PhysicsUpdateError>(joltSystem->Update(deltaTime, collisionSteps, integrationSubSteps, joltTempAlocator, joltJobSystem));
}

JPH_BodyInterface* JPH_PhysicsSystem_GetBodyInterface(JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    auto joltSystem = reinterpret_cast<JPH::PhysicsSystem*>(system);
    return reinterpret_cast<JPH_BodyInterface*>(&joltSystem->GetBodyInterface());
}

JPH_BodyInterface* JPH_PhysicsSystem_GetBodyInterfaceNoLock(JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    auto joltSystem = reinterpret_cast<JPH::PhysicsSystem*>(system);
    return reinterpret_cast<JPH_BodyInterface*>(&joltSystem->GetBodyInterfaceNoLock());
}

const JPH_BodyLockInterface* JPC_PhysicsSystem_GetBodyLockInterface(const JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    auto joltSystem = reinterpret_cast<const JPH::PhysicsSystem*>(system);
    return reinterpret_cast<const JPH_BodyLockInterface*>(&joltSystem->GetBodyLockInterface());
}
const JPH_BodyLockInterface* JPC_PhysicsSystem_GetBodyLockInterfaceNoLock(const JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    auto joltSystem = reinterpret_cast<const JPH::PhysicsSystem*>(system);
    return reinterpret_cast<const JPH_BodyLockInterface*>(&joltSystem->GetBodyLockInterfaceNoLock());
}

const JPH_NarrowPhaseQuery* JPC_PhysicsSystem_GetNarrowPhaseQuery(const JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    auto joltSystem = reinterpret_cast<const JPH::PhysicsSystem*>(system);
    return reinterpret_cast<const JPH_NarrowPhaseQuery*>(&joltSystem->GetNarrowPhaseQuery());
}
const JPH_NarrowPhaseQuery* JPC_PhysicsSystem_GetNarrowPhaseQueryNoLock(const JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    auto joltSystem = reinterpret_cast<const JPH::PhysicsSystem*>(system);
    return reinterpret_cast<const JPH_NarrowPhaseQuery*>(&joltSystem->GetNarrowPhaseQueryNoLock());
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

void JPH_PhysicsSystem_SetGravity(JPH_PhysicsSystem* system, const JPH_Vec3* value)
{
    JPH_ASSERT(system);

    auto joltSystem = reinterpret_cast<JPH::PhysicsSystem*>(system);
    joltSystem->SetGravity(ToVec3(value));
}

void JPH_PhysicsSystem_GetGravity(JPH_PhysicsSystem* system, JPH_Vec3* result)
{
    JPH_ASSERT(system);

    auto joltSystem = reinterpret_cast<JPH::PhysicsSystem*>(system);
    auto joltVector = joltSystem->GetGravity();
    FromVec3(joltVector, result);
}

void JPH_PhysicsSystem_AddConstraint(JPH_PhysicsSystem* system, JPH_Constraint* constraint)
{
    JPH_ASSERT(system);
    JPH_ASSERT(constraint);

    auto joltSystem = reinterpret_cast<JPH::PhysicsSystem*>(system);
    auto joltConstraint = reinterpret_cast<JPH::Constraint*>(constraint);
    joltSystem->AddConstraint(joltConstraint);
}

void JPH_PhysicsSystem_RemoveConstraint(JPH_PhysicsSystem* system, JPH_Constraint* constraint)
{
    JPH_ASSERT(system);
    JPH_ASSERT(constraint);

    auto joltSystem = reinterpret_cast<JPH::PhysicsSystem*>(system);
    auto joltConstraint = reinterpret_cast<JPH::Constraint*>(constraint);
    joltSystem->RemoveConstraint(joltConstraint);
}

JPH_CAPI void JPH_PhysicsSystem_AddConstraints(JPH_PhysicsSystem* system, JPH_Constraint** constraints, uint32_t count)
{
    JPH_ASSERT(system);
    JPH_ASSERT(constraints);
    JPH_ASSERT(count > 0);

    Array<Constraint*> joltConstraints(count);
    for (uint32_t i = 0; i < count; ++i)
    {
        auto joltConstraint = reinterpret_cast<JPH::Constraint*>(constraints[i]);
        joltConstraints.push_back(joltConstraint);
    }

    auto joltSystem = reinterpret_cast<JPH::PhysicsSystem*>(system);
    joltSystem->AddConstraints(joltConstraints.data(), (int)joltConstraints.size());
}

JPH_CAPI void JPH_PhysicsSystem_RemoveConstraints(JPH_PhysicsSystem* system, JPH_Constraint** constraints, uint32_t count)
{
    JPH_ASSERT(system);
    JPH_ASSERT(constraints);
    JPH_ASSERT(count > 0);

    Array<Constraint*> joltConstraints(count);
    for (uint32_t i = 0; i < count; ++i)
    {
        auto joltConstraint = reinterpret_cast<JPH::Constraint*>(constraints[i]);
        joltConstraints.push_back(joltConstraint);
    }

    auto joltSystem = reinterpret_cast<JPH::PhysicsSystem*>(system);
    joltSystem->RemoveConstraints(joltConstraints.data(), (int)joltConstraints.size());
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

JPH_Bool32 JPH_BodyInterface_AssignBodyID(JPH_BodyInterface* interface, JPH_Body* body)
{
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    auto joltBody = reinterpret_cast<JPH::Body*>(body);

    return joltBodyInterface->AssignBodyID(joltBody);
}

JPH_Bool32 JPH_BodyInterface_AssignBodyID2(JPH_BodyInterface* interface, JPH_Body* body, JPH_BodyID bodyID)
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

JPH_Bool32 JPH_BodyInterface_IsActive(JPH_BodyInterface* interface, JPH_BodyID bodyID)
{
    JPH_ASSERT(interface);

    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    return joltBodyInterface->IsActive(JPH::BodyID(bodyID));
}

JPH_Bool32 JPH_BodyInterface_IsAdded(JPH_BodyInterface* interface, JPH_BodyID bodyID)
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

void JPH_BodyInterface_SetPosition(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* position, JPH_ActivationMode activationMode)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->SetPosition(JPH::BodyID(bodyId), ToRVec3(position), static_cast<JPH::EActivation>(activationMode));
}

void JPH_BodyInterface_GetPosition(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* result)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    FromRVec3(joltBodyInterface->GetPosition(JPH::BodyID(bodyId)), result);
}

void JPH_BodyInterface_SetRotation(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Quat* rotation, JPH_ActivationMode activationMode)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->SetRotation(JPH::BodyID(bodyId), ToQuat(rotation), static_cast<JPH::EActivation>(activationMode));
}

void JPH_BodyInterface_GetRotation(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Quat* result)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    FromJolt(joltBodyInterface->GetRotation(JPH::BodyID(bodyId)), result);
}

void JPH_BodyInterface_SetPositionAndRotation(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* position, JPH_Quat* rotation, JPH_ActivationMode activationMode)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->SetPositionAndRotation(JPH::BodyID(bodyId), ToRVec3(position), ToQuat(rotation), static_cast<JPH::EActivation>(activationMode));
}

void JPH_BodyInterface_SetPositionAndRotationWhenChanged(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* position, JPH_Quat* rotation, JPH_ActivationMode activationMode)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->SetPositionAndRotationWhenChanged(JPH::BodyID(bodyId), ToRVec3(position), ToQuat(rotation), static_cast<JPH::EActivation>(activationMode));
}

void JPH_BodyInterface_SetPositionRotationAndVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* position, JPH_Quat* rotation, JPH_Vec3* linearVelocity, JPH_Vec3* angularVelocity)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->SetPositionRotationAndVelocity(JPH::BodyID(bodyId), ToRVec3(position), ToQuat(rotation), ToVec3(linearVelocity), ToVec3(angularVelocity));
}

void JPH_BodyInterface_SetShape(JPH_BodyInterface* interface, JPH_BodyID bodyId, const JPH_Shape* shape, JPH_Bool32 updateMassProperties, JPH_ActivationMode activationMode)
{
    JPH_ASSERT(interface);
    auto jolyBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    auto jphShape = reinterpret_cast<const JPH::Shape*>(shape);

    // !! is to make ms compiler happy.
    jolyBodyInterface->SetShape(JPH::BodyID(bodyId), jphShape, !!updateMassProperties, static_cast<JPH::EActivation>(activationMode));
}

void JPH_BodyInterface_NotifyShapeChanged(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* previousCenterOfMass, JPH_Bool32 updateMassProperties, JPH_ActivationMode activationMode)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->NotifyShapeChanged(JPH::BodyID(bodyId), ToVec3(previousCenterOfMass), !!updateMassProperties, static_cast<JPH::EActivation>(activationMode));
}

void JPH_BodyInterface_ActivateBody(JPH_BodyInterface* interface, JPH_BodyID bodyId)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->ActivateBody(JPH::BodyID(bodyId));
}

void JPH_BodyInterface_DeactivateBody(JPH_BodyInterface* interface, JPH_BodyID bodyId)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->DeactivateBody(JPH::BodyID(bodyId));
}

void JPH_BodyInterface_SetObjectLayer(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_ObjectLayer layer)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->SetObjectLayer(JPH::BodyID(bodyId), layer);
}

JPH_ObjectLayer JPH_BodyInterface_GetObjectLayer(JPH_BodyInterface* interface, JPH_BodyID bodyId)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    return joltBodyInterface->GetObjectLayer(JPH::BodyID(bodyId));
}

void JPH_BodyInterface_GetWorldTransform(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Matrix4x4* result)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    auto mat = joltBodyInterface->GetWorldTransform(JPH::BodyID(bodyId));
    FromJolt(mat, result);
}

void JPH_BodyInterface_GetCenterOfMassTransform(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Matrix4x4* resutlt)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    auto mat = joltBodyInterface->GetCenterOfMassTransform(JPH::BodyID(bodyId));
    FromJolt(mat, resutlt);
}

void JPH_BodyInterface_MoveKinematic(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* targetPosition, JPH_Quat* targetRotation, float deltaTime)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->MoveKinematic(JPH::BodyID(bodyId), ToRVec3(targetPosition), ToQuat(targetRotation), deltaTime);
}

void JPH_BodyInterface_SetLinearAndAngularVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* linearVelocity, JPH_Vec3* angularVelocity)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->SetLinearAndAngularVelocity(JPH::BodyID(bodyId), ToVec3(linearVelocity), ToVec3(angularVelocity));
}

void JPH_BodyInterface_GetLinearAndAngularVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* linearVelocity, JPH_Vec3* angularVelocity)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    JPH::Vec3 linear, angular;
    joltBodyInterface->GetLinearAndAngularVelocity(JPH::BodyID(bodyId), linear, angular);
    FromVec3(linear, linearVelocity);
    FromVec3(angular, angularVelocity);
}

void JPH_BodyInterface_AddLinearVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* linearVelocity)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->AddLinearVelocity(JPH::BodyID(bodyId), ToVec3(linearVelocity));
}

void JPH_BodyInterface_AddLinearAndAngularVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* linearVelocity, JPH_Vec3* angularVelocity)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->AddLinearAndAngularVelocity(JPH::BodyID(bodyId), ToVec3(linearVelocity), ToVec3(angularVelocity));
}

void JPH_BodyInterface_SetAngularVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* angularVelocity)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->SetAngularVelocity(JPH::BodyID(bodyId), ToVec3(angularVelocity));
}

void JPH_BodyInterface_GetAngularVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* angularVelocity)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    auto result = joltBodyInterface->GetAngularVelocity(JPH::BodyID(bodyId));
    FromVec3(result, angularVelocity);
}

void JPH_BodyInterface_GetPointVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* point, JPH_Vec3* velocity)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    auto result = joltBodyInterface->GetPointVelocity(JPH::BodyID(bodyId), ToRVec3(point));
    FromVec3(result, velocity);
}

void JPH_BodyInterface_AddForce(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* force)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->AddForce(JPH::BodyID(bodyId), ToVec3(force));
}

void JPH_BodyInterface_AddForce2(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* force, JPH_RVec3* point)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->AddForce(JPH::BodyID(bodyId), ToVec3(force), ToRVec3(point));
}

void JPH_BodyInterface_AddTorque(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* torque)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->AddTorque(JPH::BodyID(bodyId), ToVec3(torque));
}

void JPH_BodyInterface_AddForceAndTorque(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* force, JPH_Vec3* torque)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->AddForceAndTorque(JPH::BodyID(bodyId), ToVec3(force), ToVec3(torque));
}

void JPH_BodyInterface_AddImpulse(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* impulse)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->AddImpulse(JPH::BodyID(bodyId), ToVec3(impulse));
}

void JPH_BodyInterface_AddImpulse2(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* impulse, JPH_RVec3* point)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->AddImpulse(JPH::BodyID(bodyId), ToVec3(impulse), ToRVec3(point));
}

void JPH_BodyInterface_AddAngularImpulse(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* angularImpulse)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->AddAngularImpulse(JPH::BodyID(bodyId), ToVec3(angularImpulse));
}

void JPH_BodyInterface_GetInverseInertia(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Matrix4x4* result)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    JPH::RMat44 mat = static_cast<JPH::RMat44>(joltBodyInterface->GetInverseInertia(JPH::BodyID(bodyId)));
    FromJolt(mat, result);
}

void JPH_BodyInterface_SetGravityFactor(JPH_BodyInterface* interface, JPH_BodyID bodyId, float gravityFactor)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->SetGravityFactor(JPH::BodyID(bodyId), gravityFactor);
}

float JPH_BodyInterface_GetGravityFactor(JPH_BodyInterface* interface, JPH_BodyID bodyId)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    return joltBodyInterface->GetGravityFactor(JPH::BodyID(bodyId));
}

void JPH_BodyInterface_InvalidateContactCache(JPH_BodyInterface* interface, JPH_BodyID bodyId)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->InvalidateContactCache(JPH::BodyID(bodyId));
}

//--------------------------------------------------------------------------------------------------
// JPH_BodyLockInterface
//--------------------------------------------------------------------------------------------------
void JPH_BodyLockInterface_LockRead(const JPH_BodyLockInterface* lockInterface, JPH_BodyID bodyID, JPH_BodyLockRead* outLock)
{
    JPH_ASSERT(outLock != nullptr);
    auto joltBodyLockInterface = reinterpret_cast<const JPH::BodyLockInterface*>(lockInterface);

    ::new (outLock) JPH::BodyLockRead(*joltBodyLockInterface, JPH::BodyID(bodyID));
}

void JPH_BodyLockInterface_UnlockRead(const JPH_BodyLockInterface* lockInterface, JPH_BodyLockRead* ioLock)
{
    JPH_UNUSED(lockInterface);
    JPH_ASSERT(ioLock != nullptr);
    JPH_ASSERT(lockInterface != nullptr && lockInterface == ioLock->lockInterface);

    reinterpret_cast<const JPH::BodyLockRead*>(ioLock)->~BodyLockRead();
}

void JPH_BodyLockInterface_LockWrite(const JPH_BodyLockInterface* lockInterface, JPH_BodyID bodyID, JPH_BodyLockWrite* outLock)
{
    JPH_ASSERT(outLock != nullptr);
    auto joltBodyLockInterface = reinterpret_cast<const JPH::BodyLockInterface*>(lockInterface);

    ::new (outLock) JPH::BodyLockRead(*joltBodyLockInterface, JPH::BodyID(bodyID));
}

void JPH_BodyLockInterface_UnlockWrite(const JPH_BodyLockInterface* lockInterface, JPH_BodyLockWrite* ioLock)
{
    JPH_UNUSED(lockInterface);
    JPH_ASSERT(ioLock != nullptr);
    JPH_ASSERT(lockInterface != nullptr && lockInterface == ioLock->lockInterface);

    reinterpret_cast<const JPH::BodyLockWrite*>(ioLock)->~BodyLockWrite();
}

//--------------------------------------------------------------------------------------------------
// JPH_NarrowPhaseQuery
//--------------------------------------------------------------------------------------------------
bool JPH_NarrowPhaseQuery_CastRay(const JPH_NarrowPhaseQuery* query,
    const JPH_RVec3* origin, const JPH_Vec3* direction,
    JPH_RayCastResult* hit, 
    const void* broadPhaseLayerFilter, // Can be NULL (no filter)
    const void* objectLayerFilter, // Can be NULL (no filter)
    const void* bodyFilter)
{
    JPH_ASSERT(query && origin && direction && hit);
    auto joltQuery = reinterpret_cast<const JPH::NarrowPhaseQuery*>(query);
    JPH::RRayCast ray(ToRVec3(origin), ToVec3(direction));
    const JPH::BroadPhaseLayerFilter broad_phase_layer_filter{};
    const JPH::ObjectLayerFilter object_layer_filter{};
    const JPH::BodyFilter body_filter{};

    return joltQuery->CastRay(
        ray,
        *reinterpret_cast<JPH::RayCastResult*>(hit),
        broadPhaseLayerFilter ? *static_cast<const JPH::BroadPhaseLayerFilter*>(broadPhaseLayerFilter) : broad_phase_layer_filter,
        objectLayerFilter ? *static_cast<const JPH::ObjectLayerFilter*>(objectLayerFilter) : object_layer_filter,
        bodyFilter ? *static_cast<const JPH::BodyFilter*>(bodyFilter) : body_filter
    );
}

/* Body */
JPH_BodyID JPH_Body_GetID(const JPH_Body* body)
{
    auto joltBody = reinterpret_cast<const JPH::Body*>(body);
    return joltBody->GetID().GetIndexAndSequenceNumber();
}

JPH_Bool32 JPH_Body_IsActive(const JPH_Body* body)
{
    return reinterpret_cast<const JPH::Body*>(body)->IsActive();
}

JPH_Bool32 JPH_Body_IsStatic(const JPH_Body* body)
{
    return reinterpret_cast<const JPH::Body*>(body)->IsStatic();
}

JPH_Bool32 JPH_Body_IsKinematic(JPH_Body* body)
{
    return reinterpret_cast<const JPH::Body*>(body)->IsKinematic();
}

JPH_Bool32 JPH_Body_IsDynamic(const JPH_Body* body)
{
    return reinterpret_cast<const JPH::Body*>(body)->IsDynamic();
}

JPH_Bool32 JPH_Body_IsSensor(const JPH_Body* body)
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

void JPH_Body_GetPosition(const JPH_Body* body, JPH_RVec3* result)
{
    auto joltVector = reinterpret_cast<const JPH::Body*>(body)->GetPosition();
    FromRVec3(joltVector, result);
}

void JPH_Body_GetRotation(const JPH_Body* body, JPH_Quat* result)
{
    auto joltQuat = reinterpret_cast<const JPH::Body*>(body)->GetRotation();
    FromJolt(joltQuat, result);
}

void JPH_Body_GetCenterOfMassPosition(const JPH_Body* body, JPH_RVec3* result)
{
    auto joltVector = reinterpret_cast<const JPH::Body*>(body)->GetCenterOfMassPosition();
    FromRVec3(joltVector, result);
}

void JPH_Body_GetWorldTransform(const JPH_Body* body, JPH_Matrix4x4* result)
{
    auto joltMatrix = reinterpret_cast<const JPH::Body*>(body)->GetWorldTransform();
    FromJolt(joltMatrix, result);
}

void JPH_Body_GetCenterOfMassTransform(const JPH_Body* body, JPH_Matrix4x4* result)
{
    auto joltMatrix = reinterpret_cast<const JPH::Body*>(body)->GetCenterOfMassTransform();
    FromJolt(joltMatrix, result);
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

#ifdef _MSC_VER
#   pragma warning(pop)
#endif
