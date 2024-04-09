// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

#include "joltc.h"

#ifdef _MSC_VER
__pragma(warning(push, 0))
#endif

#include "Jolt/Jolt.h"
#include "Jolt/RegisterTypes.h"
#include "Jolt/Core/Factory.h"
#include "Jolt/Core/TempAllocator.h"
#include "Jolt/Core/JobSystemThreadPool.h"
#include "Jolt/Physics/PhysicsSettings.h"
#include "Jolt/Physics/PhysicsSystem.h"
#include <Jolt/Physics/Collision/BroadPhase/BroadPhaseLayerInterfaceMask.h>
#include <Jolt/Physics/Collision/BroadPhase/ObjectVsBroadPhaseLayerFilterMask.h>
#include <Jolt/Physics/Collision/ObjectLayerPairFilterMask.h>
#include "Jolt/Physics/Collision/BroadPhase/BroadPhaseLayerInterfaceTable.h"
#include "Jolt/Physics/Collision/BroadPhase/ObjectVsBroadPhaseLayerFilterTable.h"
#include "Jolt/Physics/Collision/ObjectLayerPairFilterTable.h"
#include "Jolt/Physics/Collision/CastResult.h"
#include "Jolt/Physics/Collision/CollideShape.h"
#include <Jolt/Physics/Collision/CollisionCollectorImpl.h>
#include <Jolt/Physics/Collision/ShapeCast.h>
#include "Jolt/Physics/Collision/Shape/BoxShape.h"
#include "Jolt/Physics/Collision/Shape/SphereShape.h"
#include "Jolt/Physics/Collision/Shape/TriangleShape.h"
#include "Jolt/Physics/Collision/Shape/CapsuleShape.h"
#include "Jolt/Physics/Collision/Shape/TaperedCapsuleShape.h"
#include "Jolt/Physics/Collision/Shape/CylinderShape.h"
#include "Jolt/Physics/Collision/Shape/ConvexHullShape.h"
#include "Jolt/Physics/Collision/Shape/MeshShape.h"
#include "Jolt/Physics/Collision/Shape/HeightFieldShape.h"
#include "Jolt/Physics/Collision/Shape/StaticCompoundShape.h"
#include "Jolt/Physics/Collision/Shape/MutableCompoundShape.h"
#include "Jolt/Physics/Collision/Shape/RotatedTranslatedShape.h"
#include "Jolt/Physics/Body/BodyCreationSettings.h"
#include "Jolt/Physics/Body/BodyActivationListener.h"
#include "Jolt/Physics/SoftBody/SoftBodyCreationSettings.h"
#include "Jolt/Physics/Collision/RayCast.h"
#include "Jolt/Physics/Collision/NarrowPhaseQuery.h"
#include "Jolt/Physics/Constraints/SpringSettings.h"
#include "Jolt/Physics/Constraints/PointConstraint.h"
#include "Jolt/Physics/Constraints/DistanceConstraint.h"
#include "Jolt/Physics/Constraints/HingeConstraint.h"
#include "Jolt/Physics/Constraints/SliderConstraint.h"
#include "Jolt/Physics/Constraints/SwingTwistConstraint.h"
#include "Jolt/Physics/Constraints/SixDOFConstraint.h"
#include "Jolt/Physics/Character/CharacterBase.h"
#include "Jolt/Physics/Character/CharacterVirtual.h"

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

// From Jolt conversion methods
static void FromJolt(const JPH::Vec3& vec, JPH_Vec3* result)
{
    result->x = vec.GetX();
    result->y = vec.GetY();
    result->z = vec.GetZ();
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
    result->m44 = 1.0;
}

#if defined(JPH_DOUBLE_PRECISION)
static void FromJolt(const JPH::RVec3& vec, JPH_RVec3* result)
{
    result->x = vec.GetX();
    result->y = vec.GetY();
    result->z = vec.GetZ();
}

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
#endif /* defined(JPH_DOUBLE_PRECISION) */

// To Jolt conversion methods
static JPH::Vec3 ToJolt(const JPH_Vec3* vec)
{
    return JPH::Vec3(vec->x, vec->y, vec->z);
}

static JPH::Quat ToJolt(const JPH_Quat* quat)
{
    return JPH::Quat(quat->x, quat->y, quat->z, quat->w);
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

static JPH::Float3 ToJoltFloat3(const JPH_Vec3& vec)
{
    return JPH::Float3(vec.x, vec.y, vec.z);
}

#if defined(JPH_DOUBLE_PRECISION)
static JPH::RVec3 ToJolt(const JPH_RVec3* vec)
{
    return JPH::RVec3(vec->x, vec->y, vec->z);
}

static JPH::RMat44 ToJolt(const JPH_RMatrix4x4& matrix)
{
    JPH::RMat44 result{};
    result.SetColumn4(0, JPH::Vec4(matrix.m11, matrix.m12, matrix.m13, matrix.m14));
    result.SetColumn4(1, JPH::Vec4(matrix.m21, matrix.m22, matrix.m23, matrix.m24));
    result.SetColumn4(2, JPH::Vec4(matrix.m31, matrix.m32, matrix.m33, matrix.m34));
    result.SetTranslation(JPH::RVec3(matrix.m41, matrix.m42, matrix.m43));
    return result;
}
#endif /* defined(JPH_DOUBLE_PRECISION) */

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

void JPH_MassProperties_ScaleToMass(JPH_MassProperties* properties, float mass)
{
    reinterpret_cast<JPH::MassProperties*>(properties)->ScaleToMass(mass);
}

static JPH::Triangle ToTriangle(const JPH_Triangle& triangle)
{
    return JPH::Triangle(ToJoltFloat3(triangle.v1), ToJoltFloat3(triangle.v2), ToJoltFloat3(triangle.v3), triangle.materialIndex);
}

static JPH::IndexedTriangle ToIndexedTriangle(const JPH_IndexedTriangle& triangle)
{
    return JPH::IndexedTriangle(triangle.i1, triangle.i2, triangle.i3, triangle.materialIndex);
}

static JPH::TempAllocatorImpl* s_TempAllocator = nullptr;
static JPH::JobSystemThreadPool* s_JobSystem  = nullptr;

JPH_Bool32 JPH_Init(uint32_t tempAllocatorSize)
{
    JPH::RegisterDefaultAllocator();

    // TODO
    JPH::Trace = TraceImpl;
    JPH_IF_ENABLE_ASSERTS(JPH::AssertFailed = AssertFailedImpl;)

    // Create a factory
    JPH::Factory::sInstance = new JPH::Factory();

    // Register all Jolt physics types
    JPH::RegisterTypes();

	// Init temp allocator
	s_TempAllocator = new TempAllocatorImpl(tempAllocatorSize ? tempAllocatorSize : 10 * 1024 * 1024);

	// Init Job system.
    s_JobSystem = new JPH::JobSystemThreadPool(JPH::cMaxPhysicsJobs, JPH::cMaxPhysicsBarriers, (int)std::thread::hardware_concurrency() - 1);

    return true;
}

void JPH_Shutdown(void)
{
	delete s_JobSystem; s_JobSystem = nullptr;
	delete s_TempAllocator; s_TempAllocator = nullptr;

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

/* JPH_BroadPhaseLayerInterface */
JPH_BroadPhaseLayerInterface* JPH_BroadPhaseLayerInterfaceMask_Create(uint32_t numBroadPhaseLayers)
{
    auto system = new BroadPhaseLayerInterfaceMask(numBroadPhaseLayers);
    return reinterpret_cast<JPH_BroadPhaseLayerInterface*>(system);
}

void JPH_BroadPhaseLayerInterfaceMask_ConfigureLayer(JPH_BroadPhaseLayerInterface* bpInterface, JPH_BroadPhaseLayer broadPhaseLayer, uint32_t groupsToInclude, uint32_t groupsToExclude)
{
	reinterpret_cast<JPH::BroadPhaseLayerInterfaceMask*>(bpInterface)->ConfigureLayer(
		static_cast<JPH::BroadPhaseLayer>(broadPhaseLayer),
		groupsToInclude,
		groupsToExclude
	);
}

JPH_BroadPhaseLayerInterface* JPH_BroadPhaseLayerInterfaceTable_Create(uint32_t numObjectLayers, uint32_t numBroadPhaseLayers)
{
    auto system = new BroadPhaseLayerInterfaceTable(numObjectLayers, numBroadPhaseLayers);
    return reinterpret_cast<JPH_BroadPhaseLayerInterface*>(system);
}

void JPH_BroadPhaseLayerInterfaceTable_MapObjectToBroadPhaseLayer(JPH_BroadPhaseLayerInterface* bpInterface, JPH_ObjectLayer objectLayer, JPH_BroadPhaseLayer broadPhaseLayer)
{
	reinterpret_cast<JPH::BroadPhaseLayerInterfaceTable*>(bpInterface)->MapObjectToBroadPhaseLayer(
		static_cast<JPH::ObjectLayer>(objectLayer),
		static_cast<JPH::BroadPhaseLayer>(broadPhaseLayer)
	);
}

/* JPH_ObjectLayerPairFilter */
JPH_ObjectLayerPairFilter* JPH_ObjectLayerPairFilterMask_Create(void)
{
	auto filter = new JPH::ObjectLayerPairFilterMask();
    return reinterpret_cast<JPH_ObjectLayerPairFilter*>(filter);
}

JPH_ObjectLayer JPH_ObjectLayerPairFilterMask_GetObjectLayer(uint32_t group, uint32_t mask)
{
	return static_cast<JPH_ObjectLayer>(JPH::ObjectLayerPairFilterMask::sGetObjectLayer(group, mask));
}

uint32_t JPH_ObjectLayerPairFilterMask_GetGroup(JPH_ObjectLayer layer)
{
	return JPH::ObjectLayerPairFilterMask::sGetGroup(static_cast<JPH::ObjectLayer>(layer));
}

uint32_t JPH_ObjectLayerPairFilterMask_GetMask(JPH_ObjectLayer layer)
{
	return JPH::ObjectLayerPairFilterMask::sGetMask(static_cast<JPH::ObjectLayer>(layer));
}

JPH_ObjectLayerPairFilter* JPH_ObjectLayerPairFilterTable_Create(uint32_t numObjectLayers)
{
    auto filter = new JPH::ObjectLayerPairFilterTable(numObjectLayers);
    return reinterpret_cast<JPH_ObjectLayerPairFilter*>(filter);
}

void JPH_ObjectLayerPairFilterTable_DisableCollision(JPH_ObjectLayerPairFilter* objectFilter, JPH_ObjectLayer layer1, JPH_ObjectLayer layer2)
{
	reinterpret_cast<JPH::ObjectLayerPairFilterTable*>(objectFilter)->DisableCollision(
		static_cast<JPH::ObjectLayer>(layer1),
		static_cast<JPH::ObjectLayer>(layer2)
	);
}

void JPH_ObjectLayerPairFilterTable_EnableCollision(JPH_ObjectLayerPairFilter* objectFilter, JPH_ObjectLayer layer1, JPH_ObjectLayer layer2)
{
	reinterpret_cast<JPH::ObjectLayerPairFilterTable*>(objectFilter)->EnableCollision(
		static_cast<JPH::ObjectLayer>(layer1),
		static_cast<JPH::ObjectLayer>(layer2)
	);
}

JPH_Bool32 JPH_ObjectLayerPairFilterTable_ShouldCollide(JPH_ObjectLayerPairFilter* objectFilter, JPH_ObjectLayer layer1, JPH_ObjectLayer layer2)
{
    return reinterpret_cast<JPH::ObjectLayerPairFilterTable*>(objectFilter)->ShouldCollide(
        static_cast<JPH::ObjectLayer>(layer1),
        static_cast<JPH::ObjectLayer>(layer2)
    );
}

/* JPH_ObjectVsBroadPhaseLayerFilter */
JPH_ObjectVsBroadPhaseLayerFilter* JPH_ObjectVsBroadPhaseLayerFilterMask_Create(const JPH_BroadPhaseLayerInterface* broadPhaseLayerInterface)
{
	auto joltBroadPhaseLayerInterface = reinterpret_cast<const JPH::BroadPhaseLayerInterfaceMask*>(broadPhaseLayerInterface);
	auto filter = new JPH::ObjectVsBroadPhaseLayerFilterMask(*joltBroadPhaseLayerInterface);
    return reinterpret_cast<JPH_ObjectVsBroadPhaseLayerFilter*>(filter);
}

JPH_ObjectVsBroadPhaseLayerFilter* JPH_ObjectVsBroadPhaseLayerFilterTable_Create(
	JPH_BroadPhaseLayerInterface* broadPhaseLayerInterface, uint32_t numBroadPhaseLayers,
	JPH_ObjectLayerPairFilter* objectLayerPairFilter, uint32_t numObjectLayers)
{
	auto joltBroadPhaseLayerInterface = reinterpret_cast<JPH::BroadPhaseLayerInterface*>(broadPhaseLayerInterface);
	auto joltObjectLayerPairFilter = reinterpret_cast<JPH::ObjectLayerPairFilter*>(objectLayerPairFilter);

    auto filter = new JPH::ObjectVsBroadPhaseLayerFilterTable(*joltBroadPhaseLayerInterface, numBroadPhaseLayers, *joltObjectLayerPairFilter, numObjectLayers);
    return reinterpret_cast<JPH_ObjectVsBroadPhaseLayerFilter*>(filter);
}

/* JPH_PhysicsSystem */
struct JPH_PhysicsSystem final
{
	JPH::BroadPhaseLayerInterface* broadPhaseLayerInterface = nullptr;
	JPH::ObjectLayerPairFilter*	objectLayerPairFilter = nullptr;
	JPH::ObjectVsBroadPhaseLayerFilter* objectVsBroadPhaseLayerFilter = nullptr;
	JPH::PhysicsSystem* physicsSystem = nullptr;
};

JPH_PhysicsSystem* JPH_PhysicsSystem_Create(const JPH_PhysicsSystemSettings* settings)
{
	if(!settings)
		return nullptr;

	JPH_PhysicsSystem* system = new JPH_PhysicsSystem();
	system->broadPhaseLayerInterface = reinterpret_cast<JPH::BroadPhaseLayerInterface*>(settings->broadPhaseLayerInterface);
	system->objectLayerPairFilter = reinterpret_cast<JPH::ObjectLayerPairFilter*>(settings->objectLayerPairFilter);
	system->objectVsBroadPhaseLayerFilter = reinterpret_cast<JPH::ObjectVsBroadPhaseLayerFilter*>(settings->objectVsBroadPhaseLayerFilter);

	// Init the physics system
	const uint maxBodies = settings->maxBodies ? settings->maxBodies : 10240;
	constexpr uint cNumBodyMutexes = 0;
	const uint maxBodyPairs = settings->maxBodyPairs ? settings->maxBodyPairs : 65536;
	const uint maxContactConstraints = settings->maxContactConstraints ? settings->maxContactConstraints : 10240;
	system->physicsSystem = new PhysicsSystem();
	system->physicsSystem->Init(maxBodies, cNumBodyMutexes, maxBodyPairs, maxContactConstraints,
		*system->broadPhaseLayerInterface,
		*system->objectVsBroadPhaseLayerFilter,
		*system->objectLayerPairFilter);

    return system;
}

void JPH_PhysicsSystem_Destroy(JPH_PhysicsSystem* system)
{
    if (system)
    {
        delete system->physicsSystem;
		delete system->broadPhaseLayerInterface;
		delete system->objectVsBroadPhaseLayerFilter;
		delete system->objectLayerPairFilter;

		delete system;
    }
}

void JPH_PhysicsSystem_OptimizeBroadPhase(JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    system->physicsSystem->OptimizeBroadPhase();
}

JPH_PhysicsUpdateError JPH_PhysicsSystem_Step(JPH_PhysicsSystem* system, float deltaTime, int collisionSteps)
{
    JPH_ASSERT(system);

    return static_cast<JPH_PhysicsUpdateError>(system->physicsSystem->Update(deltaTime, collisionSteps, s_TempAllocator, s_JobSystem));
}

JPH_BodyInterface* JPH_PhysicsSystem_GetBodyInterface(JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    return reinterpret_cast<JPH_BodyInterface*>(&system->physicsSystem->GetBodyInterface());
}

JPH_BodyInterface* JPH_PhysicsSystem_GetBodyInterfaceNoLock(JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    return reinterpret_cast<JPH_BodyInterface*>(&system->physicsSystem->GetBodyInterfaceNoLock());
}

const JPH_BodyLockInterface* JPC_PhysicsSystem_GetBodyLockInterface(const JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    return reinterpret_cast<const JPH_BodyLockInterface*>(&system->physicsSystem->GetBodyLockInterface());
}
const JPH_BodyLockInterface* JPC_PhysicsSystem_GetBodyLockInterfaceNoLock(const JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    return reinterpret_cast<const JPH_BodyLockInterface*>(&system->physicsSystem->GetBodyLockInterfaceNoLock());
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

JPH_BroadPhaseLayerFilter* JPH_BroadPhaseLayerFilter_Create(void)
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

JPH_ObjectLayerFilter* JPH_ObjectLayerFilter_Create(void)
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

JPH_BodyFilter* JPH_BodyFilter_Create(void)
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

/* Math */
void JPH_Quaternion_FromTo(const JPH_Vec3* from, const JPH_Vec3* to, JPH_Quat* quat)
{
    FromJolt(JPH::Quat::sFromTo(ToJolt(from), ToJolt(to)), quat);
}

/* ShapeSettings */
void JPH_ShapeSettings_Destroy(JPH_ShapeSettings* settings)
{
    if (settings)
    {
        auto joltSettings = reinterpret_cast<JPH::ShapeSettings*>(settings);
        joltSettings->Release();
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

float JPH_ConvexShapeSettings_GetDensity(const JPH_ConvexShapeSettings* shape)
{
	return reinterpret_cast<const JPH::ConvexShapeSettings*>(shape)->mDensity;
}

void JPH_ConvexShapeSettings_SetDensity(JPH_ConvexShapeSettings* shape, float value)
{
	reinterpret_cast<JPH::ConvexShapeSettings*>(shape)->SetDensity(value);
}

/* BoxShape */
JPH_BoxShapeSettings* JPH_BoxShapeSettings_Create(const JPH_Vec3* halfExtent, float convexRadius)
{
    auto settings = new JPH::BoxShapeSettings(ToJolt(halfExtent), convexRadius);
    settings->AddRef();

    return reinterpret_cast<JPH_BoxShapeSettings*>(settings);
}

JPH_BoxShape* JPH_BoxShapeSettings_CreateShape(const JPH_BoxShapeSettings* settings)
{
	const JPH::BoxShapeSettings* jolt_settings = reinterpret_cast<const JPH::BoxShapeSettings*>(settings);
    auto shape_res = jolt_settings->Create();

    auto shape = shape_res.Get().GetPtr();
    shape->AddRef();

    return reinterpret_cast<JPH_BoxShape*>(shape);
}

JPH_BoxShape* JPH_BoxShape_Create(const JPH_Vec3* halfExtent, float convexRadius)
{
    auto shape = new JPH::BoxShape(ToJolt(halfExtent), convexRadius);
    shape->AddRef();

    return reinterpret_cast<JPH_BoxShape*>(shape);
}

void JPH_BoxShape_GetHalfExtent(const JPH_BoxShape* shape, JPH_Vec3* halfExtent)
{
    auto joltShape = reinterpret_cast<const JPH::BoxShape*>(shape);
    auto joltVector = joltShape->GetHalfExtent();
    FromJolt(joltVector, halfExtent);
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
    settings->AddRef();

    return reinterpret_cast<JPH_SphereShapeSettings*>(settings);
}

JPH_SphereShape* JPH_SphereShapeSettings_CreateShape(const JPH_SphereShapeSettings* settings)
{
	const JPH::SphereShapeSettings* jolt_settings = reinterpret_cast<const JPH::SphereShapeSettings*>(settings);
    auto shape_res = jolt_settings->Create();

    auto shape = shape_res.Get().GetPtr();
    shape->AddRef();

    return reinterpret_cast<JPH_SphereShape*>(shape);
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
    shape->AddRef();

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
    auto settings = new JPH::TriangleShapeSettings(ToJolt(v1), ToJolt(v2), ToJolt(v3), convexRadius);
    settings->AddRef();

    return reinterpret_cast<JPH_TriangleShapeSettings*>(settings);
}

/* CapsuleShapeSettings */
JPH_CapsuleShapeSettings* JPH_CapsuleShapeSettings_Create(float halfHeightOfCylinder, float radius)
{
    auto settings = new JPH::CapsuleShapeSettings(halfHeightOfCylinder, radius);
    settings->AddRef();

    return reinterpret_cast<JPH_CapsuleShapeSettings*>(settings);
}

JPH_CapsuleShape* JPH_CapsuleShape_Create(float halfHeightOfCylinder, float radius)
{
    auto shape = new JPH::CapsuleShape(halfHeightOfCylinder, radius, 0);
    shape->AddRef();

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
    settings->AddRef();

    return reinterpret_cast<JPH_CylinderShapeSettings*>(settings);
}

JPH_CylinderShape* JPH_CylinderShape_Create(float halfHeight, float radius)
{
    auto shape = new JPH::CylinderShape(halfHeight, radius, 0.f, 0);
    shape->AddRef();

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
JPH_ConvexHullShapeSettings* JPH_ConvexHullShapeSettings_Create(const JPH_Vec3* points, uint32_t pointsCount, float maxConvexRadius)
{
    Array<Vec3> joltPoints;
    joltPoints.reserve(pointsCount);

    for (uint32_t i = 0; i < pointsCount; i++)
    {
        joltPoints.push_back(ToJolt(&points[i]));
    }

    auto settings = new JPH::ConvexHullShapeSettings(joltPoints, maxConvexRadius);
    settings->AddRef();

    return reinterpret_cast<JPH_ConvexHullShapeSettings*>(settings);
}

JPH_ConvexHullShape* JPH_ConvexHullShapeSettings_CreateShape(const JPH_ConvexHullShapeSettings* settings)
{
	const JPH::ConvexHullShapeSettings* jolt_settings = reinterpret_cast<const JPH::ConvexHullShapeSettings*>(settings);
    auto shape_res = jolt_settings->Create();

    auto shape = shape_res.Get().GetPtr();
    shape->AddRef();

    return reinterpret_cast<JPH_ConvexHullShape*>(shape);
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
    settings->AddRef();

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
        joltVertices.push_back(ToJoltFloat3(vertices[i]));
    }

    for (uint32_t i = 0; i < triangleCount; ++i)
    {
        joltTriangles.push_back(ToIndexedTriangle(triangles[i]));
    }

    auto settings = new JPH::MeshShapeSettings(joltVertices, joltTriangles);
    settings->AddRef();

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

    auto shape = shape_res.Get().GetPtr();
    shape->AddRef();

    return reinterpret_cast<JPH_MeshShape*>(shape);
}

/* HeightFieldShapeSettings */
JPH_HeightFieldShapeSettings* JPH_HeightFieldShapeSettings_Create(const float* samples, const JPH_Vec3* offset, const JPH_Vec3* scale, uint32_t sampleCount)
{
    auto settings = new JPH::HeightFieldShapeSettings(samples, ToJolt(offset), ToJolt(scale), sampleCount);
    settings->AddRef();

    return reinterpret_cast<JPH_HeightFieldShapeSettings*>(settings);
}

JPH_HeightFieldShape* JPH_HeightFieldShapeSettings_CreateShape(JPH_HeightFieldShapeSettings* settings)
{
    const JPH::HeightFieldShapeSettings* jolt_settings = reinterpret_cast<const JPH::HeightFieldShapeSettings*>(settings);
    auto shape_res = jolt_settings->Create();

    auto shape = shape_res.Get().GetPtr();
    shape->AddRef();

    return reinterpret_cast<JPH_HeightFieldShape*>(shape);
}

void JPH_HeightFieldShapeSettings_DetermineMinAndMaxSample(const JPH_HeightFieldShapeSettings* settings, float* pOutMinValue, float* pOutMaxValue, float* pOutQuantizationScale)
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

uint32_t JPH_HeightFieldShapeSettings_CalculateBitsPerSampleForError(const JPH_HeightFieldShapeSettings* settings, float maxError)
{
    JPH_ASSERT(settings != nullptr);

    return reinterpret_cast<const JPH::HeightFieldShapeSettings*>(settings)->CalculateBitsPerSampleForError(maxError);
}

/* TaperedCapsuleShapeSettings */
JPH_TaperedCapsuleShapeSettings* JPH_TaperedCapsuleShapeSettings_Create(float halfHeightOfTaperedCylinder, float topRadius, float bottomRadius)
{
    auto settings = new JPH::TaperedCapsuleShapeSettings(halfHeightOfTaperedCylinder, topRadius, bottomRadius);
    settings->AddRef();

    return reinterpret_cast<JPH_TaperedCapsuleShapeSettings*>(settings);
}

/* CompoundShape */
void JPH_CompoundShapeSettings_AddShape(JPH_CompoundShapeSettings* settings, const JPH_Vec3* position, const JPH_Quat* rotation, const JPH_ShapeSettings* shape, uint32_t userData)
{
    auto joltShapeSettings = reinterpret_cast<const JPH::ShapeSettings*>(shape);
    auto joltSettings = reinterpret_cast<JPH::CompoundShapeSettings*>(settings);
    joltSettings->AddShape(ToJolt(position), ToJolt(rotation), joltShapeSettings, userData);
}

void JPH_CompoundShapeSettings_AddShape2(JPH_CompoundShapeSettings* settings, const JPH_Vec3* position, const JPH_Quat* rotation, const JPH_Shape* shape, uint32_t userData)
{
    auto joltShape = reinterpret_cast<const JPH::Shape*>(shape);
    auto joltSettings = reinterpret_cast<JPH::CompoundShapeSettings*>(settings);
    joltSettings->AddShape(ToJolt(position), ToJolt(rotation), joltShape, userData);
}

uint32_t JPH_CompoundShape_GetNumSubShapes(const JPH_CompoundShape* shape)
{
	JPH_ASSERT(shape);
	auto joltShape = reinterpret_cast<const JPH::CompoundShape*>(shape);
	return joltShape->GetNumSubShapes();
}

void JPH_CompoundShape_GetSubShape(const JPH_CompoundShape* shape, uint32_t index, const JPH_Shape** subShape, JPH_Vec3* positionCOM, JPH_Quat* rotation, uint32_t* userData)
{
	JPH_ASSERT(shape);
	auto joltShape = reinterpret_cast<const JPH::CompoundShape*>(shape);
	const JPH::CompoundShape::SubShape& sub = joltShape->GetSubShape(index);
	if (subShape) *subShape = reinterpret_cast<const JPH_Shape*>(sub.mShape.GetPtr());
	if (positionCOM) FromJolt(sub.GetPositionCOM(), positionCOM);
	if (rotation) FromJolt(sub.GetRotation(), rotation);
	if (userData) *userData = sub.mUserData;
}

uint32_t JPH_CompoundShape_GetSubShapeIndexFromID(const JPH_CompoundShape* shape, JPH_SubShapeID id, JPH_SubShapeID* remainder)
{
	JPH_ASSERT(shape);
	auto joltShape = reinterpret_cast<const JPH::CompoundShape*>(shape);
	auto joltSubShapeID = JPH::SubShapeID();
	joltSubShapeID.SetValue(id);
	JPH::SubShapeID joltRemainder = JPH::SubShapeID();
	uint32_t index = joltShape->GetSubShapeIndexFromID(joltSubShapeID, joltRemainder);
	*remainder = joltRemainder.GetValue();
	return index;
}

/* StaticCompoundShape */
JPH_StaticCompoundShapeSettings* JPH_StaticCompoundShapeSettings_Create(void)
{
    auto settings = new JPH::StaticCompoundShapeSettings();
    settings->AddRef();

    return reinterpret_cast<JPH_StaticCompoundShapeSettings*>(settings);
}

JPH_StaticCompoundShape* JPH_StaticCompoundShape_Create(const JPH_StaticCompoundShapeSettings* settings)
{
	const JPH::StaticCompoundShapeSettings* jolt_settings = reinterpret_cast<const JPH::StaticCompoundShapeSettings*>(settings);
    auto shape_res = jolt_settings->Create();

    auto shape = shape_res.Get().GetPtr();
    shape->AddRef();

    return reinterpret_cast<JPH_StaticCompoundShape*>(shape);
}

/* MutableCompoundShape */
JPH_CAPI JPH_MutableCompoundShapeSettings* JPH_MutableCompoundShapeSettings_Create(void)
{
    auto settings = new JPH::MutableCompoundShapeSettings();
    settings->AddRef();

    return reinterpret_cast<JPH_MutableCompoundShapeSettings*>(settings);
}

JPH_MutableCompoundShape* JPH_MutableCompoundShape_Create(const JPH_MutableCompoundShapeSettings* settings)
{
	const JPH::MutableCompoundShapeSettings* jolt_settings = reinterpret_cast<const JPH::MutableCompoundShapeSettings*>(settings);
    auto shape_res = jolt_settings->Create();

    auto shape = shape_res.Get().GetPtr();
    shape->AddRef();

    return reinterpret_cast<JPH_MutableCompoundShape*>(shape);
}

uint32_t JPH_MutableCompoundShape_AddShape(JPH_MutableCompoundShape* shape, const JPH_Vec3* position, const JPH_Quat* rotation, const JPH_Shape* child, uint32_t userData) {
    auto joltShape = reinterpret_cast<JPH::MutableCompoundShape*>(shape);
    auto joltChild = reinterpret_cast<const JPH::Shape*>(child);
    return joltShape->AddShape(ToJolt(position), ToJolt(rotation), joltChild, userData);
}

void JPH_MutableCompoundShape_RemoveShape(JPH_MutableCompoundShape* shape, uint32_t index) {
    reinterpret_cast<JPH::MutableCompoundShape*>(shape)->RemoveShape(index);
}

void JPH_MutableCompoundShape_ModifyShape(JPH_MutableCompoundShape* shape, uint32_t index, const JPH_Vec3* position, const JPH_Quat* rotation) {
    auto joltShape = reinterpret_cast<JPH::MutableCompoundShape*>(shape);
    joltShape->ModifyShape(index, ToJolt(position), ToJolt(rotation));
}

void JPH_MutableCompoundShape_ModifyShape2(JPH_MutableCompoundShape* shape, uint32_t index, const JPH_Vec3* position, const JPH_Quat* rotation, const JPH_Shape* newShape) {
    auto joltShape = reinterpret_cast<JPH::MutableCompoundShape*>(shape);
    auto joltNewShape = reinterpret_cast<const JPH::Shape*>(newShape);
    joltShape->ModifyShape(index, ToJolt(position), ToJolt(rotation), joltNewShape);
}

void JPH_MutableCompoundShape_AdjustCenterOfMass(JPH_MutableCompoundShape* shape) {
    reinterpret_cast<JPH::MutableCompoundShape*>(shape)->AdjustCenterOfMass();
}

/* RotatedTranslatedShape */
JPH_RotatedTranslatedShapeSettings* JPH_RotatedTranslatedShapeSettings_Create(const JPH_Vec3* position, const JPH_Quat* rotation, JPH_ShapeSettings* shapeSettings)
{
    auto jolt_settings = reinterpret_cast<JPH::ShapeSettings*>(shapeSettings);

    auto settings = new JPH::RotatedTranslatedShapeSettings(ToJolt(position), ToJolt(rotation), jolt_settings);
    settings->AddRef();

    return reinterpret_cast<JPH_RotatedTranslatedShapeSettings*>(settings);
}

JPH_RotatedTranslatedShapeSettings* JPH_RotatedTranslatedShapeSettings_Create2(const JPH_Vec3* position, const JPH_Quat* rotation, JPH_Shape* shape)
{
    auto jolt_shape = reinterpret_cast<JPH::Shape*>(shape);

    auto settings = new JPH::RotatedTranslatedShapeSettings(ToJolt(position), ToJolt(rotation), jolt_shape);
    settings->AddRef();

    return reinterpret_cast<JPH_RotatedTranslatedShapeSettings*>(settings);
}

JPH_RotatedTranslatedShape* JPH_RotatedTranslatedShapeSettings_CreateShape(const JPH_RotatedTranslatedShapeSettings* settings)
{
	const JPH::RotatedTranslatedShapeSettings* jolt_settings = reinterpret_cast<const JPH::RotatedTranslatedShapeSettings*>(settings);
    auto shape_res = jolt_settings->Create();

    auto shape = shape_res.Get().GetPtr();
    shape->AddRef();

    return reinterpret_cast<JPH_RotatedTranslatedShape*>(shape);
}

JPH_RotatedTranslatedShape* JPH_RotatedTranslatedShape_Create(const JPH_Vec3* position, const JPH_Quat* rotation, JPH_Shape* shape)
{
    auto jolt_shape = reinterpret_cast<JPH::Shape*>(shape);

    auto rotatedTranslatedShape = new JPH::RotatedTranslatedShape(ToJolt(position), ToJolt(rotation), jolt_shape);
    rotatedTranslatedShape->AddRef();

    return reinterpret_cast<JPH_RotatedTranslatedShape*>(rotatedTranslatedShape);
}

void JPH_RotatedTranslatedShape_GetPosition(const JPH_RotatedTranslatedShape* shape, JPH_Vec3* position)
{
	JPH_ASSERT(shape);
	auto joltShape = reinterpret_cast<const JPH::RotatedTranslatedShape*>(shape);
	JPH::Vec3 joltVector = joltShape->GetPosition();
	FromJolt(joltVector, position);
}

void JPH_RotatedTranslatedShape_GetRotation(const JPH_RotatedTranslatedShape* shape, JPH_Quat* rotation)
{
	JPH_ASSERT(shape);
	auto joltShape = reinterpret_cast<const JPH::RotatedTranslatedShape*>(shape);
	JPH::Quat joltQuat = joltShape->GetRotation();
	FromJolt(joltQuat, rotation);
}

/* Shape */
void JPH_Shape_Destroy(JPH_Shape* shape)
{
    if (shape)
    {
        auto joltShape = reinterpret_cast<JPH::Shape*>(shape);
        joltShape->Release();
    }
}

JPH_ShapeType JPH_Shape_GetType(const JPH_Shape* shape)
{
	return static_cast<JPH_ShapeType>(reinterpret_cast<const JPH::Shape*>(shape)->GetType());
}

JPH_ShapeSubType JPH_Shape_GetSubType(const JPH_Shape* shape)
{
	return static_cast<JPH_ShapeSubType>(reinterpret_cast<const JPH::Shape*>(shape)->GetSubType());
}

uint64_t JPH_Shape_GetUserData(const JPH_Shape* shape)
{
    return reinterpret_cast<const JPH::Shape*>(shape)->GetUserData();
}

void JPH_Shape_SetUserData(JPH_Shape* shape, uint64_t userData)
{
    reinterpret_cast<JPH::Shape*>(shape)->SetUserData(userData);
}

JPH_Bool32 JPH_Shape_MustBeStatic(const JPH_Shape* shape)
{
	return reinterpret_cast<const JPH::Shape*>(shape)->MustBeStatic();
}

void JPH_Shape_GetCenterOfMass(const JPH_Shape* shape, JPH_Vec3* result)
{
	auto joltShape = reinterpret_cast<const JPH::Shape*>(shape);
	auto joltVector = joltShape->GetCenterOfMass();
	FromJolt(joltVector, result);
}

void JPH_Shape_GetLocalBounds(const JPH_Shape* shape, JPH_AABox* result)
{
	JPH_ASSERT(shape);
	JPH_ASSERT(result);

    auto bounds = reinterpret_cast<const JPH::Shape*>(shape)->GetLocalBounds();
	FromJolt(bounds.mMin, &result->min);
	FromJolt(bounds.mMax, &result->max);
}

void JPH_Shape_GetWorldSpaceBounds(const JPH_Shape* shape, JPH_RMatrix4x4* centerOfMassTransform, JPH_Vec3* scale, JPH_AABox* result)
{
	auto joltShape = reinterpret_cast<const JPH::Shape*>(shape);
	auto bounds = joltShape->GetWorldSpaceBounds(ToJolt(*centerOfMassTransform), ToJolt(scale));
	FromJolt(bounds.mMin, &result->min);
	FromJolt(bounds.mMax, &result->max);
}

float JPH_Shape_GetInnerRadius(const JPH_Shape* shape)
{
	auto joltShape = reinterpret_cast<const JPH::Shape*>(shape);
	return joltShape->GetInnerRadius();
}

void JPH_Shape_GetMassProperties(const JPH_Shape* shape, JPH_MassProperties* result)
{
    auto joltShape = reinterpret_cast<const JPH::Shape*>(shape);
	FromJolt(joltShape->GetMassProperties(), result);
}

void JPH_Shape_GetSurfaceNormal(const JPH_Shape* shape, JPH_SubShapeID subShapeID, JPH_Vec3* localPosition, JPH_Vec3* normal)
{
    auto joltShape = reinterpret_cast<const JPH::Shape*>(shape);
    auto joltSubShapeID = JPH::SubShapeID();
    joltSubShapeID.SetValue(subShapeID);
    Vec3 joltNormal = joltShape->GetSurfaceNormal(joltSubShapeID, ToJolt(localPosition));
    FromJolt(joltNormal, normal);
}

float JPH_Shape_GetVolume(const JPH_Shape* shape)
{
	return reinterpret_cast<const JPH::Shape*>(shape)->GetVolume();
}

/* JPH_BodyCreationSettings */
JPH_BodyCreationSettings* JPH_BodyCreationSettings_Create(void)
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
        ToJolt(position),
        ToJolt(rotation),
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
        ToJolt(position),
        ToJolt(rotation),
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
    FromJolt(joltVector, velocity);
}

void JPH_BodyCreationSettings_SetLinearVelocity(JPH_BodyCreationSettings* settings, const JPH_Vec3* velocity)
{
    JPH_ASSERT(settings);

    reinterpret_cast<JPH::BodyCreationSettings*>(settings)->mLinearVelocity = ToJolt(velocity);
}

void JPH_BodyCreationSettings_GetAngularVelocity(JPH_BodyCreationSettings* settings, JPH_Vec3* velocity)
{
    JPH_ASSERT(settings);

    auto joltVector = reinterpret_cast<JPH::BodyCreationSettings*>(settings)->mAngularVelocity;
    FromJolt(joltVector, velocity);
}

void JPH_BodyCreationSettings_SetAngularVelocity(JPH_BodyCreationSettings* settings, const JPH_Vec3* velocity)
{
    JPH_ASSERT(settings);

    reinterpret_cast<JPH::BodyCreationSettings*>(settings)->mAngularVelocity = ToJolt(velocity);
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
JPH_SoftBodyCreationSettings* JPH_SoftBodyCreationSettings_Create(void)
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
        auto joltSettings = reinterpret_cast<JPH::ConstraintSettings*>(settings);
        joltSettings->Release();
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

uint64_t JPH_Constraint_GetUserData(const JPH_Constraint* constraint)
{
    return reinterpret_cast<const JPH::Constraint*>(constraint)->GetUserData();
}

void JPH_Constraint_SetUserData(JPH_Constraint* constraint, uint64_t userData)
{
    reinterpret_cast<JPH::Constraint*>(constraint)->SetUserData(userData);
}

void JPH_Constraint_Destroy(JPH_Constraint* constraint)
{
    if (constraint)
    {
        auto joltConstraint = reinterpret_cast<JPH::Constraint*>(constraint);
        joltConstraint->Release();
    }
}

JPH_SpringSettings* JPH_SpringSettings_Create(float frequency, float damping)
{
    auto settings = new JPH::SpringSettings(ESpringMode::FrequencyAndDamping, frequency, damping);
    return reinterpret_cast<JPH_SpringSettings*>(settings);
}

void JPH_SpringSettings_Destroy(JPH_SpringSettings* settings)
{
    if (settings)
    {
        delete reinterpret_cast<JPH::SpringSettings*>(settings);
    }
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
    settings->AddRef();

    return reinterpret_cast<JPH_DistanceConstraintSettings*>(settings);
}

void JPH_DistanceConstraintSettings_GetPoint1(JPH_DistanceConstraintSettings* settings, JPH_RVec3* result)
{
    JPH_ASSERT(settings);
	JPH_ASSERT(result);
    auto joltSettings = reinterpret_cast<JPH::PointConstraintSettings*>(settings);

    auto joltVector = joltSettings->mPoint1;
    FromJolt(joltVector, result);
}

void JPH_DistanceConstraintSettings_SetPoint1(JPH_DistanceConstraintSettings* settings, const JPH_RVec3* value)
{
    JPH_ASSERT(settings);
	JPH_ASSERT(value);
    auto joltSettings = reinterpret_cast<JPH::PointConstraintSettings*>(settings);

    joltSettings->mPoint1 = ToJolt(value);
}

void JPH_DistanceConstraintSettings_GetPoint2(JPH_DistanceConstraintSettings* settings, JPH_RVec3* result)
{
    JPH_ASSERT(settings);
	JPH_ASSERT(result);
    auto joltSettings = reinterpret_cast<JPH::PointConstraintSettings*>(settings);

    auto joltVector = joltSettings->mPoint2;
    FromJolt(joltVector, result);
}

void JPH_DistanceConstraintSettings_SetPoint2(JPH_DistanceConstraintSettings* settings, const JPH_RVec3* value)
{
    JPH_ASSERT(settings);
	JPH_ASSERT(value);
    auto joltSettings = reinterpret_cast<JPH::PointConstraintSettings*>(settings);

    joltSettings->mPoint2 = ToJolt(value);
}

JPH_DistanceConstraint* JPH_DistanceConstraintSettings_CreateConstraint(JPH_DistanceConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2)
{
    auto joltBody1 = reinterpret_cast<JPH::Body*>(body1);
    auto joltBody2 = reinterpret_cast<JPH::Body*>(body2);
    JPH::TwoBodyConstraint* constraint = reinterpret_cast<JPH::DistanceConstraintSettings*>(settings)->Create(*joltBody1, *joltBody2);
    constraint->AddRef();

    return reinterpret_cast<JPH_DistanceConstraint*>(static_cast<JPH::DistanceConstraint*>(constraint));
}

/* JPH_HingeConstraintSettings */

JPH_HingeConstraintSettings* JPH_HingeConstraintSettings_Create(void)
{
    auto settings = new JPH::HingeConstraintSettings();
    settings->AddRef();

    return reinterpret_cast<JPH_HingeConstraintSettings*>(settings);
}

void JPH_HingeConstraintSettings_GetPoint1(JPH_HingeConstraintSettings* settings, JPH_RVec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);

    auto joltVector = joltSettings->mPoint1;
    FromJolt(joltVector, result);
}

void JPH_HingeConstraintSettings_SetPoint1(JPH_HingeConstraintSettings* settings, const JPH_RVec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);

    joltSettings->mPoint1 = ToJolt(value);
}

void JPH_HingeConstraintSettings_GetPoint2(JPH_HingeConstraintSettings* settings, JPH_RVec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);
    auto joltVector = joltSettings->mPoint2;
    FromJolt(joltVector, result);
}

void JPH_HingeConstraintSettings_SetPoint2(JPH_HingeConstraintSettings* settings, const JPH_RVec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);
    joltSettings->mPoint2 = ToJolt(value);
}

void JPH_HingeConstraintSettings_SetHingeAxis1(JPH_HingeConstraintSettings* settings, const JPH_Vec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);
    joltSettings->mHingeAxis1 = ToJolt(value);
}

void JPH_HingeConstraintSettings_GetHingeAxis1(JPH_HingeConstraintSettings* settings, JPH_Vec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);
    FromJolt(joltSettings->mHingeAxis1, result);
}

void JPH_HingeConstraintSettings_SetNormalAxis1(JPH_HingeConstraintSettings* settings, const JPH_Vec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);
    joltSettings->mNormalAxis1 = ToJolt(value);
}

void JPH_HingeConstraintSettings_GetNormalAxis1(JPH_HingeConstraintSettings* settings, JPH_Vec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);
    FromJolt(joltSettings->mNormalAxis1, result);
}

void JPH_HingeConstraintSettings_SetHingeAxis2(JPH_HingeConstraintSettings* settings, const JPH_Vec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);
    joltSettings->mHingeAxis2 = ToJolt(value);
}

void JPH_HingeConstraintSettings_GetHingeAxis2(JPH_HingeConstraintSettings* settings, JPH_Vec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);
    FromJolt(joltSettings->mHingeAxis2, result);
}

void JPH_HingeConstraintSettings_SetNormalAxis2(JPH_HingeConstraintSettings* settings, const JPH_Vec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);
    joltSettings->mNormalAxis2 = ToJolt(value);
}

void JPH_HingeConstraintSettings_GetNormalAxis2(JPH_HingeConstraintSettings* settings, JPH_Vec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::HingeConstraintSettings*>(settings);
    FromJolt(joltSettings->mNormalAxis2, result);
}

JPH_HingeConstraint* JPH_HingeConstraintSettings_CreateConstraint(JPH_HingeConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2)
{
    auto joltBody1 = reinterpret_cast<JPH::Body*>(body1);
    auto joltBody2 = reinterpret_cast<JPH::Body*>(body2);
    JPH::TwoBodyConstraint* constraint = reinterpret_cast<JPH::HingeConstraintSettings*>(settings)->Create(*joltBody1, *joltBody2);
    constraint->AddRef();

    return reinterpret_cast<JPH_HingeConstraint*>(static_cast<JPH::HingeConstraint*>(constraint));
}

JPH_HingeConstraintSettings* JPH_HingeConstraint_GetSettings(JPH_HingeConstraint* constraint)
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
    settings->AddRef();

    return reinterpret_cast<JPH_SliderConstraintSettings*>(settings);
}

void JPH_SliderConstraintSettings_SetSliderAxis(JPH_SliderConstraintSettings* settings, const JPH_Vec3* axis)
{
    JPH_ASSERT(settings);

    auto joltSettings = reinterpret_cast<JPH::SliderConstraintSettings*>(settings);
    joltSettings->SetSliderAxis(ToJolt(axis));
}

void JPH_SliderConstraintSettings_GetSliderAxis(JPH_SliderConstraintSettings* settings, JPH_Vec3* result)
{
	JPH_ASSERT(settings);

    auto joltSettings = reinterpret_cast<JPH::SliderConstraintSettings*>(settings);
    FromJolt(joltSettings->mSliderAxis1, result);
}

JPH_SliderConstraintSettings* JPH_SliderConstraint_GetSettings(JPH_SliderConstraint* constraint)
{
    auto joltConstraint = reinterpret_cast<JPH::SliderConstraint*>(constraint);
    auto joltSettings = joltConstraint->GetConstraintSettings().GetPtr();
    return reinterpret_cast<JPH_SliderConstraintSettings*>(joltSettings);
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

/* JPH_SwingTwistConstraintSettings */
JPH_SwingTwistConstraintSettings* JPH_SwingTwistConstraintSettings_Create(void)
{
	auto settings = new JPH::SwingTwistConstraintSettings();
    settings->AddRef();

    return reinterpret_cast<JPH_SwingTwistConstraintSettings*>(settings);
}

JPH_SwingTwistConstraint* JPH_SwingTwistConstraintSettings_CreateConstraint(JPH_SwingTwistConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2)
{
	auto joltBody1 = reinterpret_cast<JPH::Body*>(body1);
    auto joltBody2 = reinterpret_cast<JPH::Body*>(body2);
    JPH::TwoBodyConstraint* constraint = reinterpret_cast<JPH::SwingTwistConstraintSettings*>(settings)->Create(*joltBody1, *joltBody2);
    constraint->AddRef();

    return reinterpret_cast<JPH_SwingTwistConstraint*>(static_cast<JPH::SwingTwistConstraint*>(constraint));
}

/* JPH_SwingTwistConstraint */
float JPH_SwingTwistConstraint_GetNormalHalfConeAngle(JPH_SwingTwistConstraint* constraint)
{
	return reinterpret_cast<JPH::SwingTwistConstraint*>(constraint)->GetNormalHalfConeAngle();
}

/* JPH_SixDOFConstraintSettings */
JPH_SixDOFConstraintSettings* JPH_SixDOFConstraintSettings_Create(void)
{
	auto settings = new JPH::SixDOFConstraintSettings();
    settings->AddRef();

    return reinterpret_cast<JPH_SixDOFConstraintSettings*>(settings);
}

JPH_SixDOFConstraint* JPH_SixDOFConstraintSettings_CreateConstraint(JPH_SixDOFConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2)
{
	auto joltBody1 = reinterpret_cast<JPH::Body*>(body1);
    auto joltBody2 = reinterpret_cast<JPH::Body*>(body2);
    JPH::TwoBodyConstraint* constraint = reinterpret_cast<JPH::SixDOFConstraintSettings*>(settings)->Create(*joltBody1, *joltBody2);
    constraint->AddRef();

    return reinterpret_cast<JPH_SixDOFConstraint*>(static_cast<JPH::SixDOFConstraint*>(constraint));
}

/* JPH_SixDOFConstraint */
float JPH_SixDOFConstraint_GetLimitsMin(JPH_SixDOFConstraint* constraint, JPH_SixDOFConstraintAxis axis)
{
	return reinterpret_cast<JPH::SixDOFConstraint*>(constraint)->GetLimitsMin(static_cast<JPH::SixDOFConstraint::EAxis>(axis));
}

float JPH_SixDOFConstraint_GetLimitsMax(JPH_SixDOFConstraint* constraint, JPH_SixDOFConstraintAxis axis)
{
	return reinterpret_cast<JPH::SixDOFConstraint*>(constraint)->GetLimitsMax(static_cast<JPH::SixDOFConstraint::EAxis>(axis));
}

JPH_SliderConstraint* JPH_SliderConstraintSettings_CreateConstraint(JPH_SliderConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2)
{
    auto joltBody1 = reinterpret_cast<JPH::Body*>(body1);
    auto joltBody2 = reinterpret_cast<JPH::Body*>(body2);
    JPH::TwoBodyConstraint* constraint = reinterpret_cast<JPH::SliderConstraintSettings*>(settings)->Create(*joltBody1, *joltBody2);
    constraint->AddRef();

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
    settings->AddRef();

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
    FromJolt(joltVector, result);
}

void JPH_PointConstraintSettings_SetPoint1(JPH_PointConstraintSettings* settings, const JPH_RVec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::PointConstraintSettings*>(settings);

    joltSettings->mPoint1 = ToJolt(value);
}

void JPH_PointConstraintSettings_GetPoint2(JPH_PointConstraintSettings* settings, JPH_RVec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::PointConstraintSettings*>(settings);

    auto joltVector = joltSettings->mPoint2;
    FromJolt(joltVector, result);
}

void JPH_PointConstraintSettings_SetPoint2(JPH_PointConstraintSettings* settings, const JPH_RVec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::PointConstraintSettings*>(settings);

    joltSettings->mPoint2 = ToJolt(value);
}

JPH_PointConstraint* JPH_PointConstraintSettings_CreateConstraint(JPH_PointConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2)
{
    JPH_ASSERT(settings);
    JPH_ASSERT(body1);
    JPH_ASSERT(body2);
    auto joltBody1 = reinterpret_cast<JPH::Body*>(body1);
    auto joltBody2 = reinterpret_cast<JPH::Body*>(body2);
    JPH::TwoBodyConstraint* constraint = reinterpret_cast<JPH::PointConstraintSettings*>(settings)->Create(*joltBody1, *joltBody2);
    constraint->AddRef();

    return reinterpret_cast<JPH_PointConstraint*>(static_cast<JPH::PointConstraint*>(constraint));
}

/* JPH_PointConstraint */
void JPH_PointConstraint_SetPoint1(JPH_PointConstraint* constraint, JPH_ConstraintSpace space, JPH_RVec3* value)
{
    JPH_ASSERT(constraint);
    auto joltConstraint = reinterpret_cast<JPH::PointConstraint*>(constraint);
    joltConstraint->SetPoint1(static_cast<JPH::EConstraintSpace>(space), ToJolt(value));
}

void JPH_PointConstraint_SetPoint2(JPH_PointConstraint* constraint, JPH_ConstraintSpace space, JPH_RVec3* value)
{
    JPH_ASSERT(constraint);
    auto joltConstraint = reinterpret_cast<JPH::PointConstraint*>(constraint);
    joltConstraint->SetPoint2(static_cast<JPH::EConstraintSpace>(space), ToJolt(value));
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

    return reinterpret_cast<const JPH_NarrowPhaseQuery*>(&system->physicsSystem->GetNarrowPhaseQuery());
}
const JPH_NarrowPhaseQuery* JPC_PhysicsSystem_GetNarrowPhaseQueryNoLock(const JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    return reinterpret_cast<const JPH_NarrowPhaseQuery*>(&system->physicsSystem->GetNarrowPhaseQueryNoLock());
}

void JPH_PhysicsSystem_SetContactListener(JPH_PhysicsSystem* system, JPH_ContactListener* listener)
{
    JPH_ASSERT(system);

    auto joltListener = reinterpret_cast<JPH::ContactListener*>(listener);
    system->physicsSystem->SetContactListener(joltListener);
}

void JPH_PhysicsSystem_SetBodyActivationListener(JPH_PhysicsSystem* system, JPH_BodyActivationListener* listener)
{
    JPH_ASSERT(system);

    auto joltListener = reinterpret_cast<JPH::BodyActivationListener*>(listener);
    system->physicsSystem->SetBodyActivationListener(joltListener);
}

uint32_t JPH_PhysicsSystem_GetNumBodies(const JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    return system->physicsSystem->GetNumBodies();
}

uint32_t JPH_PhysicsSystem_GetNumActiveBodies(const JPH_PhysicsSystem* system, JPH_BodyType type)
{
    JPH_ASSERT(system);

    return system->physicsSystem->GetNumActiveBodies(static_cast<JPH::EBodyType>(type));
}

uint32_t JPH_PhysicsSystem_GetMaxBodies(const JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    return system->physicsSystem->GetMaxBodies();
}

uint32_t JPH_PhysicsSystem_GetNumConstraints(const JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

	return (uint32_t) system->physicsSystem->GetConstraints().size();
}

void JPH_PhysicsSystem_SetGravity(JPH_PhysicsSystem* system, const JPH_Vec3* value)
{
    JPH_ASSERT(system);

    system->physicsSystem->SetGravity(ToJolt(value));
}

void JPH_PhysicsSystem_GetGravity(JPH_PhysicsSystem* system, JPH_Vec3* result)
{
    JPH_ASSERT(system);

    auto joltVector = system->physicsSystem->GetGravity();
    FromJolt(joltVector, result);
}

void JPH_PhysicsSystem_AddConstraint(JPH_PhysicsSystem* system, JPH_Constraint* constraint)
{
    JPH_ASSERT(system);
    JPH_ASSERT(constraint);

    auto joltConstraint = reinterpret_cast<JPH::Constraint*>(constraint);
    system->physicsSystem->AddConstraint(joltConstraint);
}

void JPH_PhysicsSystem_RemoveConstraint(JPH_PhysicsSystem* system, JPH_Constraint* constraint)
{
    JPH_ASSERT(system);
    JPH_ASSERT(constraint);

    auto joltConstraint = reinterpret_cast<JPH::Constraint*>(constraint);
    system->physicsSystem->RemoveConstraint(joltConstraint);
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

    system->physicsSystem->AddConstraints(joltConstraints.data(), (int)count);
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

    system->physicsSystem->RemoveConstraints(joltConstraints.data(), (int)count);
}

JPH_CAPI void JPH_PhysicsSystem_GetBodies(const JPH_PhysicsSystem* system, JPH_BodyID* ids, uint32_t count)
{
    JPH_ASSERT(system);
    JPH_ASSERT(ids);
	JPH_ASSERT(count <= JPH_PhysicsSystem_GetNumBodies(system));

	JPH::BodyIDVector bodies;
	system->physicsSystem->GetBodies(bodies);

	for (uint32_t i = 0; i < count; i++) {
		ids[i] = bodies[i].GetIndexAndSequenceNumber();
	}
}

JPH_CAPI void JPH_PhysicsSystem_GetConstraints(const JPH_PhysicsSystem* system, const JPH_Constraint** constraints, uint32_t count)
{
    JPH_ASSERT(system);
    JPH_ASSERT(constraints);

	JPH::Constraints list = system->physicsSystem->GetConstraints();

	for (uint32_t i = 0; i < count && i < list.size(); i++) {
		constraints[i] = reinterpret_cast<JPH_Constraint*>(list[i].GetPtr());
	}
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
    joltBodyInterface->SetLinearVelocity(JPH::BodyID(bodyID), ToJolt(velocity));
}

void JPH_BodyInterface_GetLinearVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyID, JPH_Vec3* velocity)
{
    JPH_ASSERT(interface);

    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    auto joltVector = joltBodyInterface->GetLinearVelocity(JPH::BodyID(bodyID));
    FromJolt(joltVector, velocity);
}

void JPH_BodyInterface_GetCenterOfMassPosition(JPH_BodyInterface* interface, JPH_BodyID bodyID, JPH_RVec3* position)
{
    JPH_ASSERT(interface);

    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
    auto joltVector = joltBodyInterface->GetCenterOfMassPosition(JPH::BodyID(bodyID));
    FromJolt(joltVector, position);
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

    joltBodyInterface->SetPosition(JPH::BodyID(bodyId), ToJolt(position), static_cast<JPH::EActivation>(activationMode));
}

void JPH_BodyInterface_GetPosition(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* result)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    FromJolt(joltBodyInterface->GetPosition(JPH::BodyID(bodyId)), result);
}

void JPH_BodyInterface_SetRotation(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Quat* rotation, JPH_Activation activationMode)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->SetRotation(JPH::BodyID(bodyId), ToJolt(rotation), static_cast<JPH::EActivation>(activationMode));
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

    joltBodyInterface->SetPositionAndRotation(JPH::BodyID(bodyId), ToJolt(position), ToJolt(rotation), static_cast<JPH::EActivation>(activationMode));
}

void JPH_BodyInterface_SetPositionAndRotationWhenChanged(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* position, JPH_Quat* rotation, JPH_Activation activationMode)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->SetPositionAndRotationWhenChanged(JPH::BodyID(bodyId), ToJolt(position), ToJolt(rotation), static_cast<JPH::EActivation>(activationMode));
}

void JPH_BodyInterface_SetPositionRotationAndVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* position, JPH_Quat* rotation, JPH_Vec3* linearVelocity, JPH_Vec3* angularVelocity)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->SetPositionRotationAndVelocity(JPH::BodyID(bodyId), ToJolt(position), ToJolt(rotation), ToJolt(linearVelocity), ToJolt(angularVelocity));
}

const JPH_Shape* JPH_BodyInterface_GetShape(JPH_BodyInterface* interface, JPH_BodyID bodyId)
{
	JPH_ASSERT(interface);
	auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
	const JPH::Shape* shape = joltBodyInterface->GetShape(JPH::BodyID(bodyId)).GetPtr();
	return reinterpret_cast<const JPH_Shape*>(shape);
}

void JPH_BodyInterface_SetShape(JPH_BodyInterface* interface, JPH_BodyID bodyId, const JPH_Shape* shape, JPH_Bool32 updateMassProperties, JPH_Activation activationMode)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    auto jphShape = reinterpret_cast<const JPH::Shape*>(shape);

    // !! is to make ms compiler happy.
    joltBodyInterface->SetShape(JPH::BodyID(bodyId), jphShape, !!updateMassProperties, static_cast<JPH::EActivation>(activationMode));
}

void JPH_BodyInterface_NotifyShapeChanged(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* previousCenterOfMass, JPH_Bool32 updateMassProperties, JPH_Activation activationMode)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->NotifyShapeChanged(JPH::BodyID(bodyId), ToJolt(previousCenterOfMass), !!updateMassProperties, static_cast<JPH::EActivation>(activationMode));
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

    joltBodyInterface->MoveKinematic(JPH::BodyID(bodyId), ToJolt(targetPosition), ToJolt(targetRotation), deltaTime);
}

void JPH_BodyInterface_SetLinearAndAngularVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* linearVelocity, JPH_Vec3* angularVelocity)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->SetLinearAndAngularVelocity(JPH::BodyID(bodyId), ToJolt(linearVelocity), ToJolt(angularVelocity));
}

void JPH_BodyInterface_GetLinearAndAngularVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* linearVelocity, JPH_Vec3* angularVelocity)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    JPH::Vec3 linear, angular;
    joltBodyInterface->GetLinearAndAngularVelocity(JPH::BodyID(bodyId), linear, angular);
    FromJolt(linear, linearVelocity);
    FromJolt(angular, angularVelocity);
}

void JPH_BodyInterface_AddLinearVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* linearVelocity)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->AddLinearVelocity(JPH::BodyID(bodyId), ToJolt(linearVelocity));
}

void JPH_BodyInterface_AddLinearAndAngularVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* linearVelocity, JPH_Vec3* angularVelocity)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->AddLinearAndAngularVelocity(JPH::BodyID(bodyId), ToJolt(linearVelocity), ToJolt(angularVelocity));
}

void JPH_BodyInterface_SetAngularVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* angularVelocity)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->SetAngularVelocity(JPH::BodyID(bodyId), ToJolt(angularVelocity));
}

void JPH_BodyInterface_GetAngularVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* angularVelocity)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    auto result = joltBodyInterface->GetAngularVelocity(JPH::BodyID(bodyId));
    FromJolt(result, angularVelocity);
}

void JPH_BodyInterface_GetPointVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* point, JPH_Vec3* velocity)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    auto result = joltBodyInterface->GetPointVelocity(JPH::BodyID(bodyId), ToJolt(point));
    FromJolt(result, velocity);
}

void JPH_BodyInterface_AddForce(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* force)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->AddForce(JPH::BodyID(bodyId), ToJolt(force));
}

void JPH_BodyInterface_AddForce2(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* force, JPH_RVec3* point)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->AddForce(JPH::BodyID(bodyId), ToJolt(force), ToJolt(point));
}

void JPH_BodyInterface_AddTorque(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* torque)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->AddTorque(JPH::BodyID(bodyId), ToJolt(torque));
}

void JPH_BodyInterface_AddForceAndTorque(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* force, JPH_Vec3* torque)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->AddForceAndTorque(JPH::BodyID(bodyId), ToJolt(force), ToJolt(torque));
}

void JPH_BodyInterface_AddImpulse(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* impulse)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->AddImpulse(JPH::BodyID(bodyId), ToJolt(impulse));
}

void JPH_BodyInterface_AddImpulse2(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* impulse, JPH_RVec3* point)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->AddImpulse(JPH::BodyID(bodyId), ToJolt(impulse), ToJolt(point));
}

void JPH_BodyInterface_AddAngularImpulse(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* angularImpulse)
{
    JPH_ASSERT(interface);
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

    joltBodyInterface->AddAngularImpulse(JPH::BodyID(bodyId), ToJolt(angularImpulse));
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
    JPH::RRayCast ray(ToJolt(origin), ToJolt(direction));
    const JPH::BroadPhaseLayerFilter broad_phase_layer_filter{};
    const JPH::ObjectLayerFilter object_layer_filter{};
    const JPH::BodyFilter body_filter{};

    ClosestHitCollisionCollector<CastRayCollector> collector;
    RayCastSettings ray_settings;
    joltQuery->CastRay(
        ray,
        ray_settings,
        collector,
        broadPhaseLayerFilter ? *static_cast<const JPH::BroadPhaseLayerFilter*>(broadPhaseLayerFilter) : broad_phase_layer_filter,
        objectLayerFilter ? *static_cast<const JPH::ObjectLayerFilter*>(objectLayerFilter) : object_layer_filter,
        bodyFilter ? *static_cast<const JPH::BodyFilter*>(bodyFilter) : body_filter
    );

    if (collector.HadHit())
    {
		hit->fraction = collector.mHit.mFraction;
		hit->bodyID = collector.mHit.mBodyID.GetIndexAndSequenceNumber();
		hit->subShapeID2 = collector.mHit.mSubShapeID2.GetValue();
	}

    return collector.HadHit();
}

JPH_AllHit_CastRayCollector* JPH_AllHit_CastRayCollector_Create(void)
{
    auto joltCollector = new AllHitCollisionCollector<CastRayCollector>();
    return reinterpret_cast<JPH_AllHit_CastRayCollector*>(joltCollector);
}

void JPH_AllHit_CastRayCollector_Destroy(JPH_AllHit_CastRayCollector* collector)
{
    if (collector)
    {
        delete reinterpret_cast<AllHitCollisionCollector<CastRayCollector>*>(collector);
    }
}

void JPH_AllHit_CastRayCollector_Reset(JPH_AllHit_CastRayCollector* collector)
{
    auto joltCollector = reinterpret_cast<AllHitCollisionCollector<CastRayCollector>*>(collector);
    joltCollector->Reset();
}

JPH_RayCastResult* JPH_AllHit_CastRayCollector_GetHits(JPH_AllHit_CastRayCollector* collector, size_t * size)
{
    auto joltCollector = reinterpret_cast<AllHitCollisionCollector<CastRayCollector>*>(collector);
    *size = joltCollector->mHits.size();
    return reinterpret_cast<JPH_RayCastResult*>(joltCollector->mHits.data());
}

JPH_Bool32 JPH_NarrowPhaseQuery_CastRayAll(const JPH_NarrowPhaseQuery* query,
    const JPH_RVec3* origin, const JPH_Vec3* direction,
    JPH_AllHit_CastRayCollector* hit_collector,
    const void* broadPhaseLayerFilter, // Can be NULL (no filter)
    const void* objectLayerFilter, // Can be NULL (no filter)
    const void* bodyFilter) // Can be NULL (no filter)
{
    JPH_ASSERT(query && origin && direction && hit_collector);
    auto joltQuery = reinterpret_cast<const JPH::NarrowPhaseQuery*>(query);
    JPH::RRayCast ray(ToJolt(origin), ToJolt(direction));
    const JPH::BroadPhaseLayerFilter broad_phase_layer_filter{};
    const JPH::ObjectLayerFilter object_layer_filter{};
    const JPH::BodyFilter body_filter{};
    AllHitCollisionCollector<CastRayCollector>& joltCollector = *reinterpret_cast<AllHitCollisionCollector<CastRayCollector>*>(hit_collector);
    RayCastSettings ray_settings;
    joltQuery->CastRay(
        ray,
        ray_settings,
        joltCollector,
        broadPhaseLayerFilter ? *static_cast<const JPH::BroadPhaseLayerFilter*>(broadPhaseLayerFilter) : broad_phase_layer_filter,
        objectLayerFilter ? *static_cast<const JPH::ObjectLayerFilter*>(objectLayerFilter) : object_layer_filter,
        bodyFilter ? *static_cast<const JPH::BodyFilter*>(bodyFilter) : body_filter
    );
    joltCollector.Sort();
    return !joltCollector.mHits.empty();
}

JPH_AllHit_CastShapeCollector* JPH_AllHit_CastShapeCollector_Create(void)
{
    auto joltCollector = new AllHitCollisionCollector<CastShapeCollector>();
    return reinterpret_cast<JPH_AllHit_CastShapeCollector*>(joltCollector);
}

void JPH_AllHit_CastShapeCollector_Destroy(JPH_AllHit_CastShapeCollector* collector)
{
    if (collector)
    {
        delete reinterpret_cast<AllHitCollisionCollector<CastShapeCollector>*>(collector);
    }
}

void JPH_AllHit_CastShapeCollector_Reset(JPH_AllHit_CastShapeCollector* collector)
{
    auto joltCollector = reinterpret_cast<AllHitCollisionCollector<CastShapeCollector>*>(collector);
    joltCollector->Reset();
}

JPH_ShapeCastResult* JPH_AllHit_CastShapeCollector_GetHits(JPH_AllHit_CastShapeCollector* collector, size_t * size)
{
    auto joltCollector = reinterpret_cast<AllHitCollisionCollector<CastShapeCollector>*>(collector);
    *size = joltCollector->mHits.size();
    return reinterpret_cast<JPH_ShapeCastResult*>(joltCollector->mHits.data());
}

JPH_BodyID JPH_AllHit_CastShapeCollector_GetBodyID2(JPH_AllHit_CastShapeCollector* collector, unsigned index)
{
    auto joltCollector = reinterpret_cast<AllHitCollisionCollector<CastShapeCollector>*>(collector);
    JPH_ASSERT(index < joltCollector->mHits.size());
    return joltCollector->mHits[index].mBodyID2.GetIndexAndSequenceNumber();
}

JPH_BodyID JPH_AllHit_CastShapeCollector_GetSubShapeID2(JPH_AllHit_CastShapeCollector* collector, unsigned index)
{
    auto joltCollector = reinterpret_cast<AllHitCollisionCollector<CastShapeCollector>*>(collector);
    JPH_ASSERT(index < joltCollector->mHits.size());
    return joltCollector->mHits[index].mSubShapeID2.GetValue();
}

JPH_Bool32 JPH_NarrowPhaseQuery_CastShape(const JPH_NarrowPhaseQuery* query,
    const JPH_Shape* shape,
    const JPH_RMatrix4x4* worldTransform, const JPH_Vec3* direction,
    JPH_RVec3* baseOffset,
    JPH_AllHit_CastShapeCollector* hit_collector)
{
    JPH_ASSERT(query && worldTransform && direction && hit_collector);
    auto joltShape = reinterpret_cast<const JPH::Shape*>(shape);

    RShapeCast shape_cast = RShapeCast::sFromWorldTransform(
        joltShape,
        JPH::Vec3(1.f, 1.f, 1.f), // scale can be embedded in worldTransform
        ToJolt(*worldTransform),
        ToJolt(direction));

    ShapeCastSettings settings;
    settings.mActiveEdgeMode = EActiveEdgeMode::CollideWithAll;
    settings.mBackFaceModeTriangles = EBackFaceMode::CollideWithBackFaces;
    settings.mBackFaceModeConvex = EBackFaceMode::CollideWithBackFaces;

    AllHitCollisionCollector<CastShapeCollector>& joltCollector = *reinterpret_cast<AllHitCollisionCollector<CastShapeCollector>*>(hit_collector);

    auto joltQuery = reinterpret_cast<const JPH::NarrowPhaseQuery*>(query);
    auto joltBaseOffset = ToJolt(baseOffset);

    joltQuery->CastShape(shape_cast, settings, joltBaseOffset, joltCollector);
    return joltCollector.HadHit();
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
    FromJolt(bounds.mMin, &result->min);
	FromJolt(bounds.mMax, &result->max);
}

void JPH_Body_GetWorldSpaceSurfaceNormal(const JPH_Body* body, JPH_SubShapeID subShapeID, const JPH_RVec3* position, JPH_Vec3* normal)
{
    auto joltBody = reinterpret_cast<const JPH::Body*>(body);
    auto joltSubShapeID = JPH::SubShapeID();
    joltSubShapeID.SetValue(subShapeID);
    Vec3 joltNormal = joltBody->GetWorldSpaceSurfaceNormal(joltSubShapeID, ToJolt(position));
    FromJolt(joltNormal, normal);
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

void JPH_Body_SetCollideKinematicVsNonDynamic(JPH_Body* body, JPH_Bool32 value)
{
	reinterpret_cast<JPH::Body*>(body)->SetCollideKinematicVsNonDynamic(!!value);
}

JPH_Bool32 JPH_Body_GetCollideKinematicVsNonDynamic(const JPH_Body* body)
{
	return reinterpret_cast<const JPH::Body*>(body)->GetCollideKinematicVsNonDynamic();
}

void JPH_Body_SetUseManifoldReduction(JPH_Body* body, JPH_Bool32 value)
{
	reinterpret_cast<JPH::Body*>(body)->SetUseManifoldReduction(!!value);
}

JPH_Bool32 JPH_Body_GetUseManifoldReduction(const JPH_Body* body)
{
	return reinterpret_cast<const JPH::Body*>(body)->GetUseManifoldReduction();
}

JPH_Bool32 JPH_Body_GetUseManifoldReductionWithBody(const JPH_Body* body, const JPH_Body* other)
{
	return reinterpret_cast<const JPH::Body*>(body)->GetUseManifoldReductionWithBody(*reinterpret_cast<const JPH::Body*>(other));
}

void JPH_Body_SetApplyGyroscopicForce(JPH_Body* body, JPH_Bool32 value)
{
	reinterpret_cast<JPH::Body*>(body)->SetApplyGyroscopicForce(!!value);
}

JPH_Bool32 JPH_Body_GetApplyGyroscopicForce(const JPH_Body* body)
{
	return reinterpret_cast<const JPH::Body*>(body)->GetApplyGyroscopicForce();
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

void JPH_Body_ResetSleepTimer(JPH_Body* body)
{
	reinterpret_cast<JPH::Body*>(body)->ResetSleepTimer();
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
    FromJolt(joltVector, velocity);
}

void JPH_Body_SetLinearVelocity(JPH_Body* body, const JPH_Vec3* velocity)
{
    reinterpret_cast<JPH::Body*>(body)->SetLinearVelocity(ToJolt(velocity));
}

void JPH_Body_GetAngularVelocity(JPH_Body* body, JPH_Vec3* velocity)
{
    auto joltVector = reinterpret_cast<JPH::Body*>(body)->GetAngularVelocity();
    FromJolt(joltVector, velocity);
}

void JPH_Body_SetAngularVelocity(JPH_Body* body, const JPH_Vec3* velocity)
{
    reinterpret_cast<JPH::Body*>(body)->SetAngularVelocity(ToJolt(velocity));
}

void JPH_Body_AddForce(JPH_Body* body, const JPH_Vec3* force)
{
    reinterpret_cast<JPH::Body*>(body)->AddForce(ToJolt(force));
}

void JPH_Body_AddForceAtPosition(JPH_Body* body, const JPH_Vec3* force, const JPH_RVec3* position)
{
    reinterpret_cast<JPH::Body*>(body)->AddForce(ToJolt(force), ToJolt(position));
}

void JPH_Body_AddTorque(JPH_Body* body, const JPH_Vec3* force)
{
    reinterpret_cast<JPH::Body*>(body)->AddTorque(ToJolt(force));
}

void JPH_Body_GetAccumulatedForce(JPH_Body* body, JPH_Vec3* force)
{
    auto joltVector = reinterpret_cast<JPH::Body*>(body)->GetAccumulatedForce();
    FromJolt(joltVector, force);
}

void JPH_Body_GetAccumulatedTorque(JPH_Body* body, JPH_Vec3* force)
{
    auto joltVector = reinterpret_cast<JPH::Body*>(body)->GetAccumulatedTorque();
    FromJolt(joltVector, force);
}

void JPH_Body_AddImpulse(JPH_Body* body, const JPH_Vec3* impulse)
{
    reinterpret_cast<JPH::Body*>(body)->AddImpulse(ToJolt(impulse));
}

void JPH_Body_AddImpulseAtPosition(JPH_Body* body, const JPH_Vec3* impulse, const JPH_RVec3* position)
{
    reinterpret_cast<JPH::Body*>(body)->AddImpulse(ToJolt(impulse), ToJolt(position));
}

void JPH_Body_AddAngularImpulse(JPH_Body* body, const JPH_Vec3* angularImpulse)
{
    reinterpret_cast<JPH::Body*>(body)->AddAngularImpulse(ToJolt(angularImpulse));
}

void JPH_Body_GetPosition(const JPH_Body* body, JPH_RVec3* result)
{
    auto joltVector = reinterpret_cast<const JPH::Body*>(body)->GetPosition();
    FromJolt(joltVector, result);
}

void JPH_Body_GetRotation(const JPH_Body* body, JPH_Quat* result)
{
    auto joltQuat = reinterpret_cast<const JPH::Body*>(body)->GetRotation();
    FromJolt(joltQuat, result);
}

void JPH_Body_GetCenterOfMassPosition(const JPH_Body* body, JPH_RVec3* result)
{
    auto joltVector = reinterpret_cast<const JPH::Body*>(body)->GetCenterOfMassPosition();
    FromJolt(joltVector, result);
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
        FromJolt(inBaseOffset, &baseOffset);

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

JPH_ContactListener* JPH_ContactListener_Create(void)
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

JPH_BodyActivationListener* JPH_BodyActivationListener_Create(void)
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

/* CharacterBaseSettings */
void JPH_CharacterBaseSettings_Destroy(JPH_CharacterBaseSettings* settings)
{
    if (settings)
    {
        auto jolt_settings = reinterpret_cast<JPH::CharacterBaseSettings*>(settings);
        jolt_settings->Release();
    }
}

void JPH_CharacterBaseSettings_SetSupportingVolume(JPH_CharacterBaseSettings* settings, const JPH_Vec3* normal, float constant)
{
    auto jolt_settings = reinterpret_cast<JPH::CharacterBaseSettings*>(settings);

    jolt_settings->mSupportingVolume = JPH::Plane(ToJolt(normal), constant);
}

void JPH_CharacterBaseSettings_SetMaxSlopeAngle(JPH_CharacterBaseSettings* settings, float maxSlopeAngle)
{
    auto jolt_settings = reinterpret_cast<JPH::CharacterBaseSettings*>(settings);
    jolt_settings->mMaxSlopeAngle = DegreesToRadians(maxSlopeAngle);
}

void JPH_CharacterBaseSettings_SetShape(JPH_CharacterBaseSettings* settings, JPH_Shape* shape)
{
    auto jolt_settings = reinterpret_cast<JPH::CharacterBaseSettings*>(settings);
    auto jolt_shape = reinterpret_cast<JPH::Shape*>(shape);

    jolt_settings->mShape = jolt_shape;
}

/* CharacterBase */
void JPH_CharacterBase_Destroy(JPH_CharacterBase* character)
{
    if (character)
    {
        auto jolt_character = reinterpret_cast<JPH::CharacterBase*>(character);
        jolt_character->Release();
    }
}

JPH_GroundState JPH_CharacterBase_GetGroundState(JPH_CharacterBase* character)
{
    auto jolt_character = reinterpret_cast<JPH::CharacterBase*>(character);
    return static_cast<JPH_GroundState>(jolt_character->GetGroundState());
}

JPH_Bool32 JPH_CharacterBase_IsSupported(JPH_CharacterBase* character)
{
    auto jolt_character = reinterpret_cast<JPH::CharacterBase*>(character);
    return jolt_character->IsSupported();
}

void JPH_CharacterBase_GetGroundPosition(JPH_CharacterBase* character, JPH_RVec3* position)
{
    auto jolt_character = reinterpret_cast<JPH::CharacterBase*>(character);
    auto jolt_vector = jolt_character->GetGroundPosition();
    FromJolt(jolt_vector, position);
}

void JPH_CharacterBase_GetGroundNormal(JPH_CharacterBase* character, JPH_Vec3* normal)
{
    auto jolt_character = reinterpret_cast<JPH::CharacterBase*>(character);
    auto jolt_vector = jolt_character->GetGroundNormal();
    FromJolt(jolt_vector, normal);
}

void JPH_CharacterBase_GetGroundVelocity(JPH_CharacterBase* character, JPH_Vec3* velocity)
{
    auto jolt_character = reinterpret_cast<JPH::CharacterBase*>(character);
    auto jolt_vector = jolt_character->GetGroundVelocity();
    FromJolt(jolt_vector, velocity);
}

JPH_BodyID JPH_CharacterBase_GetGroundBodyId(JPH_CharacterBase* character)
{
    auto jolt_character = reinterpret_cast<JPH::CharacterBase*>(character);
    return jolt_character->GetGroundBodyID().GetIndexAndSequenceNumber();
}

JPH_SubShapeID JPH_CharacterBase_GetGroundSubShapeId(JPH_CharacterBase* character)
{
    auto jolt_character = reinterpret_cast<JPH::CharacterBase*>(character);
    return jolt_character->GetGroundSubShapeID().GetValue();
}

/* CharacterVirtualSettings */
JPH_CharacterVirtualSettings* JPH_CharacterVirtualSettings_Create(void)
{
    auto settings = new JPH::CharacterVirtualSettings();
    settings->AddRef();

    return reinterpret_cast<JPH_CharacterVirtualSettings*>(settings);
}

/* CharacterVirtual */
JPH_CharacterVirtual* JPH_CharacterVirtual_Create(JPH_CharacterVirtualSettings* settings,
    const JPH_RVec3* position,
    const JPH_Quat* rotation,
	uint64_t userData,
    JPH_PhysicsSystem* system)
{
    auto jolt_settings = reinterpret_cast<JPH::CharacterVirtualSettings*>(settings);

    auto jolt_character = new JPH::CharacterVirtual(jolt_settings, ToJolt(position), ToJolt(rotation), userData, system->physicsSystem);
    jolt_character->AddRef();

    return reinterpret_cast<JPH_CharacterVirtual*>(jolt_character);
}

void JPH_CharacterVirtual_GetLinearVelocity(JPH_CharacterVirtual* character, JPH_Vec3* velocity)
{
    auto jolt_character = reinterpret_cast<JPH::CharacterVirtual*>(character);
    auto jolt_vector = jolt_character->GetLinearVelocity();
    FromJolt(jolt_vector, velocity);
}

void JPH_CharacterVirtual_SetLinearVelocity(JPH_CharacterVirtual* character, const JPH_Vec3* velocity)
{
    auto jolt_character = reinterpret_cast<JPH::CharacterVirtual*>(character);
    jolt_character->SetLinearVelocity(ToJolt(velocity));
}

void JPH_CharacterVirtual_GetPosition(JPH_CharacterVirtual* character, JPH_RVec3* position)
{
    auto jolt_character = reinterpret_cast<JPH::CharacterVirtual*>(character);
    auto jolt_vector = jolt_character->GetPosition();
    FromJolt(jolt_vector, position);
}

void JPH_CharacterVirtual_SetPosition(JPH_CharacterVirtual* character, const JPH_RVec3* position)
{
    auto jolt_character = reinterpret_cast<JPH::CharacterVirtual*>(character);
    jolt_character->SetPosition(ToJolt(position));
}

void JPH_CharacterVirtual_GetRotation(JPH_CharacterVirtual* character, JPH_Quat* rotation)
{
    auto jolt_character = reinterpret_cast<JPH::CharacterVirtual*>(character);
    auto jolt_quat = jolt_character->GetRotation();
    FromJolt(jolt_quat, rotation);
}

void JPH_CharacterVirtual_SetRotation(JPH_CharacterVirtual* character, const JPH_Quat* rotation)
{
    auto jolt_character = reinterpret_cast<JPH::CharacterVirtual*>(character);
    jolt_character->SetRotation(ToJolt(rotation));
}

void JPH_CharacterVirtual_ExtendedUpdate(JPH_CharacterVirtual* character, float deltaTime,
    const JPH_ExtendedUpdateSettings* settings, JPH_ObjectLayer layer, JPH_PhysicsSystem* system)
{
	JPH_ASSERT(settings);

    auto jolt_character = reinterpret_cast<JPH::CharacterVirtual*>(character);

	// Convert to Jolt
    JPH::CharacterVirtual::ExtendedUpdateSettings jolt_settings = {};
	jolt_settings.mStickToFloorStepDown = ToJolt(&settings->stickToFloorStepDown);
	jolt_settings.mWalkStairsStepUp = ToJolt(&settings->walkStairsStepUp);
	jolt_settings.mWalkStairsMinStepForward = settings->walkStairsMinStepForward;
	jolt_settings.mWalkStairsStepForwardTest = settings->walkStairsStepForwardTest;
	jolt_settings.mWalkStairsCosAngleForwardContact = settings->walkStairsCosAngleForwardContact;
	jolt_settings.mWalkStairsStepDownExtra = ToJolt(&settings->walkStairsStepDownExtra);

    auto jolt_object_layer = static_cast<JPH::ObjectLayer>(layer);

    jolt_character->ExtendedUpdate(deltaTime,
        system->physicsSystem->GetGravity(),
        jolt_settings,
        system->physicsSystem->GetDefaultBroadPhaseLayerFilter(jolt_object_layer),
        system->physicsSystem->GetDefaultLayerFilter(jolt_object_layer),
        {},
        {},
        *s_TempAllocator
    );
}

void JPH_CharacterVirtual_RefreshContacts(JPH_CharacterVirtual* character, JPH_ObjectLayer layer, JPH_PhysicsSystem* system)
{
    auto jolt_character = reinterpret_cast<JPH::CharacterVirtual*>(character);
    auto jolt_object_layer = static_cast<JPH::ObjectLayer>(layer);

    jolt_character->RefreshContacts(
        system->physicsSystem->GetDefaultBroadPhaseLayerFilter(jolt_object_layer),
        system->physicsSystem->GetDefaultLayerFilter(jolt_object_layer),
        {},
        {},
        *s_TempAllocator
    );
}

#ifdef _MSC_VER
#   pragma warning(pop)
#endif
