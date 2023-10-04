// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

#include "joltc.h"

#ifdef _MSC_VER
__pragma(warning(push, 0))
#endif

#include <Jolt/Jolt.h>
#include <Jolt/RegisterTypes.h>
#include <Jolt/Core/Factory.h>
#include <Jolt/Core/TempAllocator.h>
#include <Jolt/Core/JobSystemThreadPool.h>
#include <Jolt/Core/JobSystemSingleThreaded.h>
#include <Jolt/Physics/PhysicsSettings.h>
#include <Jolt/Physics/PhysicsSystem.h>
#include <Jolt/Physics/Collision/CastResult.h>
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
#include <Jolt/Physics/SoftBody/SoftBodyCreationSettings.h>
#include <Jolt/Physics/Collision/RayCast.h>
#include <Jolt/Physics/Collision/NarrowPhaseQuery.h>
#include <Jolt/Physics/Constraints/SpringSettings.h>
#include <Jolt/Physics/Constraints/PointConstraint.h>
#include <Jolt/Physics/Constraints/DistanceConstraint.h>
#include <Jolt/Physics/Constraints/HingeConstraint.h>
#include <Jolt/Physics/Constraints/SliderConstraint.h>

#ifdef _MSC_VER
__pragma(warning(pop))
#endif

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
static JPH_AssertFailureFunc s_AssertFailureFunc = nullptr;

// Callback for asserts, connect this to your own assert handler if you have one
static bool AssertFailedImpl(const char* inExpression, const char* inMessage, const char* inFile, uint inLine)
{
    if (s_AssertFailureFunc) {
        return !!s_AssertFailureFunc(inExpression, inMessage, inFile, inLine);
    }

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
#ifdef JPH_DOUBLE_PRECISION
    return JPH::RVec3(vec->x, vec->y, vec->z);
#else
    return JPH::RVec3(static_cast<float>(vec->x), static_cast<float>(vec->y), static_cast<float>(vec->z));
#endif
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

static void FromJolt(const JPH::Mat44& matrix, JPH_Matrix4x4* result)
{
    JPH::Vec4 column0 = matrix.GetColumn4(0);
    JPH::Vec4 column1 = matrix.GetColumn4(1);
    JPH::Vec4 column2 = matrix.GetColumn4(2);
    JPH::Vec3 translation = matrix.GetTranslation();

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

    result->m41 = translation.GetX();
    result->m42 = translation.GetY();
    result->m43 = translation.GetZ();
    result->m44 = 1.0f;
}


static JPH::Mat44 ToJolt(const JPH_Matrix4x4& matrix)
{
	JPH::Mat44 result{};
	result.SetColumn4(0, JPH::Vec4(matrix.m11, matrix.m12, matrix.m13, matrix.m14));
	result.SetColumn4(1, JPH::Vec4(matrix.m21, matrix.m22, matrix.m23, matrix.m24));
	result.SetColumn4(2, JPH::Vec4(matrix.m31, matrix.m32, matrix.m33, matrix.m34));
	result.SetColumn4(3, JPH::Vec4(matrix.m41, matrix.m42, matrix.m43, matrix.m44));
    return result;
}

#ifdef JPH_DOUBLE_PRECISION
static void FromJolt(const JPH::DMat44& matrix, JPH_RMatrix4x4* result)
{
    JPH::Vec4 column0 = matrix.GetColumn4(0);
    JPH::Vec4 column1 = matrix.GetColumn4(1);
    JPH::Vec4 column2 = matrix.GetColumn4(2);
    JPH::DVec3 translation = matrix.GetTranslation();

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

    result->m41 = translation.GetX();
    result->m42 = translation.GetY();
    result->m43 = translation.GetZ();
    result->m44 = 1.0f;
}
#else

static void FromJolt(const JPH::Mat44& matrix, JPH_RMatrix4x4* result)
{
    JPH::Vec4 column0 = matrix.GetColumn4(0);
    JPH::Vec4 column1 = matrix.GetColumn4(1);
    JPH::Vec4 column2 = matrix.GetColumn4(2);
    JPH::Vec3 translation = matrix.GetTranslation();

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

    result->m41 = translation.GetX();
    result->m42 = translation.GetY();
    result->m43 = translation.GetZ();
    result->m44 = 1.0;
}

#endif


static void FromJolt(const JPH::MassProperties& jolt, JPH_MassProperties* result)
{
	result->mass = jolt.mMass;
	FromJolt(jolt.mInertia, &result->inertia);
}

static JPH::MassProperties ToJolt(const JPH_MassProperties* properties)
{
	JPH::MassProperties result{};
	result.mMass = properties->mass;
	result.mInertia = ToJolt(properties->inertia);
	return result;
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

void JPH_SetAssertFailureHandler(JPH_AssertFailureFunc handler)
{
#ifdef JPH_ENABLE_ASSERTS
    s_AssertFailureFunc = handler;
#else
    JPH_UNUSED(handler);
#endif
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

JPH_JobSystem* JPH_JobSystemThreadPool_Create(uint32_t maxJobs, uint32_t maxBarriers, int inNumThreads)
{
    auto job_system = new JPH::JobSystemThreadPool(maxJobs, maxBarriers, inNumThreads);
    return reinterpret_cast<JPH_JobSystem*>(job_system);
}

JPH_JobSystem* JPH_JobSystemSingleThreaded_Create(uint32_t maxJobs)
{
    auto job_system = new JPH::JobSystemSingleThreaded(maxJobs);
    return reinterpret_cast<JPH_JobSystem*>(job_system);
}

void JPH_JobSystem_Destroy(JPH_JobSystem* system)
{
    if (system)
    {
        delete reinterpret_cast<JPH::JobSystem*>(system);
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

/* JPH_BroadPhaseLayerFilter */
static JPH_BroadPhaseLayerFilter_Procs g_BroadPhaseLayerFilter_Procs;

class ManagedBroadPhaseLayerFilter final : public JPH::BroadPhaseLayerFilter
{
public:
    ManagedBroadPhaseLayerFilter() = default;

    ManagedBroadPhaseLayerFilter(const ManagedBroadPhaseLayerFilter&) = delete;
    ManagedBroadPhaseLayerFilter(const ManagedBroadPhaseLayerFilter&&) = delete;
    ManagedBroadPhaseLayerFilter& operator=(const ManagedBroadPhaseLayerFilter&) = delete;
    ManagedBroadPhaseLayerFilter& operator=(const ManagedBroadPhaseLayerFilter&&) = delete;

    bool ShouldCollide(BroadPhaseLayer inLayer) const override
    {
        return g_BroadPhaseLayerFilter_Procs.ShouldCollide(
            reinterpret_cast<const JPH_BroadPhaseLayerFilter*>(this),
            static_cast<JPH_BroadPhaseLayer>(inLayer)
        ) == 1;
    }
};

void JPH_BroadPhaseLayerFilter_SetProcs(JPH_BroadPhaseLayerFilter_Procs procs)
{
    g_BroadPhaseLayerFilter_Procs = procs;
}

JPH_BroadPhaseLayerFilter* JPH_BroadPhaseLayerFilter_Create()
{
    auto filter = new ManagedBroadPhaseLayerFilter();
    return reinterpret_cast<JPH_BroadPhaseLayerFilter*>(filter);
}

void JPH_BroadPhaseLayerFilter_Destroy(JPH_BroadPhaseLayerFilter* filter)
{
    if (filter)
    {
        delete reinterpret_cast<ManagedBroadPhaseLayerFilter*>(filter);
    }
}

/* JPH_ObjectLayerFilter */
static JPH_ObjectLayerFilter_Procs g_ObjectLayerFilter_Procs;

class ManagedObjectLayerFilter final : public JPH::ObjectLayerFilter
{
public:
    ManagedObjectLayerFilter() = default;

    ManagedObjectLayerFilter(const ManagedObjectLayerFilter&) = delete;
    ManagedObjectLayerFilter(const ManagedObjectLayerFilter&&) = delete;
    ManagedObjectLayerFilter& operator=(const ManagedObjectLayerFilter&) = delete;
    ManagedObjectLayerFilter& operator=(const ManagedObjectLayerFilter&&) = delete;

    bool ShouldCollide(ObjectLayer inLayer) const override
    {
        return g_ObjectLayerFilter_Procs.ShouldCollide(
            reinterpret_cast<const JPH_ObjectLayerFilter*>(this),
            static_cast<JPH_ObjectLayer>(inLayer)
        ) == 1;
    }
};

void JPH_ObjectLayerFilter_SetProcs(JPH_ObjectLayerFilter_Procs procs)
{
    g_ObjectLayerFilter_Procs = procs;
}

JPH_ObjectLayerFilter* JPH_ObjectLayerFilter_Create()
{
    auto filter = new ManagedObjectLayerFilter();
    return reinterpret_cast<JPH_ObjectLayerFilter*>(filter);
}

void JPH_ObjectLayerFilter_Destroy(JPH_ObjectLayerFilter* filter)
{
    if (filter)
    {
        delete reinterpret_cast<ManagedObjectLayerFilter*>(filter);
    }
}

/* JPH_BodyFilter */
static JPH_BodyFilter_Procs g_BodyFilter_Procs;

class ManagedBodyFilter final : public JPH::BodyFilter
{
public:
    ManagedBodyFilter() = default;

    ManagedBodyFilter(const ManagedBodyFilter&) = delete;
    ManagedBodyFilter(const ManagedBodyFilter&&) = delete;
    ManagedBodyFilter& operator=(const ManagedBodyFilter&) = delete;
    ManagedBodyFilter& operator=(const ManagedBodyFilter&&) = delete;

    bool ShouldCollide(const BodyID &bodyID) const override
    {
        return !!g_BodyFilter_Procs.ShouldCollide(
            reinterpret_cast<const JPH_BodyFilter*>(this),
            (JPH_BodyID)bodyID.GetIndexAndSequenceNumber());
    }

    bool ShouldCollideLocked(const Body& body) const override
    {
        return !!g_BodyFilter_Procs.ShouldCollideLocked(
            reinterpret_cast<const JPH_BodyFilter*>(this),
            reinterpret_cast<const JPH_Body *>(&body));
    }
};

void JPH_BodyFilter_SetProcs(JPH_BodyFilter_Procs procs)
{
    g_BodyFilter_Procs = procs;
}

JPH_BodyFilter* JPH_BodyFilter_Create()
{
    auto filter = new ManagedBodyFilter();
    return reinterpret_cast<JPH_BodyFilter*>(filter);
}

void JPH_BodyFilter_Destroy(JPH_BodyFilter* filter)
{
    if (filter)
    {
        delete reinterpret_cast<ManagedBodyFilter*>(filter);
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

/* ConvexShape */
float JPH_ConvexShape_GetDensity(const JPH_ConvexShape* shape)
{
    return reinterpret_cast<const JPH::ConvexShape*>(shape)->GetDensity();
}

void JPH_ConvexShape_SetDensity(JPH_ConvexShape* shape, float density)
{
    reinterpret_cast<JPH::ConvexShape*>(shape)->SetDensity(density);
}

/* BoxShape */
JPH_BoxShapeSettings* JPH_BoxShapeSettings_Create(const JPH_Vec3* halfExtent, float convexRadius)
{
    auto settings = new JPH::BoxShapeSettings(ToVec3(halfExtent), convexRadius);
    return reinterpret_cast<JPH_BoxShapeSettings*>(settings);
}

JPH_BoxShape* JPH_BoxShapeSettings_CreateShape(const JPH_BoxShapeSettings* settings)
{
	const JPH::BoxShapeSettings* jolt_settings = reinterpret_cast<const JPH::BoxShapeSettings*>(settings);
    auto shape_res = jolt_settings->Create();
    return reinterpret_cast<JPH_BoxShape*>(shape_res.Get().GetPtr());
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

JPH_SphereShape* JPH_SphereShapeSettings_CreateShape(const JPH_SphereShapeSettings* settings)
{
	const JPH::SphereShapeSettings* jolt_settings = reinterpret_cast<const JPH::SphereShapeSettings*>(settings);
    auto shape_res = jolt_settings->Create();
    return reinterpret_cast<JPH_SphereShape*>(shape_res.Get().GetPtr());
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

JPH_CapsuleShape* JPH_CapsuleShape_Create(float halfHeightOfCylinder, float radius)
{
    auto shape = new JPH::CapsuleShape(halfHeightOfCylinder, radius, 0);
    return reinterpret_cast<JPH_CapsuleShape*>(shape);
}

float JPH_CapsuleShape_GetRadius(const JPH_CapsuleShape* shape)
{
    JPH_ASSERT(shape);
    return reinterpret_cast<const JPH::CapsuleShape*>(shape)->GetRadius();
}

float JPH_CapsuleShape_GetHalfHeightOfCylinder(const JPH_CapsuleShape* shape)
{
    JPH_ASSERT(shape);
    return reinterpret_cast<const JPH::CapsuleShape*>(shape)->GetHalfHeightOfCylinder();
}

/* CylinderShapeSettings */
JPH_CylinderShapeSettings* JPH_CylinderShapeSettings_Create(float halfHeight, float radius, float convexRadius)
{
    auto settings = new JPH::CylinderShapeSettings(halfHeight, radius, convexRadius);
    return reinterpret_cast<JPH_CylinderShapeSettings*>(settings);
}

JPH_CylinderShape* JPH_CylinderShape_Create(float halfHeight, float radius)
{
    auto shape = new JPH::CylinderShape(halfHeight, radius, 0.f, 0);
    return reinterpret_cast<JPH_CylinderShape*>(shape);
}

float JPH_CylinderShape_GetRadius(const JPH_CylinderShape* shape)
{
    JPH_ASSERT(shape);
    return reinterpret_cast<const JPH::CylinderShape*>(shape)->GetRadius();
}

float JPH_CylinderShape_GetHalfHeight(const JPH_CylinderShape* shape)
{
    JPH_ASSERT(shape);
    return reinterpret_cast<const JPH::CylinderShape*>(shape)->GetHalfHeight();
}

/* ConvexHullShape */
float JPH_ConvexShapeSettings_GetDensity(const JPH_ConvexShapeSettings* shape)
{
	return reinterpret_cast<const JPH::ConvexShapeSettings*>(shape)->mDensity;
}

void JPH_ConvexShapeSettings_SetDensity(JPH_ConvexShapeSettings* shape, float value)
{
    reinterpret_cast<JPH::ConvexShapeSettings*>(shape)->SetDensity(value);
}

JPH_ConvexHullShapeSettings* JPH_ConvexHullShapeSettings_Create(const JPH_Vec3* points, uint32_t pointsCount, float maxConvexRadius)
{
    Array<Vec3> joltPoints;
    joltPoints.reserve(pointsCount);

    for (uint32_t i = 0; i < pointsCount; i++)
    {
        joltPoints.push_back(ToVec3(points[i]));
    }

    auto settings = new JPH::ConvexHullShapeSettings(joltPoints, maxConvexRadius);
    return reinterpret_cast<JPH_ConvexHullShapeSettings*>(settings);
}

JPH_ConvexHullShape* JPH_ConvexHullShapeSettings_CreateShape(const JPH_ConvexHullShapeSettings* settings)
{
	const JPH::ConvexHullShapeSettings* jolt_settings = reinterpret_cast<const JPH::ConvexHullShapeSettings*>(settings);
    auto shape_res = jolt_settings->Create();
    return reinterpret_cast<JPH_ConvexHullShape*>(shape_res.Get().GetPtr());
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

    for (uint32_t i = 0; i < verticesCount; ++i)
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

JPH_MeshShape* JPH_MeshShapeSettings_CreateShape(const JPH_MeshShapeSettings* settings)
{
    const JPH::MeshShapeSettings* jolt_settings = reinterpret_cast<const JPH::MeshShapeSettings*>(settings);
    auto shape_res = jolt_settings->Create();
    return reinterpret_cast<JPH_MeshShape*>(shape_res.Get().GetPtr());
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

/* MutableCompoundShape */
JPH_CAPI JPH_MutableCompoundShapeSettings* JPH_MutableCompoundShapeSettings_Create()
{
    auto settings = new JPH::MutableCompoundShapeSettings();
    return reinterpret_cast<JPH_MutableCompoundShapeSettings*>(settings);
}

JPH_MutableCompoundShape* JPH_MutableCompoundShape_Create(const JPH_MutableCompoundShapeSettings* settings)
{
	const JPH::MutableCompoundShapeSettings* jolt_settings = reinterpret_cast<const JPH::MutableCompoundShapeSettings*>(settings);
    auto shape_res = jolt_settings->Create();
    return reinterpret_cast<JPH_MutableCompoundShape*>(shape_res.Get().GetPtr());
}


/* Shape */
void JPH_Shape_Destroy(JPH_Shape* shape)
{
    if (shape)
    {
        delete reinterpret_cast<JPH::Shape*>(shape);
    }
}

void JPH_Shape_GetLocalBounds(JPH_Shape* shape, JPH_AABox* result)
{
	JPH_ASSERT(shape);
	JPH_ASSERT(result);

    auto bounds = reinterpret_cast<JPH::Shape*>(shape)->GetLocalBounds();
	FromVec3(bounds.mMin, &result->min);
	FromVec3(bounds.mMax, &result->max);
}

void JPH_Shape_GetMassProperties(const JPH_Shape* shape, JPH_MassProperties* result)
{
    auto joltShape = reinterpret_cast<const JPH::Shape*>(shape);
	FromJolt(joltShape->GetMassProperties(), result);
}

void JPH_Shape_GetCenterOfMass(JPH_Shape* shape, JPH_Vec3* result)
{
	auto joltShape = reinterpret_cast<const JPH::Shape*>(shape);
	auto joltVector = joltShape->GetCenterOfMass();
	FromVec3(joltVector, result);
}

float JPH_Shape_GetInnerRadius(JPH_Shape* shape)
{
	auto joltShape = reinterpret_cast<const JPH::Shape*>(shape);
	return joltShape->GetInnerRadius();
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

void JPH_BodyCreationSettings_GetLinearVelocity(JPH_BodyCreationSettings* settings, JPH_Vec3* velocity)
{
    JPH_ASSERT(settings);

    auto joltVector = reinterpret_cast<JPH::BodyCreationSettings*>(settings)->mLinearVelocity;
    FromVec3(joltVector, velocity);
}

void JPH_BodyCreationSettings_SetLinearVelocity(JPH_BodyCreationSettings* settings, const JPH_Vec3* velocity)
{
    JPH_ASSERT(settings);

    reinterpret_cast<JPH::BodyCreationSettings*>(settings)->mLinearVelocity = ToVec3(velocity);
}

void JPH_BodyCreationSettings_GetAngularVelocity(JPH_BodyCreationSettings* settings, JPH_Vec3* velocity)
{
    JPH_ASSERT(settings);

    auto joltVector = reinterpret_cast<JPH::BodyCreationSettings*>(settings)->mAngularVelocity;
    FromVec3(joltVector, velocity);
}

void JPH_BodyCreationSettings_SetAngularVelocity(JPH_BodyCreationSettings* settings, const JPH_Vec3* velocity)
{
    JPH_ASSERT(settings);

    reinterpret_cast<JPH::BodyCreationSettings*>(settings)->mAngularVelocity = ToVec3(velocity);
}

JPH_MotionType JPH_BodyCreationSettings_GetMotionType(JPH_BodyCreationSettings* settings)
{
    JPH_ASSERT(settings);

    return static_cast<JPH_MotionType>(reinterpret_cast<JPH::BodyCreationSettings*>(settings)->mMotionType);
}

void JPH_BodyCreationSettings_SetMotionType(JPH_BodyCreationSettings* settings, JPH_MotionType value)
{
    JPH_ASSERT(settings);

    reinterpret_cast<JPH::BodyCreationSettings*>(settings)->mMotionType = (JPH::EMotionType)value;
}

JPH_AllowedDOFs JPH_BodyCreationSettings_GetAllowedDOFs(JPH_BodyCreationSettings* settings)
{
    JPH_ASSERT(settings);

    return static_cast<JPH_AllowedDOFs>(reinterpret_cast<JPH::BodyCreationSettings*>(settings)->mAllowedDOFs);
}

void JPH_BodyCreationSettings_SetAllowedDOFs(JPH_BodyCreationSettings* settings, JPH_AllowedDOFs value)
{
    reinterpret_cast<JPH::BodyCreationSettings*>(settings)->mAllowedDOFs = (JPH::EAllowedDOFs)value;
}

/* JPH_SoftBodyCreationSettings */
JPH_SoftBodyCreationSettings* JPH_SoftBodyCreationSettings_Create()
{
    auto bodyCreationSettings = new JPH::SoftBodyCreationSettings();
    return reinterpret_cast<JPH_SoftBodyCreationSettings*>(bodyCreationSettings);
}

//void JPH_SoftBodyCreationSettings_Destroy(JPH_SoftBodyCreationSettings* settings)
//{
//    if (settings)
//    {
//		// TODO: MSVC issue -> `scalar deleting destructor'(unsigned int) __ptr64': function not inlined
//		auto bodyCreationSettings = reinterpret_cast<JPH::SoftBodyCreationSettings*>(settings);
//		delete bodyCreationSettings;
//    }
//}

/* JPH_ConstraintSettings */
void JPH_ConstraintSettings_Destroy(JPH_ConstraintSettings* settings)
{
    if (settings)
    {
        delete reinterpret_cast<JPH::ConstraintSettings*>(settings);
    }
}

/* JPH_Constraint */
JPH_ConstraintSettings* JPH_Constraint_GetConstraintSettings(JPH_Constraint* constraint)
{
    auto joltConstraint = reinterpret_cast<JPH::HingeConstraint*>(constraint);
    auto settings = joltConstraint->GetConstraintSettings().GetPtr();
    return reinterpret_cast<JPH_ConstraintSettings*>(settings);
}

JPH_Bool32 JPH_Constraint_GetEnabled(JPH_Constraint* constraint)
{
    auto joltConstraint = reinterpret_cast<JPH::HingeConstraint*>(constraint);
    return joltConstraint->GetEnabled();
}

void JPH_Constraint_SetEnabled(JPH_Constraint* constraint, JPH_Bool32 enabled)
{
    auto joltConstraint = reinterpret_cast<JPH::HingeConstraint*>(constraint);
    joltConstraint->SetEnabled(!!enabled);
}

void JPH_Constraint_Destroy(JPH_Constraint* constraint)
{
    if (constraint)
    {
        delete reinterpret_cast<JPH::Constraint*>(constraint);
    }
}

JPH_SpringSettings* JPH_SpringSettings_Create(float frequency, float damping)
{
    auto settings = new JPH::SpringSettings(ESpringMode::FrequencyAndDamping, frequency, damping);
    return reinterpret_cast<JPH_SpringSettings*>(settings);
}

float JPH_SpringSettings_GetFrequency(JPH_SpringSettings* settings)
{
    return reinterpret_cast<JPH::SpringSettings*>(settings)->mFrequency;
}

/* JPH_TwoBodyConstraintSettings */

/* JPH_DistanceConstraintSettings */
JPH_DistanceConstraintSettings* JPH_DistanceConstraintSettings_Create(void)
{
    auto settings = new JPH::DistanceConstraintSettings();
    return reinterpret_cast<JPH_DistanceConstraintSettings*>(settings);
}

void JPH_DistanceConstraintSettings_GetPoint1(JPH_DistanceConstraintSettings* settings, JPH_RVec3* result)
{
    JPH_ASSERT(settings);
	JPH_ASSERT(result);
    auto joltSettings = reinterpret_cast<JPH::PointConstraintSettings*>(settings);

    auto joltVector = joltSettings->mPoint1;
    FromRVec3(joltVector, result);
}

void JPH_DistanceConstraintSettings_SetPoint1(JPH_DistanceConstraintSettings* settings, const JPH_RVec3* value)
{
    JPH_ASSERT(settings);
	JPH_ASSERT(value);
    auto joltSettings = reinterpret_cast<JPH::PointConstraintSettings*>(settings);

    joltSettings->mPoint1 = ToRVec3(value);
}

void JPH_DistanceConstraintSettings_GetPoint2(JPH_DistanceConstraintSettings* settings, JPH_RVec3* result)
{
    JPH_ASSERT(settings);
	JPH_ASSERT(result);
    auto joltSettings = reinterpret_cast<JPH::PointConstraintSettings*>(settings);

    auto joltVector = joltSettings->mPoint2;
    FromRVec3(joltVector, result);
}

void JPH_DistanceConstraintSettings_SetPoint2(JPH_DistanceConstraintSettings* settings, const JPH_RVec3* value)
{
    JPH_ASSERT(settings);
	JPH_ASSERT(value);
    auto joltSettings = reinterpret_cast<JPH::PointConstraintSettings*>(settings);

    joltSettings->mPoint2 = ToRVec3(value);
}

JPH_DistanceConstraint* JPH_DistanceConstraintSettings_CreateConstraint(JPH_DistanceConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2)
{
    auto joltBody1 = reinterpret_cast<JPH::Body*>(body1);
    auto joltBody2 = reinterpret_cast<JPH::Body*>(body2);
    JPH::TwoBodyConstraint* constraint = reinterpret_cast<JPH::DistanceConstraintSettings*>(settings)->Create(*joltBody1, *joltBody2);
    return reinterpret_cast<JPH_DistanceConstraint*>(static_cast<JPH::DistanceConstraint*>(constraint));
}

/* JPH_HingeConstraintSettings */

JPH_HingeConstraintSettings* JPH_HingeConstraintSettings_Create(void)
{
    auto settings = new JPH::HingeConstraintSettings();
    return reinterpret_cast<JPH_HingeConstraintSettings*>(settings);
}

void JPH_HingeConstraintSettings_GetPoint1(JPH_HingeConstraintSettings* settings, JPH_RVec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);

    auto joltVector = joltSettings->mPoint1;
    FromRVec3(joltVector, result);
}

void JPH_HingeConstraintSettings_SetPoint1(JPH_HingeConstraintSettings* settings, const JPH_RVec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);

    joltSettings->mPoint1 = ToRVec3(value);
}

void JPH_HingeConstraintSettings_GetPoint2(JPH_HingeConstraintSettings* settings, JPH_RVec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);
    auto joltVector = joltSettings->mPoint2;
    FromRVec3(joltVector, result);
}

void JPH_HingeConstraintSettings_SetPoint2(JPH_HingeConstraintSettings* settings, const JPH_RVec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);
    joltSettings->mPoint2 = ToRVec3(value);
}

void JPH_HingeConstraintSettings_SetHingeAxis1(JPH_HingeConstraintSettings* settings, const JPH_Vec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);
    joltSettings->mHingeAxis1 = ToVec3(value);
}

void JPH_HingeConstraintSettings_GetHingeAxis1(JPH_HingeConstraintSettings* settings, JPH_Vec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);
    FromVec3(joltSettings->mHingeAxis1, result);
}

void JPH_HingeConstraintSettings_SetNormalAxis1(JPH_HingeConstraintSettings* settings, const JPH_Vec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);
    joltSettings->mNormalAxis1 = ToVec3(value);
}

void JPH_HingeConstraintSettings_GetNormalAxis1(JPH_HingeConstraintSettings* settings, JPH_Vec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);
    FromVec3(joltSettings->mNormalAxis1, result);
}

void JPH_HingeConstraintSettings_SetHingeAxis2(JPH_HingeConstraintSettings* settings, const JPH_Vec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);
    joltSettings->mHingeAxis2 = ToVec3(value);
}

void JPH_HingeConstraintSettings_GetHingeAxis2(JPH_HingeConstraintSettings* settings, JPH_Vec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);
    FromVec3(joltSettings->mHingeAxis2, result);
}

void JPH_HingeConstraintSettings_SetNormalAxis2(JPH_HingeConstraintSettings* settings, const JPH_Vec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);
    joltSettings->mNormalAxis2 = ToVec3(value);
}

void JPH_HingeConstraintSettings_GetNormalAxis2(JPH_HingeConstraintSettings* settings, JPH_Vec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);
    FromVec3(joltSettings->mNormalAxis2, result);
}

JPH_HingeConstraint* JPH_HingeConstraintSettings_CreateConstraint(JPH_HingeConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2)
{
    auto joltBody1 = reinterpret_cast<JPH::Body*>(body1);
    auto joltBody2 = reinterpret_cast<JPH::Body*>(body2);
    JPH::TwoBodyConstraint* constraint = reinterpret_cast<JPH::HingeConstraintSettings*>(settings)->Create(*joltBody1, *joltBody2);
    return reinterpret_cast<JPH_HingeConstraint*>(static_cast<JPH::HingeConstraint*>(constraint));
}

JPH_HingeConstraintSettings * JPH_HingeConstraint_GetSettings(JPH_HingeConstraint* constraint)
{
    auto joltConstraint = reinterpret_cast<JPH::HingeConstraint*>(constraint);
    auto joltSettings = joltConstraint->GetConstraintSettings().GetPtr();
    return reinterpret_cast<JPH_HingeConstraintSettings*>(joltSettings);
}

float JPH_HingeConstraint_GetCurrentAngle(JPH_HingeConstraint* constraint)
{
    return reinterpret_cast<JPH::HingeConstraint*>(constraint)->GetCurrentAngle();
}

void JPH_HingeConstraint_SetLimits(JPH_HingeConstraint* constraint, float inLimitsMin, float inLimitsMax)
{
    return reinterpret_cast<JPH::HingeConstraint*>(constraint)->SetLimits(inLimitsMin, inLimitsMax);
}

float JPH_HingeConstraint_GetLimitsMin(JPH_HingeConstraint* constraint)
{
    return reinterpret_cast<JPH::HingeConstraint*>(constraint)->GetLimitsMin();
}

float JPH_HingeConstraint_GetLimitsMax(JPH_HingeConstraint* constraint)
{
    return reinterpret_cast<JPH::HingeConstraint*>(constraint)->GetLimitsMax();
}

JPH_Bool32 JPH_HingeConstraint_HasLimits(JPH_HingeConstraint* constraint)
{
    return reinterpret_cast<JPH::HingeConstraint*>(constraint)->HasLimits();
}

JPH_SpringSettings* JPH_HingeConstraint_GetLimitsSpringSettings(JPH_HingeConstraint* constraint)
{
    return reinterpret_cast<JPH_SpringSettings*>(&reinterpret_cast<JPH::HingeConstraint*>(constraint)->GetLimitsSpringSettings());
}

void JPH_HingeConstraint_SetLimitsSpringSettings(JPH_HingeConstraint* constraint, JPH_SpringSettings* settings)
{
    reinterpret_cast<JPH::HingeConstraint*>(constraint)->SetLimitsSpringSettings(*reinterpret_cast<JPH::SpringSettings*>(settings));
}

/* JPH_SliderConstraintSettings */

JPH_SliderConstraintSettings* JPH_SliderConstraintSettings_Create(void)
{
    auto settings = new JPH::SliderConstraintSettings();
    return reinterpret_cast<JPH_SliderConstraintSettings*>(settings);
}

void JPH_SliderConstraintSettings_SetSliderAxis(JPH_SliderConstraintSettings* settings, const JPH_Vec3* axis)
{
    JPH_ASSERT(settings);

    auto joltSettings = reinterpret_cast<JPH::SliderConstraintSettings*>(settings);
    joltSettings->SetSliderAxis(ToVec3(axis));
}

void JPH_SliderConstraintSettings_GetSliderAxis(JPH_SliderConstraintSettings* settings, JPH_Vec3* axis)
{
	JPH_ASSERT(settings);

    auto joltSettings = reinterpret_cast<JPH::SliderConstraintSettings*>(settings);
    FromVec3(joltSettings->mSliderAxis1, axis);
}

float JPH_SliderConstraint_GetCurrentPosition(JPH_SliderConstraint* constraint)
{
    return reinterpret_cast<JPH::SliderConstraint*>(constraint)->GetCurrentPosition();
}

void JPH_SliderConstraint_SetLimits(JPH_SliderConstraint* constraint, float inLimitsMin, float inLimitsMax)
{
    return reinterpret_cast<JPH::SliderConstraint*>(constraint)->SetLimits(inLimitsMin, inLimitsMax);
}

float JPH_SliderConstraint_GetLimitsMin(JPH_SliderConstraint* constraint)
{
    return reinterpret_cast<JPH::SliderConstraint*>(constraint)->GetLimitsMin();
}

float JPH_SliderConstraint_GetLimitsMax(JPH_SliderConstraint* constraint)
{
    return reinterpret_cast<JPH::SliderConstraint*>(constraint)->GetLimitsMax();
}

JPH_Bool32 JPH_SliderConstraint_HasLimits(JPH_SliderConstraint* constraint)
{
    return reinterpret_cast<JPH::SliderConstraint*>(constraint)->HasLimits();
}

JPH_SliderConstraint* JPH_SliderConstraintSettings_CreateConstraint(JPH_SliderConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2)
{
    auto joltBody1 = reinterpret_cast<JPH::Body*>(body1);
    auto joltBody2 = reinterpret_cast<JPH::Body*>(body2);
    JPH::TwoBodyConstraint* constraint = reinterpret_cast<JPH::SliderConstraintSettings*>(settings)->Create(*joltBody1, *joltBody2);
    return reinterpret_cast<JPH_SliderConstraint*>(static_cast<JPH::SliderConstraint*>(constraint));
}

void JPH_DistanceConstraint_SetDistance(JPH_DistanceConstraint* constraint, float minDistance, float maxDistance)
{
    reinterpret_cast<JPH::DistanceConstraint*>(constraint)->SetDistance(minDistance, maxDistance);
}

float JPH_DistanceConstraint_GetMinDistance(JPH_DistanceConstraint* constraint)
{
    return reinterpret_cast<JPH::DistanceConstraint*>(constraint)->GetMinDistance();
}

float JPH_DistanceConstraint_GetMaxDistance(JPH_DistanceConstraint* constraint)
{
    return reinterpret_cast<JPH::DistanceConstraint*>(constraint)->GetMaxDistance();
}

JPH_SpringSettings* JPH_DistanceConstraint_GetLimitsSpringSettings(JPH_DistanceConstraint* constraint)
{
    return reinterpret_cast<JPH_SpringSettings*>(&reinterpret_cast<JPH::DistanceConstraint*>(constraint)->GetLimitsSpringSettings());
}

void JPH_DistanceConstraint_SetLimitsSpringSettings(JPH_DistanceConstraint* constraint, JPH_SpringSettings* settings)
{
    reinterpret_cast<JPH::DistanceConstraint*>(constraint)->SetLimitsSpringSettings(*reinterpret_cast<JPH::SpringSettings*>(settings));
}

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
    JPH_ASSERT(settings);
    JPH_ASSERT(body1);
    JPH_ASSERT(body2);
    auto joltBody1 = reinterpret_cast<JPH::Body*>(body1);
    auto joltBody2 = reinterpret_cast<JPH::Body*>(body2);
    JPH::TwoBodyConstraint* constraint = reinterpret_cast<JPH::PointConstraintSettings*>(settings)->Create(*joltBody1, *joltBody2);
    return reinterpret_cast<JPH_PointConstraint*>(static_cast<JPH::PointConstraint*>(constraint));
}

/* JPH_PointConstraint */
void JPH_PointConstraint_SetPoint1(JPH_PointConstraint* constraint, JPH_ConstraintSpace space, JPH_RVec3* value)
{
    JPH_ASSERT(constraint);
    auto joltConstraint = reinterpret_cast<JPH::PointConstraint*>(constraint);
    joltConstraint->SetPoint1(static_cast<JPH::EConstraintSpace>(space), ToRVec3(value));
}

void JPH_PointConstraint_SetPoint2(JPH_PointConstraint* constraint, JPH_ConstraintSpace space, JPH_RVec3* value)
{
    JPH_ASSERT(constraint);
    auto joltConstraint = reinterpret_cast<JPH::PointConstraint*>(constraint);
    joltConstraint->SetPoint2(static_cast<JPH::EConstraintSpace>(space), ToRVec3(value));
}


/* JPH_TwoBodyConstraint */
JPH_Body* JPH_TwoBodyConstraint_GetBody1(JPH_TwoBodyConstraint* constraint)
{
    JPH_ASSERT(constraint);
    auto joltConstraint = reinterpret_cast<JPH::TwoBodyConstraint*>(constraint);
    auto joltBody = joltConstraint->GetBody1();
    return reinterpret_cast<JPH_Body*>(joltBody);
}

JPH_Body* JPH_TwoBodyConstraint_GetBody2(JPH_TwoBodyConstraint* constraint)
{
    JPH_ASSERT(constraint);
    auto joltConstraint = reinterpret_cast<JPH::TwoBodyConstraint*>(constraint);
    auto joltBody = joltConstraint->GetBody2();
    return reinterpret_cast<JPH_Body*>(joltBody);
}

void JPH_TwoBodyConstraint_GetConstraintToBody1Matrix(JPH_TwoBodyConstraint* constraint, JPH_Matrix4x4* result)
{
    auto joltMatrix = reinterpret_cast<const JPH::TwoBodyConstraint*>(constraint)->GetConstraintToBody1Matrix();
    FromJolt(joltMatrix, result);
}

void JPH_TwoBodyConstraint_GetConstraintToBody2Matrix(JPH_TwoBodyConstraint* constraint, JPH_Matrix4x4* result)
{
    auto joltMatrix = reinterpret_cast<const JPH::TwoBodyConstraint*>(constraint)->GetConstraintToBody2Matrix();
    FromJolt(joltMatrix, result);
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

JPH_PhysicsUpdateError JPH_PhysicsSystem_Update(JPH_PhysicsSystem* system, float deltaTime, int collisionSteps,
    JPH_TempAllocator* tempAllocator,
    JPH_JobSystem* jobSystem)
{
    JPH_ASSERT(system);

    auto joltSystem = reinterpret_cast<JPH::PhysicsSystem*>(system);
    auto joltTempAllocator = reinterpret_cast<JPH::TempAllocator*>(tempAllocator);
    auto joltJobSystem = reinterpret_cast<JPH::JobSystemThreadPool*>(jobSystem);
    return static_cast<JPH_PhysicsUpdateError>(joltSystem->Update(deltaTime, collisionSteps, joltTempAllocator, joltJobSystem));
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

/* JPH_MotionProperties */
void JPH_MotionProperties_SetLinearDamping(JPH_MotionProperties* properties, float damping)
{
    reinterpret_cast<JPH::MotionProperties*>(properties)->SetLinearDamping(damping);
}

float JPH_MotionProperties_GetLinearDamping(const JPH_MotionProperties* properties)
{
    return reinterpret_cast<const JPH::MotionProperties*>(properties)->GetLinearDamping();
}

void JPH_MotionProperties_SetAngularDamping(JPH_MotionProperties* properties, float damping)
{
    reinterpret_cast<JPH::MotionProperties*>(properties)->SetAngularDamping(damping);
}

float JPH_MotionProperties_GetAngularDamping(const JPH_MotionProperties* properties)
{
    return reinterpret_cast<const JPH::MotionProperties*>(properties)->GetAngularDamping();
}

float JPH_MotionProperties_GetInverseMassUnchecked(JPH_MotionProperties* properties)
{
    return reinterpret_cast<JPH::MotionProperties*>(properties)->GetInverseMassUnchecked();
}

void JPH_MotionProperties_SetMassProperties(JPH_MotionProperties* properties, JPH_AllowedDOFs allowedDOFs, const JPH_MassProperties* massProperties)
{
    reinterpret_cast<JPH::MotionProperties*>(properties)->SetMassProperties(
        static_cast<EAllowedDOFs>(allowedDOFs),
        ToJolt(massProperties));
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

uint32_t JPH_PhysicsSystem_GetNumActiveBodies(const JPH_PhysicsSystem* system, JPH_BodyType type)
{
    JPH_ASSERT(system);

    auto joltSystem = reinterpret_cast<const JPH::PhysicsSystem*>(system);
    return joltSystem->GetNumActiveBodies(static_cast<JPH::EBodyType>(type));
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

    Array<Constraint*> joltConstraints;
	joltConstraints.reserve(count);
    for (uint32_t i = 0; i < count; ++i)
    {
        auto joltConstraint = reinterpret_cast<JPH::Constraint*>(constraints[i]);
        joltConstraints.push_back(joltConstraint);
    }

    auto joltSystem = reinterpret_cast<JPH::PhysicsSystem*>(system);
    joltSystem->AddConstraints(joltConstraints.data(), (int)count);
}

JPH_CAPI void JPH_PhysicsSystem_RemoveConstraints(JPH_PhysicsSystem* system, JPH_Constraint** constraints, uint32_t count)
{
    JPH_ASSERT(system);
    JPH_ASSERT(constraints);
    JPH_ASSERT(count > 0);

    Array<Constraint*> joltConstraints;
	joltConstraints.reserve(count);
    for (uint32_t i = 0; i < count; ++i)
    {
        auto joltConstraint = reinterpret_cast<JPH::Constraint*>(constraints[i]);
        joltConstraints.push_back(joltConstraint);
    }

    auto joltSystem = reinterpret_cast<JPH::PhysicsSystem*>(system);
    joltSystem->RemoveConstraints(joltConstraints.data(), (int)count);
}

JPH_Body* JPH_BodyInterface_CreateBody(JPH_BodyInterface* interface, JPH_BodyCreationSettings* settings)
{
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    auto body = joltBodyInterface->CreateBody(
        *reinterpret_cast<const JPH::BodyCreationSettings*>(settings)
    );

    return reinterpret_cast<JPH_Body*>(body);
}

JPH_Body* JPH_BodyInterface_CreateSoftBody(JPH_BodyInterface* interface, JPH_SoftBodyCreationSettings* settings)
{
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    auto body = joltBodyInterface->CreateSoftBody(
        *reinterpret_cast<const JPH::SoftBodyCreationSettings*>(settings)
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

JPH_BodyID JPH_BodyInterface_CreateAndAddBody(JPH_BodyInterface* interface, JPH_BodyCreationSettings* settings, JPH_Activation activationMode)
{
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    JPH::BodyID bodyID = joltBodyInterface->CreateAndAddBody(
        *reinterpret_cast<const JPH::BodyCreationSettings*>(settings),
        (JPH::EActivation)activationMode
    );

    return bodyID.GetIndexAndSequenceNumber();
}

void JPH_BodyInterface_DestroyBody(JPH_BodyInterface* interface, JPH_BodyID bodyID)
{
    JPH_ASSERT(interface);

    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    joltBodyInterface->DestroyBody(JPH::BodyID(bodyID));
}

void JPH_BodyInterface_AddBody(JPH_BodyInterface* interface, JPH_BodyID bodyID, JPH_Activation activationMode)
{
    JPH_ASSERT(interface);

    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    joltBodyInterface->AddBody(JPH::BodyID(bodyID), (JPH::EActivation)activationMode);
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

JPH_BodyType JPH_BodyInterface_GetBodyType(JPH_BodyInterface* interface, JPH_BodyID bodyID)
{
    JPH_ASSERT(interface);

    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    return static_cast<JPH_BodyType>(joltBodyInterface->GetBodyType(JPH::BodyID(bodyID)));
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

void JPH_BodyInterface_SetMotionType(JPH_BodyInterface* interface, JPH_BodyID bodyID, JPH_MotionType motionType, JPH_Activation activationMode)
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

void JPH_BodyInterface_SetPosition(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* position, JPH_Activation activationMode)
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

void JPH_BodyInterface_SetRotation(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Quat* rotation, JPH_Activation activationMode)
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

void JPH_BodyInterface_SetPositionAndRotation(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* position, JPH_Quat* rotation, JPH_Activation activationMode)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->SetPositionAndRotation(JPH::BodyID(bodyId), ToRVec3(position), ToQuat(rotation), static_cast<JPH::EActivation>(activationMode));
}

void JPH_BodyInterface_SetPositionAndRotationWhenChanged(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* position, JPH_Quat* rotation, JPH_Activation activationMode)
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

void JPH_BodyInterface_SetShape(JPH_BodyInterface* interface, JPH_BodyID bodyId, const JPH_Shape* shape, JPH_Bool32 updateMassProperties, JPH_Activation activationMode)
{
    JPH_ASSERT(interface);
    auto jolyBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    auto jphShape = reinterpret_cast<const JPH::Shape*>(shape);

    // !! is to make ms compiler happy.
    jolyBodyInterface->SetShape(JPH::BodyID(bodyId), jphShape, !!updateMassProperties, static_cast<JPH::EActivation>(activationMode));
}

void JPH_BodyInterface_NotifyShapeChanged(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* previousCenterOfMass, JPH_Bool32 updateMassProperties, JPH_Activation activationMode)
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

void JPH_BodyInterface_GetWorldTransform(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RMatrix4x4* result)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    const JPH::RMat44& mat = joltBodyInterface->GetWorldTransform(JPH::BodyID(bodyId));
    FromJolt(mat, result);
}

void JPH_BodyInterface_GetCenterOfMassTransform(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RMatrix4x4* resutlt)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    const JPH::RMat44& mat = joltBodyInterface->GetCenterOfMassTransform(JPH::BodyID(bodyId));
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

void JPH_BodyInterface_SetMotionQuality(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_MotionQuality quality)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->SetMotionQuality(JPH::BodyID(bodyId), static_cast<JPH::EMotionQuality>(quality));
}

JPH_MotionQuality JPH_BodyInterface_GetMotionQuality(JPH_BodyInterface* interface, JPH_BodyID bodyId)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    return static_cast<JPH_MotionQuality>(joltBodyInterface->GetMotionQuality(JPH::BodyID(bodyId)));
}

void JPH_BodyInterface_GetInverseInertia(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Matrix4x4* result)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    const JPH::Mat44& mat = joltBodyInterface->GetInverseInertia(JPH::BodyID(bodyId));
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

void JPH_BodyInterface_SetUserData(JPH_BodyInterface* interface, JPH_BodyID bodyId, uint64_t userData)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    joltBodyInterface->SetUserData(JPH::BodyID(bodyId), userData);
}

uint64_t JPH_BodyInterface_GetUserData(JPH_BodyInterface* interface, JPH_BodyID bodyId)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    return joltBodyInterface->GetUserData(JPH::BodyID(bodyId));
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
JPH_Bool32 JPH_NarrowPhaseQuery_CastRay(const JPH_NarrowPhaseQuery* query,
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

JPH_BodyType JPH_Body_GetBodyType(const JPH_Body* body)
{
    auto joltBody = reinterpret_cast<const JPH::Body*>(body);
    return static_cast<JPH_BodyType>(joltBody->GetBodyType());
}

void JPH_Body_GetWorldSpaceBounds(const JPH_Body* body, JPH_AABox* result)
{
	JPH_ASSERT(body);
	JPH_ASSERT(result);

    auto joltBody = reinterpret_cast<const JPH::Body*>(body);
    auto bounds = joltBody->GetWorldSpaceBounds();
    FromVec3(bounds.mMin, &result->min);
	FromVec3(bounds.mMax, &result->max);
}

JPH_Bool32 JPH_Body_IsActive(const JPH_Body* body)
{
    return reinterpret_cast<const JPH::Body*>(body)->IsActive();
}

JPH_Bool32 JPH_Body_IsStatic(const JPH_Body* body)
{
    return reinterpret_cast<const JPH::Body*>(body)->IsStatic();
}

JPH_Bool32 JPH_Body_IsKinematic(const JPH_Body* body)
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

void JPH_Body_SetIsSensor(JPH_Body* body, JPH_Bool32 value)
{
    reinterpret_cast<JPH::Body*>(body)->SetIsSensor(!!value);
}

JPH_MotionProperties* JPH_Body_GetMotionProperties(JPH_Body* body)
{
    return reinterpret_cast<JPH_MotionProperties*>(reinterpret_cast<JPH::Body*>(body)->GetMotionProperties());
}

JPH_MotionType JPH_Body_GetMotionType(const JPH_Body* body)
{
    return static_cast<JPH_MotionType>(reinterpret_cast<const JPH::Body*>(body)->GetMotionType());
}

void JPH_Body_SetMotionType(JPH_Body* body, JPH_MotionType motionType)
{
    reinterpret_cast<JPH::Body*>(body)->SetMotionType(static_cast<JPH::EMotionType>(motionType));
}

JPH_Bool32 JPH_Body_GetAllowSleeping(JPH_Body* body)
{
    return reinterpret_cast<JPH::Body*>(body)->GetAllowSleeping();
}

void JPH_Body_SetAllowSleeping(JPH_Body* body, JPH_Bool32 allowSleeping)
{
    reinterpret_cast<JPH::Body*>(body)->SetAllowSleeping(!!allowSleeping);
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

void JPH_Body_GetWorldTransform(const JPH_Body* body, JPH_RMatrix4x4* result)
{
    auto joltMatrix = reinterpret_cast<const JPH::Body*>(body)->GetWorldTransform();
    FromJolt(joltMatrix, result);
}

void JPH_Body_GetCenterOfMassTransform(const JPH_Body* body, JPH_RMatrix4x4* result)
{
    auto joltMatrix = reinterpret_cast<const JPH::Body*>(body)->GetCenterOfMassTransform();
    FromJolt(joltMatrix, result);
}

void JPH_Body_SetUserData(JPH_Body* body, uint64_t userData)
{
    reinterpret_cast<JPH::Body*>(body)->SetUserData(userData);
}

uint64_t JPH_Body_GetUserData(JPH_Body* body)
{
    return reinterpret_cast<JPH::Body*>(body)->GetUserData();
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
