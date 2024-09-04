// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

#include "joltc.h"

#include <Jolt/Core/Core.h>

JPH_SUPPRESS_WARNING_PUSH
JPH_SUPPRESS_WARNINGS

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
#include "Jolt/Physics/Collision/CollidePointResult.h"
#include "Jolt/Physics/Collision/CollideShape.h"
#include <Jolt/Physics/Collision/CollisionCollectorImpl.h>
#include <Jolt/Physics/Collision/ShapeCast.h>
#include "Jolt/Physics/Collision/Shape/PlaneShape.h"
#include "Jolt/Physics/Collision/Shape/BoxShape.h"
#include "Jolt/Physics/Collision/Shape/SphereShape.h"
#include "Jolt/Physics/Collision/Shape/TriangleShape.h"
#include "Jolt/Physics/Collision/Shape/CapsuleShape.h"
#include "Jolt/Physics/Collision/Shape/TaperedCapsuleShape.h"
#include "Jolt/Physics/Collision/Shape/CylinderShape.h"
#include <Jolt/Physics/Collision/Shape/TaperedCylinderShape.h>
#include "Jolt/Physics/Collision/Shape/ConvexHullShape.h"
#include "Jolt/Physics/Collision/Shape/MeshShape.h"
#include "Jolt/Physics/Collision/Shape/HeightFieldShape.h"
#include "Jolt/Physics/Collision/Shape/StaticCompoundShape.h"
#include "Jolt/Physics/Collision/Shape/MutableCompoundShape.h"
#include "Jolt/Physics/Collision/Shape/DecoratedShape.h"
#include "Jolt/Physics/Collision/Shape/RotatedTranslatedShape.h"
#include "Jolt/Physics/Collision/Shape/OffsetCenterOfMassShape.h"
#include "Jolt/Physics/Body/BodyCreationSettings.h"
#include "Jolt/Physics/Body/BodyActivationListener.h"
#include "Jolt/Physics/SoftBody/SoftBodyCreationSettings.h"
#include "Jolt/Physics/Collision/RayCast.h"
#include "Jolt/Physics/Collision/BroadPhase/BroadPhaseQuery.h"
#include "Jolt/Physics/Collision/NarrowPhaseQuery.h"
#include "Jolt/Physics/Constraints/SpringSettings.h"
#include "Jolt/Physics/Constraints/FixedConstraint.h"
#include "Jolt/Physics/Constraints/PointConstraint.h"
#include "Jolt/Physics/Constraints/DistanceConstraint.h"
#include "Jolt/Physics/Constraints/HingeConstraint.h"
#include "Jolt/Physics/Constraints/SliderConstraint.h"
#include "Jolt/Physics/Constraints/ConeConstraint.h"
#include "Jolt/Physics/Constraints/SwingTwistConstraint.h"
#include "Jolt/Physics/Constraints/SixDOFConstraint.h"
#include "Jolt/Physics/Character/CharacterBase.h"
#include "Jolt/Physics/Character/CharacterVirtual.h"

#include <iostream>
#include <cstdarg>
#include <thread>

// All Jolt symbols are in the JPH namespace
using namespace JPH;

// Callback for traces, connect this to your own trace function if you have one
static JPH_TraceFunc s_TraceFunc = nullptr;

static void TraceImpl(const char* fmt, ...)
{
    // Format the message
	va_list list;
	va_start(list, fmt);
	char buffer[1024];
	vsnprintf(buffer, sizeof(buffer), fmt, list);
	va_end(list);

    // Print to the TTY
	if(s_TraceFunc)
	{
		s_TraceFunc(buffer);
	}
	else
	{
		std::cout << buffer << std::endl;
	}
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
static inline JPH_Bool32 FromJolt(bool value)
{
    return value ? 1 : 0;
}

static inline void FromJolt(const JPH::Vec3& vec, JPH_Vec3* result)
{
    result->x = vec.GetX();
    result->y = vec.GetY();
    result->z = vec.GetZ();
}

static inline void FromJolt(const JPH::Quat& quat, JPH_Quat* result)
{
    result->x = quat.GetX();
    result->y = quat.GetY();
    result->z = quat.GetZ();
    result->w = quat.GetW();
}

static inline void FromJolt(const JPH::Plane& value, JPH_Plane* result)
{
	FromJolt(value.GetNormal(), &result->normal);
	result->distance = value.GetConstant();
}

static inline void FromJolt(const JPH::Mat44& matrix, JPH_Matrix4x4* result)
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
static inline void FromJolt(const JPH::RVec3& vec, JPH_RVec3* result)
{
    result->x = vec.GetX();
    result->y = vec.GetY();
    result->z = vec.GetZ();
}

static inline void FromJolt(const JPH::DMat44& matrix, JPH_RMatrix4x4* result)
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

static inline void FromJolt(const JPH::MassProperties& jolt, JPH_MassProperties* result)
{
	result->mass = jolt.mMass;
	FromJolt(jolt.mInertia, &result->inertia);
}

static inline void FromJolt(const JPH::SpringSettings& jolt, JPH_SpringSettings* result)
{
	result->mode = static_cast<JPH_SpringMode>(jolt.mMode);
    result->frequencyOrStiffness = jolt.mFrequency;
    result->damping = jolt.mDamping;
}

static inline void FromJolt(const JPH::MotorSettings& jolt, JPH_MotorSettings* result)
{
    FromJolt(jolt.mSpringSettings, &result->springSettings);
    result->minForceLimit = jolt.mMinForceLimit;
    result->maxForceLimit = jolt.mMaxForceLimit;
    result->minTorqueLimit = jolt.mMaxTorqueLimit;
    result->maxTorqueLimit = jolt.mMaxTorqueLimit;
}

// To Jolt conversion methods
static inline bool ToJolt(JPH_Bool32 value)
{
    return value == 1;
}

static inline JPH::Vec3 ToJolt(const JPH_Vec3& vec)
{
    return JPH::Vec3(vec.x, vec.y, vec.z);
}

static inline JPH::Vec3 ToJolt(const JPH_Vec3* vec)
{
    return JPH::Vec3(vec->x, vec->y, vec->z);
}

static inline JPH::Quat ToJolt(const JPH_Quat* quat)
{
    return JPH::Quat(quat->x, quat->y, quat->z, quat->w);
}

static inline JPH::Plane ToJolt(const JPH_Plane* value)
{
    return JPH::Plane(ToJolt(value->normal), value->distance);
}

static inline JPH::Mat44 ToJolt(const JPH_Matrix4x4& matrix)
{
    JPH::Mat44 result{};
    result.SetColumn4(0, JPH::Vec4(matrix.m11, matrix.m12, matrix.m13, matrix.m14));
    result.SetColumn4(1, JPH::Vec4(matrix.m21, matrix.m22, matrix.m23, matrix.m24));
    result.SetColumn4(2, JPH::Vec4(matrix.m31, matrix.m32, matrix.m33, matrix.m34));
    result.SetColumn4(3, JPH::Vec4(matrix.m41, matrix.m42, matrix.m43, matrix.m44));
    return result;
}

static inline JPH::Float3 ToJoltFloat3(const JPH_Vec3& vec)
{
    return JPH::Float3(vec.x, vec.y, vec.z);
}

#if defined(JPH_DOUBLE_PRECISION)
static inline JPH::RVec3 ToJolt(const JPH_RVec3* vec)
{
    return JPH::RVec3(vec->x, vec->y, vec->z);
}

static inline JPH::RMat44 ToJolt(const JPH_RMatrix4x4& matrix)
{
    JPH::RMat44 result{};
    result.SetColumn4(0, JPH::Vec4(matrix.m11, matrix.m12, matrix.m13, matrix.m14));
    result.SetColumn4(1, JPH::Vec4(matrix.m21, matrix.m22, matrix.m23, matrix.m24));
    result.SetColumn4(2, JPH::Vec4(matrix.m31, matrix.m32, matrix.m33, matrix.m34));
    result.SetTranslation(JPH::RVec3(matrix.m41, matrix.m42, matrix.m43));
    return result;
}
#endif /* defined(JPH_DOUBLE_PRECISION) */

static inline JPH::MassProperties ToJolt(const JPH_MassProperties* properties)
{
	JPH::MassProperties result{};
	result.mMass = properties->mass;
	result.mInertia = ToJolt(properties->inertia);
	return result;
}

static inline JPH::SpringSettings ToJolt(const JPH_SpringSettings* settings)
{
    JPH::SpringSettings result{};
	result.mMode = static_cast<JPH::ESpringMode>(settings->mode);
    result.mFrequency = settings->frequencyOrStiffness;
    result.mDamping = settings->damping;
    return result;
}

static inline JPH::MotorSettings ToJolt(const JPH_MotorSettings* settings)
{
    JPH::MotorSettings result{};
    result.mSpringSettings = ToJolt(&settings->springSettings);
    result.mMinForceLimit = settings->minForceLimit;
    result.mMaxForceLimit = settings->maxForceLimit;
    result.mMinTorqueLimit = settings->minTorqueLimit;
    result.mMaxTorqueLimit = settings->maxTorqueLimit;
    return result;
}

void JPH_MassProperties_DecomposePrincipalMomentsOfInertia(JPH_MassProperties* properties, JPH_Matrix4x4* rotation, JPH_Vec3* diagonal)
{
	JPH::Mat44 joltRotation;
	JPH::Vec3 joltDiagonal;
	JPH::MassProperties joltProperties = ToJolt(properties);
	joltProperties.DecomposePrincipalMomentsOfInertia(joltRotation, joltDiagonal);
	FromJolt(joltRotation, rotation);
	FromJolt(joltDiagonal, diagonal);
}

void JPH_MassProperties_ScaleToMass(JPH_MassProperties* properties, float mass)
{
	JPH::MassProperties joltProperties = ToJolt(properties);
	joltProperties.ScaleToMass(mass);
	properties->mass = joltProperties.mMass;
	FromJolt(joltProperties.mInertia, &properties->inertia);
}

static JPH::Triangle ToTriangle(const JPH_Triangle& triangle)
{
    return JPH::Triangle(ToJoltFloat3(triangle.v1), ToJoltFloat3(triangle.v2), ToJoltFloat3(triangle.v3), triangle.materialIndex);
}

static JPH::IndexedTriangle ToIndexedTriangle(const JPH_IndexedTriangle& triangle)
{
    return JPH::IndexedTriangle(triangle.i1, triangle.i2, triangle.i3, triangle.materialIndex, triangle.userData);
}

// 10 MB was not enough for large simulation, let's use TempAllocatorMalloc
static TempAllocator* s_TempAllocator = nullptr;
static JobSystemThreadPool* s_JobSystem  = nullptr;

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

	// Init temp allocator
	s_TempAllocator = new TempAllocatorImplWithMallocFallback(8 * 1024 * 1024);

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

void JPH_SetTraceHandler(JPH_TraceFunc handler)
{
	s_TraceFunc = handler;
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
	const uint cNumBodyMutexes = settings->numBodyMutexes;
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

void JPH_PhysicsSystem_SetPhysicsSettings(JPH_PhysicsSystem* system, JPH_PhysicsSettings* settings)
{
	JPH::PhysicsSettings joltSettings;
	joltSettings.mMaxInFlightBodyPairs = settings->maxInFlightBodyPairs;
	joltSettings.mStepListenersBatchSize = settings->stepListenersBatchSize;
	joltSettings.mStepListenerBatchesPerJob = settings->stepListenerBatchesPerJob;
	joltSettings.mBaumgarte = settings->baumgarte;
	joltSettings.mSpeculativeContactDistance = settings->speculativeContactDistance;
	joltSettings.mPenetrationSlop = settings->penetrationSlop;
	joltSettings.mLinearCastThreshold = settings->linearCastThreshold;
	joltSettings.mLinearCastMaxPenetration = settings->linearCastMaxPenetration;
	joltSettings.mManifoldToleranceSq = settings->manifoldToleranceSq;
	joltSettings.mMaxPenetrationDistance = settings->maxPenetrationDistance;
	joltSettings.mBodyPairCacheMaxDeltaPositionSq = settings->bodyPairCacheMaxDeltaPositionSq;
	joltSettings.mBodyPairCacheCosMaxDeltaRotationDiv2 = settings->bodyPairCacheCosMaxDeltaRotationDiv2;
	joltSettings.mContactNormalCosMaxDeltaRotation = settings->contactNormalCosMaxDeltaRotation;
	joltSettings.mContactPointPreserveLambdaMaxDistSq = settings->contactPointPreserveLambdaMaxDistSq;
	joltSettings.mNumVelocitySteps = settings->numVelocitySteps;
	joltSettings.mNumPositionSteps = settings->numPositionSteps;
	joltSettings.mMinVelocityForRestitution = settings->minVelocityForRestitution;
	joltSettings.mTimeBeforeSleep = settings->timeBeforeSleep;
	joltSettings.mPointVelocitySleepThreshold = settings->pointVelocitySleepThreshold;
	joltSettings.mDeterministicSimulation = static_cast<bool>(settings->deterministicSimulation);
	joltSettings.mConstraintWarmStart = static_cast<bool>(settings->constraintWarmStart);
	joltSettings.mUseBodyPairContactCache = static_cast<bool>(settings->useBodyPairContactCache);
	joltSettings.mUseManifoldReduction = static_cast<bool>(settings->useManifoldReduction);
	joltSettings.mUseLargeIslandSplitter = static_cast<bool>(settings->useLargeIslandSplitter);
	joltSettings.mAllowSleeping = static_cast<bool>(settings->allowSleeping);
	joltSettings.mCheckActiveEdges = static_cast<bool>(settings->checkActiveEdges);
	system->physicsSystem->SetPhysicsSettings(joltSettings);
}

void JPH_PhysicsSystem_GetPhysicsSettings(JPH_PhysicsSystem* system, JPH_PhysicsSettings* result)
{
	auto joltSettings = system->physicsSystem->GetPhysicsSettings();
	result->maxInFlightBodyPairs = joltSettings.mMaxInFlightBodyPairs;
	result->stepListenersBatchSize = joltSettings.mStepListenersBatchSize;
	result->stepListenerBatchesPerJob = joltSettings.mStepListenerBatchesPerJob;
	result->baumgarte = joltSettings.mBaumgarte;
	result->speculativeContactDistance = joltSettings.mSpeculativeContactDistance;
	result->penetrationSlop = joltSettings.mPenetrationSlop;
	result->linearCastThreshold = joltSettings.mLinearCastThreshold;
	result->linearCastMaxPenetration = joltSettings.mLinearCastMaxPenetration;
	result->manifoldToleranceSq = joltSettings.mManifoldToleranceSq;
	result->maxPenetrationDistance = joltSettings.mMaxPenetrationDistance;
	result->bodyPairCacheMaxDeltaPositionSq = joltSettings.mBodyPairCacheMaxDeltaPositionSq;
	result->bodyPairCacheCosMaxDeltaRotationDiv2 = joltSettings.mBodyPairCacheCosMaxDeltaRotationDiv2;
	result->contactNormalCosMaxDeltaRotation = joltSettings.mContactNormalCosMaxDeltaRotation;
	result->contactPointPreserveLambdaMaxDistSq = joltSettings.mContactPointPreserveLambdaMaxDistSq;
	result->numVelocitySteps = joltSettings.mNumVelocitySteps;
	result->numPositionSteps = joltSettings.mNumPositionSteps;
	result->minVelocityForRestitution = joltSettings.mMinVelocityForRestitution;
	result->timeBeforeSleep = joltSettings.mTimeBeforeSleep;
	result->pointVelocitySleepThreshold = joltSettings.mPointVelocitySleepThreshold;
	result->deterministicSimulation = joltSettings.mDeterministicSimulation;
	result->constraintWarmStart = joltSettings.mConstraintWarmStart;
	result->useBodyPairContactCache = joltSettings.mUseBodyPairContactCache;
	result->useManifoldReduction = joltSettings.mUseManifoldReduction;
	result->useLargeIslandSplitter = joltSettings.mUseLargeIslandSplitter;
	result->allowSleeping = joltSettings.mAllowSleeping;
	result->checkActiveEdges = joltSettings.mCheckActiveEdges;
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

const JPH_BodyLockInterface* JPH_PhysicsSystem_GetBodyLockInterface(const JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    return reinterpret_cast<const JPH_BodyLockInterface*>(&system->physicsSystem->GetBodyLockInterface());
}
const JPH_BodyLockInterface* JPH_PhysicsSystem_GetBodyLockInterfaceNoLock(const JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    return reinterpret_cast<const JPH_BodyLockInterface*>(&system->physicsSystem->GetBodyLockInterfaceNoLock());
}

/* JPH_BroadPhaseLayerFilter */
static const JPH::BroadPhaseLayerFilter& ToJolt(JPH_BroadPhaseLayerFilter* bpFilter)
{
    static const JPH::BroadPhaseLayerFilter g_defaultBroadPhaseLayerFilter = {};
    return bpFilter ? *reinterpret_cast<JPH::BroadPhaseLayerFilter*>(bpFilter) : g_defaultBroadPhaseLayerFilter;
}

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
        if (procs.ShouldCollide)
		{
            return procs.ShouldCollide(userData, static_cast<JPH_BroadPhaseLayer>(inLayer)) == 1;
        }

        return true;
    }

    JPH_BroadPhaseLayerFilter_Procs procs = {};
    void* userData = nullptr;
};

void JPH_BroadPhaseLayerFilter_SetProcs(JPH_BroadPhaseLayerFilter* filter, JPH_BroadPhaseLayerFilter_Procs procs, void* userData)
{
    auto managedFilter = reinterpret_cast<ManagedBroadPhaseLayerFilter*>(filter);
    managedFilter->procs = procs;
    managedFilter->userData = userData;
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
static const JPH::ObjectLayerFilter& ToJolt(JPH_ObjectLayerFilter* opFilter)
{
    static const JPH::ObjectLayerFilter g_defaultObjectLayerFilter = {};
    return opFilter ? *reinterpret_cast<JPH::ObjectLayerFilter*>(opFilter) : g_defaultObjectLayerFilter;
}

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
        if (procs.ShouldCollide)
        {
            return procs.ShouldCollide(userData, static_cast<JPH_ObjectLayer>(inLayer)) == 1;
        }

        return true;
    }

    JPH_ObjectLayerFilter_Procs procs = {};
    void* userData = nullptr;
};

void JPH_ObjectLayerFilter_SetProcs(JPH_ObjectLayerFilter* filter, JPH_ObjectLayerFilter_Procs procs, void* userData)
{
    auto managedFilter = reinterpret_cast<ManagedObjectLayerFilter*>(filter);
    managedFilter->procs = procs;
    managedFilter->userData = userData;
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
static const JPH::BodyFilter& ToJolt(JPH_BodyFilter* bodyFilter)
{
    static const JPH::BodyFilter g_defaultBodyFilter = {};
    return bodyFilter ? *reinterpret_cast<JPH::BodyFilter*>(bodyFilter) : g_defaultBodyFilter;
}

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
        if (procs.ShouldCollide)
        {
            return !!procs.ShouldCollide(userData, (JPH_BodyID)bodyID.GetIndexAndSequenceNumber());
        }

        return true;
    }

    bool ShouldCollideLocked(const Body& body) const override
    {
        if (procs.ShouldCollideLocked)
        {
            return !!procs.ShouldCollideLocked(userData, reinterpret_cast<const JPH_Body *>(&body));
        }

        return true;
    }

    JPH_BodyFilter_Procs procs = {};
    void* userData = nullptr;
};

void JPH_BodyFilter_SetProcs(JPH_BodyFilter* filter, JPH_BodyFilter_Procs procs, void* userData)
{
    auto managedFilter = reinterpret_cast<ManagedBodyFilter*>(filter);
    managedFilter->procs = procs;
    managedFilter->userData = userData;
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

/* Material */
JPH_PhysicsMaterial* JPH_PhysicsMaterial_Create(void)
{
    auto material = new JPH::PhysicsMaterial();
    material->AddRef();

    return reinterpret_cast<JPH_PhysicsMaterial*>(material);
}

void JPH_PhysicsMaterial_Destroy(JPH_PhysicsMaterial* material)
{
    if (material)
    {
        auto joltMaterial = reinterpret_cast<JPH::PhysicsMaterial*>(material);
        joltMaterial->Release();
    }
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
    return reinterpret_cast<const JPH::SphereShape*>(shape)->GetRadius();
}

/* PlaneShape */
JPH_PlaneShapeSettings* JPH_PlaneShapeSettings_Create(const JPH_Plane* plane, const JPH_PhysicsMaterial* material, float halfExtent)
{
    const JPH::PhysicsMaterial* joltMaterial = material != nullptr ? reinterpret_cast<const JPH::PhysicsMaterial*>(material) : nullptr;

    auto settings = new JPH::PlaneShapeSettings(ToJolt(plane), joltMaterial, halfExtent);
    settings->AddRef();

    return reinterpret_cast<JPH_PlaneShapeSettings*>(settings);
}

JPH_PlaneShape* JPH_PlaneShapeSettings_CreateShape(const JPH_PlaneShapeSettings* settings)
{
    const JPH::PlaneShapeSettings* joltSettings = reinterpret_cast<const JPH::PlaneShapeSettings*>(settings);
    auto shape_res = joltSettings->Create();

    auto shape = shape_res.Get().GetPtr();
    shape->AddRef();

    return reinterpret_cast<JPH_PlaneShape*>(shape);
}

JPH_PlaneShape* JPH_PlaneShape_Create(const JPH_Plane* plane, const JPH_PhysicsMaterial* material, float halfExtent)
{
    const JPH::PhysicsMaterial* joltMaterial = material != nullptr ? reinterpret_cast<const JPH::PhysicsMaterial*>(material) : nullptr;

    auto shape = new JPH::PlaneShape(ToJolt(plane), joltMaterial, halfExtent);
    shape->AddRef();

    return reinterpret_cast<JPH_PlaneShape*>(shape);
}

void JPH_PlaneShape_GetPlane(const JPH_PlaneShape* shape, JPH_Plane* result)
{
    FromJolt(reinterpret_cast<const JPH::PlaneShape*>(shape)->GetPlane(), result);
}

float JPH_PlaneShape_GetHalfExtent(const JPH_PlaneShape* shape)
{
    return reinterpret_cast<const JPH::PlaneShape*>(shape)->GetHalfExtent();
}

/* TriangleShape */
JPH_TriangleShapeSettings* JPH_TriangleShapeSettings_Create(const JPH_Vec3* v1, const JPH_Vec3* v2, const JPH_Vec3* v3, float convexRadius)
{
    auto settings = new JPH::TriangleShapeSettings(ToJolt(v1), ToJolt(v2), ToJolt(v3), convexRadius);
    settings->AddRef();

    return reinterpret_cast<JPH_TriangleShapeSettings*>(settings);
}

JPH_TriangleShape* JPH_TriangleShapeSettings_CreateShape(const JPH_TriangleShapeSettings* settings)
{
	const JPH::TriangleShapeSettings* joltSettings = reinterpret_cast<const JPH::TriangleShapeSettings*>(settings);
    auto shape_res = joltSettings->Create();

    auto shape = shape_res.Get().GetPtr();
    shape->AddRef();

    return reinterpret_cast<JPH_TriangleShape*>(shape);
}

JPH_TriangleShape* JPH_TriangleShape_Create(const JPH_Vec3* v1, const JPH_Vec3* v2, const JPH_Vec3* v3, float convexRadius)
{
	auto shape = new JPH::TriangleShape(ToJolt(v1), ToJolt(v2), ToJolt(v3), convexRadius);
    shape->AddRef();

    return reinterpret_cast<JPH_TriangleShape*>(shape);
}

float JPH_TriangleShape_GetConvexRadius(const JPH_TriangleShape* shape)
{
    return reinterpret_cast<const JPH::TriangleShape*>(shape)->GetConvexRadius();
}

/* CapsuleShapeSettings */
JPH_CapsuleShapeSettings* JPH_CapsuleShapeSettings_Create(float halfHeightOfCylinder, float radius)
{
    auto settings = new JPH::CapsuleShapeSettings(halfHeightOfCylinder, radius);
    settings->AddRef();

    return reinterpret_cast<JPH_CapsuleShapeSettings*>(settings);
}

JPH_CapsuleShape* JPH_CapsuleShapeSettings_CreateShape(const JPH_CapsuleShapeSettings* settings)
{
	const JPH::CapsuleShapeSettings* joltSettings = reinterpret_cast<const JPH::CapsuleShapeSettings*>(settings);
    auto shape_res = joltSettings->Create();

    auto shape = shape_res.Get().GetPtr();
    shape->AddRef();

    return reinterpret_cast<JPH_CapsuleShape*>(shape);
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

JPH_CylinderShape* JPH_CylinderShapeSettings_CreateShape(const JPH_CylinderShapeSettings* settings)
{
	const JPH::CylinderShapeSettings* joltSettings = reinterpret_cast<const JPH::CylinderShapeSettings*>(settings);
    auto shape_res = joltSettings->Create();

    auto shape = shape_res.Get().GetPtr();
    shape->AddRef();

    return reinterpret_cast<JPH_CylinderShape*>(shape);
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

/* TaperedCylinderShape */
JPH_TaperedCylinderShapeSettings* JPH_TaperedCylinderShapeSettings_Create(float halfHeightOfTaperedCylinder, float topRadius, float bottomRadius, float convexRadius/* = cDefaultConvexRadius*/, const JPH_PhysicsMaterial* material /* = NULL*/)
{
    const JPH::PhysicsMaterial* joltMaterial = material != nullptr ? reinterpret_cast<const JPH::PhysicsMaterial*>(material) : nullptr;

    auto settings = new JPH::TaperedCylinderShapeSettings(halfHeightOfTaperedCylinder, topRadius, bottomRadius, convexRadius, joltMaterial);
    settings->AddRef();

    return reinterpret_cast<JPH_TaperedCylinderShapeSettings*>(settings);
}

JPH_TaperedCylinderShape* JPH_TaperedCylinderShapeSettings_CreateShape(const JPH_TaperedCylinderShapeSettings* settings)
{
    const JPH::TaperedCylinderShapeSettings* joltSettings = reinterpret_cast<const JPH::TaperedCylinderShapeSettings*>(settings);
    auto shape_res = joltSettings->Create();

    auto shape = shape_res.Get().GetPtr();
    shape->AddRef();

    return reinterpret_cast<JPH_TaperedCylinderShape*>(shape);
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

uint32_t JPH_ConvexHullShape_GetNumPoints(const JPH_ConvexHullShape* shape)
{
	return reinterpret_cast<const JPH::ConvexHullShape*>(shape)->GetNumPoints();
}

void JPH_ConvexHullShape_GetPoint(const JPH_ConvexHullShape* shape, uint32_t index, JPH_Vec3* result)
{
	auto point = reinterpret_cast<const JPH::ConvexHullShape*>(shape)->GetPoint(index);
	FromJolt(point, result);
}

uint32_t JPH_ConvexHullShape_GetNumFaces(const JPH_ConvexHullShape* shape)
{
	return reinterpret_cast<const JPH::ConvexHullShape*>(shape)->GetNumFaces();
}

uint32_t JPH_ConvexHullShape_GetNumVerticesInFace(const JPH_ConvexHullShape* shape, uint32_t faceIndex)
{
	return reinterpret_cast<const JPH::ConvexHullShape*>(shape)->GetNumVerticesInFace(faceIndex);
}

uint32_t JPH_ConvexHullShape_GetFaceVertices(const JPH_ConvexHullShape* shape, uint32_t faceIndex, uint32_t maxVertices, uint32_t* vertices)
{
	return reinterpret_cast<const JPH::ConvexHullShape*>(shape)->GetFaceVertices(faceIndex, maxVertices, vertices);
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

JPH_TaperedCapsuleShape* JPH_TaperedCapsuleShapeSettings_CreateShape(JPH_TaperedCapsuleShapeSettings* settings)
{
	const JPH::TaperedCapsuleShapeSettings* joltSettings = reinterpret_cast<const JPH::TaperedCapsuleShapeSettings*>(settings);
    auto shape_res = joltSettings->Create();

    auto shape = shape_res.Get().GetPtr();
    shape->AddRef();

    return reinterpret_cast<JPH_TaperedCapsuleShape*>(shape);
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

/* DecoratedShape */
const JPH_Shape* JPH_DecoratedShape_GetInnerShape(const JPH_DecoratedShape* shape) {
	auto joltShape = reinterpret_cast<const JPH::DecoratedShape*>(shape);
	return reinterpret_cast<const JPH_Shape*>(joltShape->GetInnerShape());
}

/* RotatedTranslatedShape */
JPH_RotatedTranslatedShapeSettings* JPH_RotatedTranslatedShapeSettings_Create(const JPH_Vec3* position, const JPH_Quat* rotation, const JPH_ShapeSettings* shapeSettings)
{
    auto joltSettings = reinterpret_cast<const JPH::ShapeSettings*>(shapeSettings);

    auto settings = new JPH::RotatedTranslatedShapeSettings(ToJolt(position), ToJolt(rotation), joltSettings);
    settings->AddRef();

    return reinterpret_cast<JPH_RotatedTranslatedShapeSettings*>(settings);
}

JPH_RotatedTranslatedShapeSettings* JPH_RotatedTranslatedShapeSettings_Create2(const JPH_Vec3* position, const JPH_Quat* rotation, const JPH_Shape* shape)
{
    auto joltShape = reinterpret_cast<const JPH::Shape*>(shape);

    auto settings = new JPH::RotatedTranslatedShapeSettings(ToJolt(position), ToJolt(rotation), joltShape);
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

JPH_RotatedTranslatedShape* JPH_RotatedTranslatedShape_Create(const JPH_Vec3* position, const JPH_Quat* rotation, const JPH_Shape* shape)
{
    auto jolt_shape = reinterpret_cast<const JPH::Shape*>(shape);

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

/* JPH_OffsetCenterOfMassShape */
JPH_OffsetCenterOfMassShapeSettings* JPH_OffsetCenterOfMassShapeSettings_Create(const JPH_Vec3* offset, const JPH_ShapeSettings* shapeSettings)
{
	auto joltSettings = reinterpret_cast<const JPH::ShapeSettings*>(shapeSettings);

    auto settings = new JPH::OffsetCenterOfMassShapeSettings(ToJolt(offset), joltSettings);
    settings->AddRef();

    return reinterpret_cast<JPH_OffsetCenterOfMassShapeSettings*>(settings);
}

JPH_OffsetCenterOfMassShapeSettings* JPH_OffsetCenterOfMassShapeSettings_Create2(const JPH_Vec3* offset, const JPH_Shape* shape)
{
    auto joltShape = reinterpret_cast<const JPH::Shape*>(shape);

    auto rotatedTranslatedShape = new JPH::OffsetCenterOfMassShapeSettings(ToJolt(offset), joltShape);
    rotatedTranslatedShape->AddRef();

    return reinterpret_cast<JPH_OffsetCenterOfMassShapeSettings*>(rotatedTranslatedShape);
}

JPH_OffsetCenterOfMassShape* JPH_OffsetCenterOfMassShapeSettings_CreateShape(const JPH_OffsetCenterOfMassShapeSettings* settings)
{
	const JPH::OffsetCenterOfMassShapeSettings* joltSettings = reinterpret_cast<const JPH::OffsetCenterOfMassShapeSettings*>(settings);
    auto shape_res = joltSettings->Create();

    auto shape = shape_res.Get().GetPtr();
    shape->AddRef();

    return reinterpret_cast<JPH_OffsetCenterOfMassShape*>(shape);
}

JPH_OffsetCenterOfMassShape* JPH_OffsetCenterOfMassShape_Create(JPH_Vec3* offset, JPH_Shape* shape)
{
    auto joltShape = reinterpret_cast<JPH::Shape*>(shape);

    auto offsetCenterOfMassShape = new JPH::OffsetCenterOfMassShape(joltShape, ToJolt(offset));
    offsetCenterOfMassShape->AddRef();

    return reinterpret_cast<JPH_OffsetCenterOfMassShape*>(offsetCenterOfMassShape);
}

void JPH_OffsetCenterOfMassShape_GetOffset(const JPH_OffsetCenterOfMassShape* shape, JPH_Vec3* result)
{
    FromJolt(reinterpret_cast<const JPH::OffsetCenterOfMassShape*>(shape)->GetOffset(), result);
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

JPH_Bool32 JPH_Shape_CastRay(const JPH_Shape* shape, const JPH_Vec3* origin, const JPH_Vec3* direction, JPH_RayCastResult* hit)
{
    JPH_ASSERT(shape && origin && direction && hit);

    auto joltShape = reinterpret_cast<const JPH::Shape*>(shape);
    JPH::RayCast ray(ToJolt(origin), ToJolt(direction));
    SubShapeIDCreator creator;
    RayCastResult result;

    bool hadHit = joltShape->CastRay(ray, creator, result);

    if (hadHit)
    {
        hit->fraction = result.mFraction;
        hit->bodyID = result.mBodyID.GetIndexAndSequenceNumber();
        hit->subShapeID2 = result.mSubShapeID2.GetValue();
    }

    return static_cast<JPH_Bool32>(hadHit);
}

JPH_Bool32 JPH_Shape_CollidePoint(const JPH_Shape* shape, JPH_Vec3* point)
{
    JPH_ASSERT(shape && point);

    auto joltShape = reinterpret_cast<const JPH::Shape*>(shape);
    SubShapeIDCreator creator;
    AnyHitCollisionCollector<CollidePointCollector> collector;

    joltShape->CollidePoint(ToJolt(point), creator, collector);

    return static_cast<JPH_Bool32>(collector.HadHit());
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

void JPH_SoftBodyCreationSettings_Destroy(JPH_SoftBodyCreationSettings* settings)
{
    if (settings)
    {
		auto bodyCreationSettings = reinterpret_cast<JPH::SoftBodyCreationSettings*>(settings);
		delete bodyCreationSettings;
    }
}

/* JPH_ConstraintSettings */
void JPH_ConstraintSettings_Destroy(JPH_ConstraintSettings* settings)
{
    if (settings)
    {
        auto joltSettings = reinterpret_cast<JPH::ConstraintSettings*>(settings);
        joltSettings->Release();
    }
}

JPH_Bool32 JPH_ConstraintSettings_GetEnabled(JPH_ConstraintSettings* settings)
{
	JPH_ASSERT(settings);

    JPH::ConstraintSettings* joltSettings = reinterpret_cast<JPH::ConstraintSettings*>(settings);
    return static_cast<JPH_Bool32>(joltSettings->mEnabled);
}

void JPH_FixedConstraintSettings_SetEnabled(JPH_ConstraintSettings* settings, JPH_Bool32 value)
{
	JPH_ASSERT(settings);

    JPH::ConstraintSettings* joltSettings = reinterpret_cast<JPH::ConstraintSettings*>(settings);
    joltSettings->mEnabled = static_cast<bool>(value);
}

uint32_t JPH_ConstraintSettings_GetConstraintPriority(JPH_ConstraintSettings* settings)
{
	JPH_ASSERT(settings);

    JPH::ConstraintSettings* joltSettings = reinterpret_cast<JPH::ConstraintSettings*>(settings);
    return joltSettings->mConstraintPriority;
}

void JPH_FixedConstraintSettings_SetConstraintPriority(JPH_ConstraintSettings* settings, uint32_t value)
{
	JPH_ASSERT(settings);

    JPH::ConstraintSettings* joltSettings = reinterpret_cast<JPH::ConstraintSettings*>(settings);
    joltSettings->mConstraintPriority = value;
}

uint32_t JPH_ConstraintSettings_GetNumVelocityStepsOverride(JPH_ConstraintSettings* settings)
{
	JPH_ASSERT(settings);

    JPH::ConstraintSettings* joltSettings = reinterpret_cast<JPH::ConstraintSettings*>(settings);
    return joltSettings->mNumVelocityStepsOverride;
}

void JPH_ConstraintSettings_SetNumVelocityStepsOverride(JPH_ConstraintSettings* settings, uint32_t value)
{
	JPH_ASSERT(settings);

    JPH::ConstraintSettings* joltSettings = reinterpret_cast<JPH::ConstraintSettings*>(settings);
    joltSettings->mNumVelocityStepsOverride = value;
}

uint32_t JPH_ConstraintSettings_GetNumPositionStepsOverride(JPH_ConstraintSettings* settings)
{
	JPH_ASSERT(settings);

    JPH::ConstraintSettings* joltSettings = reinterpret_cast<JPH::ConstraintSettings*>(settings);
    return joltSettings->mNumPositionStepsOverride;
}

void JPH_ConstraintSettings_SetNumPositionStepsOverride(JPH_ConstraintSettings* settings, uint32_t value)
{
	JPH_ASSERT(settings);

    JPH::ConstraintSettings* joltSettings = reinterpret_cast<JPH::ConstraintSettings*>(settings);
    joltSettings->mNumPositionStepsOverride = value;
}

float JPH_ConstraintSettings_GetDrawConstraintSize(JPH_ConstraintSettings* settings)
{
	JPH_ASSERT(settings);

    JPH::ConstraintSettings* joltSettings = reinterpret_cast<JPH::ConstraintSettings*>(settings);
    return joltSettings->mDrawConstraintSize;
}

void JPH_ConstraintSettings_SetDrawConstraintSize(JPH_ConstraintSettings* settings, float value)
{
	JPH_ASSERT(settings);

    JPH::ConstraintSettings* joltSettings = reinterpret_cast<JPH::ConstraintSettings*>(settings);
    joltSettings->mDrawConstraintSize = value;
}

uint64_t JPH_ConstraintSettings_GetUserData(JPH_ConstraintSettings* settings)
{
	JPH_ASSERT(settings);

    JPH::ConstraintSettings* joltSettings = reinterpret_cast<JPH::ConstraintSettings*>(settings);
    return joltSettings->mUserData;
}

void JPH_ConstraintSettings_SetUserData(JPH_ConstraintSettings* settings, uint64_t value)
{
	JPH_ASSERT(settings);

    JPH::ConstraintSettings* joltSettings = reinterpret_cast<JPH::ConstraintSettings*>(settings);
    joltSettings->mUserData = value;
}

/* JPH_Constraint */
JPH_ConstraintSettings* JPH_Constraint_GetConstraintSettings(JPH_Constraint* constraint)
{
    auto joltConstraint = reinterpret_cast<JPH::Constraint*>(constraint);
    auto settings = joltConstraint->GetConstraintSettings().GetPtr();
    return reinterpret_cast<JPH_ConstraintSettings*>(settings);
}

JPH_ConstraintType JPH_Constraint_GetType(const JPH_Constraint* constraint)
{
	return static_cast<JPH_ConstraintType>(reinterpret_cast<const JPH::Constraint*>(constraint)->GetType());
}

JPH_ConstraintSubType JPH_Constraint_GetSubType(const JPH_Constraint* constraint)
{
	return static_cast<JPH_ConstraintSubType>(reinterpret_cast<const JPH::Constraint*>(constraint)->GetSubType());
}

uint32_t JPH_Constraint_GetConstraintPriority(const JPH_Constraint* constraint)
{
    return reinterpret_cast<const JPH::Constraint*>(constraint)->GetConstraintPriority();
}

void JPH_Constraint_SetConstraintPriority(JPH_Constraint* constraint, uint32_t priority)
{
    return reinterpret_cast<JPH::Constraint*>(constraint)->SetConstraintPriority(priority);
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

void JPH_Constraint_NotifyShapeChanged(JPH_Constraint* constraint, JPH_BodyID bodyID, JPH_Vec3* deltaCOM)
{
    reinterpret_cast<JPH::Constraint*>(constraint)->NotifyShapeChanged(JPH::BodyID(bodyID), ToJolt(deltaCOM));
}

void JPH_Constraint_Destroy(JPH_Constraint* constraint)
{
    if (constraint)
    {
        auto joltConstraint = reinterpret_cast<JPH::Constraint*>(constraint);
        joltConstraint->Release();
    }
}

/* JPH_TwoBodyConstraintSettings */

/* JPH_FixedConstraintSettings */
JPH_FixedConstraintSettings* JPH_FixedConstraintSettings_Create(void)
{
    JPH::FixedConstraintSettings* settings = new JPH::FixedConstraintSettings();
    settings->AddRef();

    return reinterpret_cast<JPH_FixedConstraintSettings*>(settings);
}

JPH_ConstraintSpace JPH_FixedConstraintSettings_GetSpace(JPH_FixedConstraintSettings* settings)
{
    JPH_ASSERT(settings);

    JPH::FixedConstraintSettings* joltSettings = reinterpret_cast<JPH::FixedConstraintSettings*>(settings);
    return static_cast<JPH_ConstraintSpace>(joltSettings->mSpace);
}

void JPH_FixedConstraintSettings_SetSpace(JPH_FixedConstraintSettings* settings, JPH_ConstraintSpace space)
{
    JPH_ASSERT(settings);

    JPH::FixedConstraintSettings* joltSettings = reinterpret_cast<JPH::FixedConstraintSettings*>(settings);
    joltSettings->mSpace = static_cast<JPH::EConstraintSpace>(space);
}

JPH_Bool32 JPH_FixedConstraintSettings_GetAutoDetectPoint(JPH_FixedConstraintSettings* settings)
{
	JPH_ASSERT(settings);

    JPH::FixedConstraintSettings* joltSettings = reinterpret_cast<JPH::FixedConstraintSettings*>(settings);
    return static_cast<JPH_Bool32>(joltSettings->mAutoDetectPoint);
}

void JPH_FixedConstraintSettings_SetAutoDetectPoint(JPH_FixedConstraintSettings* settings, JPH_Bool32 value)
{
	JPH_ASSERT(settings);

    JPH::FixedConstraintSettings* joltSettings = reinterpret_cast<JPH::FixedConstraintSettings*>(settings);
    joltSettings->mAutoDetectPoint = static_cast<bool>(value);
}

void JPH_FixedConstraintSettings_GetPoint1(JPH_FixedConstraintSettings* settings, JPH_RVec3* result)
{
    JPH_ASSERT(settings);

    JPH::FixedConstraintSettings* joltSettings = reinterpret_cast<JPH::FixedConstraintSettings*>(settings);
    FromJolt(joltSettings->mPoint1, result);
}

void JPH_FixedConstraintSettings_SetPoint1(JPH_FixedConstraintSettings* settings, const JPH_RVec3* value)
{
    JPH_ASSERT(settings);

	JPH::FixedConstraintSettings* joltSettings = reinterpret_cast<JPH::FixedConstraintSettings*>(settings);
    joltSettings->mPoint1 = ToJolt(value);
}

void JPH_FixedConstraintSettings_GetAxisX1(JPH_FixedConstraintSettings* settings, JPH_Vec3* result)
{
    JPH_ASSERT(settings);

	JPH::FixedConstraintSettings* joltSettings = reinterpret_cast<JPH::FixedConstraintSettings*>(settings);
    FromJolt(joltSettings->mAxisX1, result);
}

void JPH_FixedConstraintSettings_SetAxisX1(JPH_FixedConstraintSettings* settings, const JPH_Vec3* value)
{
    JPH_ASSERT(settings);

	JPH::FixedConstraintSettings* joltSettings = reinterpret_cast<JPH::FixedConstraintSettings*>(settings);
    joltSettings->mAxisX1 = ToJolt(value);
}

void JPH_FixedConstraintSettings_GetAxisY1(JPH_FixedConstraintSettings* settings, JPH_Vec3* result)
{
    JPH_ASSERT(settings);

	JPH::FixedConstraintSettings* joltSettings = reinterpret_cast<JPH::FixedConstraintSettings*>(settings);
    FromJolt(joltSettings->mAxisY1, result);
}

void JPH_FixedConstraintSettings_SetAxisY1(JPH_FixedConstraintSettings* settings, const JPH_Vec3* value)
{
    JPH_ASSERT(settings);

	JPH::FixedConstraintSettings* joltSettings = reinterpret_cast<JPH::FixedConstraintSettings*>(settings);
    joltSettings->mAxisX1 = ToJolt(value);
}

void JPH_FixedConstraintSettings_GetPoint2(JPH_FixedConstraintSettings* settings, JPH_RVec3* result)
{
    JPH_ASSERT(settings);

    JPH::FixedConstraintSettings* joltSettings = reinterpret_cast<JPH::FixedConstraintSettings*>(settings);
    FromJolt(joltSettings->mPoint2, result);
}

void JPH_FixedConstraintSettings_SetPoint2(JPH_FixedConstraintSettings* settings, const JPH_RVec3* value)
{
    JPH_ASSERT(settings);

    JPH::FixedConstraintSettings* joltSettings = reinterpret_cast<JPH::FixedConstraintSettings*>(settings);
    joltSettings->mPoint2 = ToJolt(value);
}

void JPH_FixedConstraintSettings_GetAxisX2(JPH_FixedConstraintSettings* settings, JPH_Vec3* result)
{
    JPH_ASSERT(settings);

	JPH::FixedConstraintSettings* joltSettings = reinterpret_cast<JPH::FixedConstraintSettings*>(settings);
    FromJolt(joltSettings->mAxisX2, result);
}

void JPH_FixedConstraintSettings_SetAxisX2(JPH_FixedConstraintSettings* settings, const JPH_Vec3* value)
{
    JPH_ASSERT(settings);

	JPH::FixedConstraintSettings* joltSettings = reinterpret_cast<JPH::FixedConstraintSettings*>(settings);
    joltSettings->mAxisX2 = ToJolt(value);
}

void JPH_FixedConstraintSettings_GetAxisY2(JPH_FixedConstraintSettings* settings, JPH_Vec3* result)
{
    JPH_ASSERT(settings);

	JPH::FixedConstraintSettings* joltSettings = reinterpret_cast<JPH::FixedConstraintSettings*>(settings);
    FromJolt(joltSettings->mAxisY2, result);
}

void JPH_FixedConstraintSettings_SetAxisY2(JPH_FixedConstraintSettings* settings, const JPH_Vec3* value)
{
    JPH_ASSERT(settings);

    JPH::FixedConstraintSettings* joltSettings = reinterpret_cast<JPH::FixedConstraintSettings*>(settings);
    joltSettings->mAxisY2 = ToJolt(value);
}

JPH_FixedConstraint* JPH_FixedConstraintSettings_CreateConstraint(JPH_FixedConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2)
{
    JPH_ASSERT(settings);
    JPH_ASSERT(body1);
    JPH_ASSERT(body2);

	JPH::FixedConstraintSettings* joltSettings = reinterpret_cast<JPH::FixedConstraintSettings*>(settings);
    JPH::Body* joltBody1 = reinterpret_cast<JPH::Body*>(body1);
    JPH::Body* joltBody2 = reinterpret_cast<JPH::Body*>(body2);

    JPH::FixedConstraint* constraint = static_cast<JPH::FixedConstraint*>(joltSettings->Create(*joltBody1, *joltBody2));
    constraint->AddRef();

    return reinterpret_cast<JPH_FixedConstraint*>(constraint);
}

/* JPH_FixedConstraint */
void JPH_FixedConstraint_GetTotalLambdaPosition(const JPH_FixedConstraint* constraint, JPH_Vec3* result)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::FixedConstraint*>(constraint);
	auto lambda = joltConstraint->GetTotalLambdaPosition();
	FromJolt(lambda, result);
}

void JPH_FixedConstraint_GetTotalLambdaRotation(const JPH_FixedConstraint* constraint, JPH_Vec3* result)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::FixedConstraint*>(constraint);
	auto lambda = joltConstraint->GetTotalLambdaRotation();
	FromJolt(lambda, result);
}

/* JPH_DistanceConstraintSettings */
JPH_DistanceConstraintSettings* JPH_DistanceConstraintSettings_Create(void)
{
    auto settings = new JPH::DistanceConstraintSettings();
    settings->AddRef();

    return reinterpret_cast<JPH_DistanceConstraintSettings*>(settings);
}

JPH_ConstraintSpace JPH_DistanceConstraintSettings_GetSpace(JPH_DistanceConstraintSettings* settings)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::DistanceConstraintSettings*>(settings);

    return static_cast<JPH_ConstraintSpace>(joltSettings->mSpace);
}

void JPH_DistanceConstraintSettings_SetSpace(JPH_DistanceConstraintSettings* settings, JPH_ConstraintSpace space)
{
	JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::DistanceConstraintSettings*>(settings);

    joltSettings->mSpace = static_cast<JPH::EConstraintSpace>(space);
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

void JPH_HingeConstraint_SetMaxFrictionTorque(JPH_HingeConstraint* constraint, float frictionTorque)
{
    reinterpret_cast<JPH::HingeConstraint*>(constraint)->SetMaxFrictionTorque(frictionTorque);
}

float JPH_HingeConstraint_GetMaxFrictionTorque(JPH_HingeConstraint* constraint)
{
    return reinterpret_cast<JPH::HingeConstraint*>(constraint)->GetMaxFrictionTorque();
}

void JPH_HingeConstraint_SetMotorSettings(JPH_HingeConstraint* constraint, JPH_MotorSettings* settings)
{
    auto joltConstraint = reinterpret_cast<JPH::HingeConstraint*>(constraint);
    JPH::MotorSettings& joltSettings = joltConstraint->GetMotorSettings();
    joltSettings = ToJolt(settings);
}

void JPH_HingeConstraint_GetMotorSettings(JPH_HingeConstraint* constraint, JPH_MotorSettings* result)
{
    auto joltConstraint = reinterpret_cast<JPH::HingeConstraint*>(constraint);
    FromJolt(joltConstraint->GetMotorSettings(), result);
}

void JPH_HingeConstraint_SetMotorState(JPH_HingeConstraint* constraint, JPH_MotorState state)
{
    reinterpret_cast<JPH::HingeConstraint*>(constraint)->SetMotorState(static_cast<JPH::EMotorState>(state));
}

JPH_MotorState JPH_HingeConstraint_GetMotorState(JPH_HingeConstraint* constraint)
{
    return static_cast<JPH_MotorState>(reinterpret_cast<JPH::HingeConstraint*>(constraint)->GetMotorState());
}

void JPH_HingeConstraint_SetTargetAngularVelocity(JPH_HingeConstraint* constraint, float angularVelocity)
{
    return reinterpret_cast<JPH::HingeConstraint*>(constraint)->SetTargetAngularVelocity(angularVelocity);
}

float JPH_HingeConstraint_GetTargetAngularVelocity(JPH_HingeConstraint* constraint)
{
    return reinterpret_cast<JPH::HingeConstraint*>(constraint)->GetTargetAngularVelocity();
}

void JPH_HingeConstraint_SetTargetAngle(JPH_HingeConstraint* constraint, float angle)
{
    return reinterpret_cast<JPH::HingeConstraint*>(constraint)->SetTargetAngle(angle);
}

float JPH_HingeConstraint_GetTargetAngle(JPH_HingeConstraint* constraint)
{
    return reinterpret_cast<JPH::HingeConstraint*>(constraint)->GetTargetAngle();
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

void JPH_HingeConstraint_GetLimitsSpringSettings(JPH_HingeConstraint* constraint, JPH_SpringSettings* result)
{
    auto joltConstraint = reinterpret_cast<JPH::HingeConstraint*>(constraint);
    FromJolt(joltConstraint->GetLimitsSpringSettings(), result);
}

void JPH_HingeConstraint_SetLimitsSpringSettings(JPH_HingeConstraint* constraint, JPH_SpringSettings* settings)
{
    auto joltConstraint = reinterpret_cast<JPH::HingeConstraint*>(constraint);
    joltConstraint->SetLimitsSpringSettings(ToJolt(settings));
}

void JPH_HingeConstraint_GetTotalLambdaPosition(const JPH_HingeConstraint* constraint, JPH_Vec3* result)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::HingeConstraint*>(constraint);
	auto lambda = joltConstraint->GetTotalLambdaPosition();
	FromJolt(lambda, result);
}

void JPH_HingeConstraint_GetTotalLambdaRotation(const JPH_HingeConstraint* constraint, float* x, float* y)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::HingeConstraint*>(constraint);
	auto lambda = joltConstraint->GetTotalLambdaRotation();
	*x = lambda[0];
	*y = lambda[1];
}

float JPH_HingeConstraint_GetTotalLambdaRotationLimits(const JPH_HingeConstraint* constraint)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::HingeConstraint*>(constraint);
	return joltConstraint->GetTotalLambdaRotationLimits();
}

float JPH_HingeConstraint_GetTotalLambdaMotor(const JPH_HingeConstraint* constraint)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::HingeConstraint*>(constraint);
	return joltConstraint->GetTotalLambdaMotor();
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

JPH_Bool32 JPH_SliderConstraintSettings_GetAutoDetectPoint(JPH_SliderConstraintSettings* settings)
{
	JPH_ASSERT(settings);

    JPH::SliderConstraintSettings* joltSettings = reinterpret_cast<JPH::SliderConstraintSettings*>(settings);
    return static_cast<JPH_Bool32>(joltSettings->mAutoDetectPoint);
}

void JPH_SliderConstraintSettings_SetAutoDetectPoint(JPH_SliderConstraintSettings* settings, JPH_Bool32 value)
{
	JPH_ASSERT(settings);

    JPH::SliderConstraintSettings* joltSettings = reinterpret_cast<JPH::SliderConstraintSettings*>(settings);
    joltSettings->mAutoDetectPoint = static_cast<bool>(value);
}

void JPH_SliderConstraintSettings_GetPoint1(JPH_SliderConstraintSettings* settings, JPH_RVec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::SliderConstraintSettings*>(settings);

    auto joltVector = joltSettings->mPoint1;
    FromJolt(joltVector, result);
}

void JPH_SliderConstraintSettings_SetPoint1(JPH_SliderConstraintSettings* settings, const JPH_RVec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::SliderConstraintSettings*>(settings);

    joltSettings->mPoint1 = ToJolt(value);
}

void JPH_SliderConstraintSettings_GetPoint2(JPH_SliderConstraintSettings* settings, JPH_RVec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::SliderConstraintSettings*>(settings);
    auto joltVector = joltSettings->mPoint2;
    FromJolt(joltVector, result);
}

void JPH_SliderConstraintSettings_SetPoint2(JPH_SliderConstraintSettings* settings, const JPH_RVec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::SliderConstraintSettings*>(settings);
    joltSettings->mPoint2 = ToJolt(value);
}

void JPH_SliderConstraintSettings_SetSliderAxis1(JPH_SliderConstraintSettings* settings, const JPH_Vec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::SliderConstraintSettings*>(settings);
    joltSettings->mSliderAxis1 = ToJolt(value);
}

void JPH_SliderConstraintSettings_GetSliderAxis1(JPH_SliderConstraintSettings* settings, JPH_Vec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::SliderConstraintSettings*>(settings);
    FromJolt(joltSettings->mSliderAxis1, result);
}

void JPH_SliderConstraintSettings_SetNormalAxis1(JPH_SliderConstraintSettings* settings, const JPH_Vec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::SliderConstraintSettings*>(settings);
    joltSettings->mNormalAxis1 = ToJolt(value);
}

void JPH_SliderConstraintSettings_GetNormalAxis1(JPH_SliderConstraintSettings* settings, JPH_Vec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::SliderConstraintSettings*>(settings);
    FromJolt(joltSettings->mNormalAxis1, result);
}

void JPH_SliderConstraintSettings_SetSliderAxis2(JPH_SliderConstraintSettings* settings, const JPH_Vec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::SliderConstraintSettings*>(settings);
    joltSettings->mSliderAxis2 = ToJolt(value);
}

void JPH_SliderConstraintSettings_GetSliderAxis2(JPH_SliderConstraintSettings* settings, JPH_Vec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::SliderConstraintSettings*>(settings);
    FromJolt(joltSettings->mSliderAxis2, result);
}

void JPH_SliderConstraintSettings_SetNormalAxis2(JPH_SliderConstraintSettings* settings, const JPH_Vec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::SliderConstraintSettings*>(settings);
    joltSettings->mNormalAxis2 = ToJolt(value);
}

void JPH_SliderConstraintSettings_GetNormalAxis2(JPH_SliderConstraintSettings* settings, JPH_Vec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::SliderConstraintSettings*>(settings);
    FromJolt(joltSettings->mNormalAxis2, result);
}

JPH_SliderConstraint* JPH_SliderConstraintSettings_CreateConstraint(JPH_SliderConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2)
{
    auto joltBody1 = reinterpret_cast<JPH::Body*>(body1);
    auto joltBody2 = reinterpret_cast<JPH::Body*>(body2);
    JPH::TwoBodyConstraint* constraint = reinterpret_cast<JPH::SliderConstraintSettings*>(settings)->Create(*joltBody1, *joltBody2);
    constraint->AddRef();

    return reinterpret_cast<JPH_SliderConstraint*>(static_cast<JPH::SliderConstraint*>(constraint));
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

void JPH_SliderConstraint_SetMaxFrictionForce(JPH_SliderConstraint* constraint, float frictionForce)
{
    reinterpret_cast<JPH::SliderConstraint*>(constraint)->SetMaxFrictionForce(frictionForce);
}

float JPH_SliderConstraint_GetMaxFrictionForce(JPH_SliderConstraint* constraint)
{
    return reinterpret_cast<JPH::SliderConstraint*>(constraint)->GetMaxFrictionForce();
}

void JPH_SliderConstraint_SetMotorSettings(JPH_SliderConstraint* constraint, JPH_MotorSettings* settings)
{
    auto joltConstraint = reinterpret_cast<JPH::SliderConstraint*>(constraint);
    JPH::MotorSettings& joltSettings = joltConstraint->GetMotorSettings();
    joltSettings = ToJolt(settings);
}

void JPH_SliderConstraint_GetMotorSettings(JPH_SliderConstraint* constraint, JPH_MotorSettings* result)
{
    auto joltConstraint = reinterpret_cast<JPH::SliderConstraint*>(constraint);
    FromJolt(joltConstraint->GetMotorSettings(), result);
}

void JPH_SliderConstraint_SetMotorState(JPH_SliderConstraint* constraint, JPH_MotorState state)
{
    reinterpret_cast<JPH::SliderConstraint*>(constraint)->SetMotorState(static_cast<JPH::EMotorState>(state));
}

JPH_MotorState JPH_SliderConstraint_GetMotorState(JPH_SliderConstraint* constraint)
{
    return static_cast<JPH_MotorState>(reinterpret_cast<JPH::SliderConstraint*>(constraint)->GetMotorState());
}

void JPH_SliderConstraint_SetTargetVelocity(JPH_SliderConstraint* constraint, float velocity)
{
    return reinterpret_cast<JPH::SliderConstraint*>(constraint)->SetTargetVelocity(velocity);
}

float JPH_SliderConstraint_GetTargetVelocity(JPH_SliderConstraint* constraint)
{
    return reinterpret_cast<JPH::SliderConstraint*>(constraint)->GetTargetVelocity();
}

void JPH_SliderConstraint_SetTargetPosition(JPH_SliderConstraint* constraint, float position)
{
    return reinterpret_cast<JPH::SliderConstraint*>(constraint)->SetTargetPosition(position);
}

float JPH_SliderConstraint_GetTargetPosition(JPH_SliderConstraint* constraint)
{
    return reinterpret_cast<JPH::SliderConstraint*>(constraint)->GetTargetPosition();
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

void JPH_SliderConstraint_GetLimitsSpringSettings(JPH_SliderConstraint* constraint, JPH_SpringSettings* result)
{
    const auto joltConstraint = reinterpret_cast<const JPH::SliderConstraint*>(constraint);
    FromJolt(joltConstraint->GetLimitsSpringSettings(), result);
}

void JPH_SliderConstraint_SetLimitsSpringSettings(JPH_SliderConstraint* constraint, JPH_SpringSettings* settings)
{
    auto joltConstraint = reinterpret_cast<JPH::SliderConstraint*>(constraint);
    joltConstraint->SetLimitsSpringSettings(ToJolt(settings));
}

void JPH_SliderConstraint_GetTotalLambdaPosition(const JPH_SliderConstraint* constraint, float* x, float* y)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::SliderConstraint*>(constraint);
	auto lambda = joltConstraint->GetTotalLambdaPosition();
	*x = lambda[0];
	*y = lambda[1];
}

float JPH_SliderConstraint_GetTotalLambdaPositionLimits(const JPH_SliderConstraint* constraint)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::SliderConstraint*>(constraint);
	return joltConstraint->GetTotalLambdaPositionLimits();
}

void JPH_SliderConstraint_GetTotalLambdaRotation(const JPH_SliderConstraint* constraint, JPH_Vec3* result)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::SliderConstraint*>(constraint);
	auto lambda = joltConstraint->GetTotalLambdaRotation();
	FromJolt(lambda, result);
}

float JPH_SliderConstraint_GetTotalLambdaMotor(const JPH_SliderConstraint* constraint)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::SliderConstraint*>(constraint);
	return joltConstraint->GetTotalLambdaMotor();
}

/* JPH_ConeConstraintSettings */
JPH_ConeConstraintSettings* JPH_ConeConstraintSettings_Create(void)
{
	auto settings = new JPH::ConeConstraintSettings();
    settings->AddRef();

    return reinterpret_cast<JPH_ConeConstraintSettings*>(settings);
}

void JPH_ConeConstraintSettings_SetPoint1(JPH_ConeConstraintSettings* settings, const JPH_RVec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::ConeConstraintSettings*>(settings);
    joltSettings->mPoint1 = ToJolt(value);
}

void JPH_ConeConstraintSettings_GetPoint1(JPH_ConeConstraintSettings* settings, JPH_RVec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::ConeConstraintSettings*>(settings);
    auto joltVector = joltSettings->mPoint1;
    FromJolt(joltVector, result);
}

void JPH_ConeConstraintSettings_SetPoint2(JPH_ConeConstraintSettings* settings, const JPH_RVec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::ConeConstraintSettings*>(settings);
    joltSettings->mPoint2 = ToJolt(value);
}

void JPH_ConeConstraintSettings_GetPoint2(JPH_ConeConstraintSettings* settings, JPH_RVec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::ConeConstraintSettings*>(settings);
    auto joltVector = joltSettings->mPoint2;
    FromJolt(joltVector, result);
}

void JPH_ConeConstraintSettings_SetTwistAxis1(JPH_ConeConstraintSettings* settings, const JPH_Vec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::ConeConstraintSettings*>(settings);
    joltSettings->mTwistAxis1 = ToJolt(value);
}

void JPH_ConeConstraintSettings_GetTwistAxis1(JPH_ConeConstraintSettings* settings, JPH_Vec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::ConeConstraintSettings*>(settings);
    FromJolt(joltSettings->mTwistAxis1, result);
}

void JPH_ConeConstraintSettings_SetTwistAxis2(JPH_ConeConstraintSettings* settings, const JPH_Vec3* value)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::ConeConstraintSettings*>(settings);
    joltSettings->mTwistAxis2 = ToJolt(value);
}

void JPH_ConeConstraintSettings_GetTwistAxis2(JPH_ConeConstraintSettings* settings, JPH_Vec3* result)
{
    JPH_ASSERT(settings);
    auto joltSettings = reinterpret_cast<JPH::ConeConstraintSettings*>(settings);
    FromJolt(joltSettings->mTwistAxis2, result);
}

void JPH_ConeConstraintSettings_SetHalfConeAngle(JPH_ConeConstraintSettings* settings, float halfConeAngle)
{
    JPH_ASSERT(settings);
    reinterpret_cast<JPH::ConeConstraintSettings*>(settings)->mHalfConeAngle = halfConeAngle;
}

float JPH_ConeConstraintSettings_GetHalfConeAngle(JPH_ConeConstraintSettings* settings)
{
    JPH_ASSERT(settings);
    return reinterpret_cast<JPH::ConeConstraintSettings*>(settings)->mHalfConeAngle;
}

JPH_ConeConstraint* JPH_ConeConstraintSettings_CreateConstraint(JPH_ConeConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2)
{
    auto joltBody1 = reinterpret_cast<JPH::Body*>(body1);
    auto joltBody2 = reinterpret_cast<JPH::Body*>(body2);
    JPH::TwoBodyConstraint* constraint = reinterpret_cast<JPH::ConeConstraintSettings*>(settings)->Create(*joltBody1, *joltBody2);
    constraint->AddRef();

    return reinterpret_cast<JPH_ConeConstraint*>(static_cast<JPH::ConeConstraint*>(constraint));
}

/* JPH_ConeConstraint */
void JPH_ConeConstraint_SetHalfConeAngle(JPH_ConeConstraint* constraint, float halfConeAngle)
{
	JPH_ASSERT(constraint);
	reinterpret_cast<JPH::ConeConstraint*>(constraint)->SetHalfConeAngle(halfConeAngle);
}

float JPH_ConeConstraint_GetCosHalfConeAngle(const JPH_ConeConstraint* constraint)
{
	JPH_ASSERT(constraint);
	return reinterpret_cast<const JPH::ConeConstraint*>(constraint)->GetCosHalfConeAngle();
}

void JPH_ConeConstraint_GetTotalLambdaPosition(const JPH_ConeConstraint* constraint, JPH_Vec3* result)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::ConeConstraint*>(constraint);
	auto lambda = joltConstraint->GetTotalLambdaPosition();
	FromJolt(lambda, result);
}

float JPH_ConeConstraint_GetTotalLambdaRotation(const JPH_ConeConstraint* constraint)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::ConeConstraint*>(constraint);
	return joltConstraint->GetTotalLambdaRotation();
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

void JPH_SwingTwistConstraint_GetTotalLambdaPosition(const JPH_SwingTwistConstraint* constraint, JPH_Vec3* result)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::SwingTwistConstraint*>(constraint);
	auto lambda = joltConstraint->GetTotalLambdaPosition();
	FromJolt(lambda, result);
}

float JPH_SwingTwistConstraint_GetTotalLambdaTwist(const JPH_SwingTwistConstraint* constraint)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::SwingTwistConstraint*>(constraint);
	return joltConstraint->GetTotalLambdaTwist();
}

float JPH_SwingTwistConstraint_GetTotalLambdaSwingY(const JPH_SwingTwistConstraint* constraint)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::SwingTwistConstraint*>(constraint);
	return joltConstraint->GetTotalLambdaSwingY();
}

float JPH_SwingTwistConstraint_GetTotalLambdaSwingZ(const JPH_SwingTwistConstraint* constraint)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::SwingTwistConstraint*>(constraint);
	return joltConstraint->GetTotalLambdaSwingZ();
}

void JPH_SwingTwistConstraint_GetTotalLambdaMotor(const JPH_SwingTwistConstraint* constraint, JPH_Vec3* result)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::SwingTwistConstraint*>(constraint);
	auto lambda = joltConstraint->GetTotalLambdaMotor();
	FromJolt(lambda, result);
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

void JPH_SixDOFConstraint_GetTotalLambdaPosition(const JPH_SixDOFConstraint* constraint, JPH_Vec3* result)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::SixDOFConstraint*>(constraint);
	auto lambda = joltConstraint->GetTotalLambdaPosition();
	FromJolt(lambda, result);
}

void JPH_SixDOFConstraint_GetTotalLambdaRotation(const JPH_SixDOFConstraint* constraint, JPH_Vec3* result)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::SixDOFConstraint*>(constraint);
	auto lambda = joltConstraint->GetTotalLambdaRotation();
	FromJolt(lambda, result);
}

void JPH_SixDOFConstraint_GetTotalLambdaMotorTranslation(const JPH_SixDOFConstraint* constraint, JPH_Vec3* result)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::SixDOFConstraint*>(constraint);
	auto lambda = joltConstraint->GetTotalLambdaMotorTranslation();
	FromJolt(lambda, result);
}

void JPH_SixDOFConstraint_GetTotalLambdaMotorRotation(const JPH_SixDOFConstraint* constraint, JPH_Vec3* result)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::SixDOFConstraint*>(constraint);
	auto lambda = joltConstraint->GetTotalLambdaMotorRotation();
	FromJolt(lambda, result);
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

void JPH_DistanceConstraint_GetLimitsSpringSettings(JPH_DistanceConstraint* constraint, JPH_SpringSettings* result)
{
    auto joltConstraint = reinterpret_cast<JPH::DistanceConstraint*>(constraint);
    FromJolt(joltConstraint->GetLimitsSpringSettings(), result);
}

void JPH_DistanceConstraint_SetLimitsSpringSettings(JPH_DistanceConstraint* constraint, JPH_SpringSettings* settings)
{
    auto joltConstraint = reinterpret_cast<JPH::DistanceConstraint*>(constraint);
    joltConstraint->SetLimitsSpringSettings(ToJolt(settings));
}

float JPH_DistanceConstraint_GetTotalLambdaPosition(const JPH_DistanceConstraint* constraint)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::DistanceConstraint*>(constraint);
	return joltConstraint->GetTotalLambdaPosition();
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

void JPH_PointConstraint_GetTotalLambdaPosition(const JPH_PointConstraint* constraint, JPH_Vec3* result)
{
	JPH_ASSERT(constraint);
	auto joltConstraint = reinterpret_cast<const JPH::PointConstraint*>(constraint);
	auto lambda = joltConstraint->GetTotalLambdaPosition();
	FromJolt(lambda, result);
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
JPH_AllowedDOFs JPH_MotionProperties_GetAllowedDOFs(const JPH_MotionProperties* properties)
{
    return static_cast<JPH_AllowedDOFs>(reinterpret_cast<const JPH::MotionProperties*>(properties)->GetAllowedDOFs());
}

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

void JPH_MotionProperties_SetMassProperties(JPH_MotionProperties* properties, JPH_AllowedDOFs allowedDOFs, const JPH_MassProperties* massProperties)
{
    reinterpret_cast<JPH::MotionProperties*>(properties)->SetMassProperties(
        static_cast<EAllowedDOFs>(allowedDOFs),
        ToJolt(massProperties));
}

float JPH_MotionProperties_GetInverseMassUnchecked(JPH_MotionProperties* properties)
{
    return reinterpret_cast<JPH::MotionProperties*>(properties)->GetInverseMassUnchecked();
}

void JPH_MotionProperties_SetInverseMass(JPH_MotionProperties* properties, float inverseMass)
{
    reinterpret_cast<JPH::MotionProperties*>(properties)->SetInverseMass(inverseMass);
}

void JPH_MotionProperties_GetInverseInertiaDiagonal(JPH_MotionProperties* properties, JPH_Vec3* result)
{
    FromJolt(reinterpret_cast<JPH::MotionProperties*>(properties)->GetInverseInertiaDiagonal(), result);
}

void JPH_MotionProperties_GetInertiaRotation(JPH_MotionProperties* properties, JPH_Quat* result)
{
    FromJolt(reinterpret_cast<JPH::MotionProperties*>(properties)->GetInertiaRotation(), result);
}

void JPH_MotionProperties_SetInverseInertia(JPH_MotionProperties* properties, JPH_Vec3* diagonal, JPH_Quat* rot)
{
    reinterpret_cast<JPH::MotionProperties*>(properties)->SetInverseInertia(ToJolt(diagonal), ToJolt(rot));
}

const JPH_BroadPhaseQuery* JPH_PhysicsSystem_GetBroadPhaseQuery(const JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    return reinterpret_cast<const JPH_BroadPhaseQuery*>(&system->physicsSystem->GetBroadPhaseQuery());
}

const JPH_NarrowPhaseQuery* JPH_PhysicsSystem_GetNarrowPhaseQuery(const JPH_PhysicsSystem* system)
{
    JPH_ASSERT(system);

    return reinterpret_cast<const JPH_NarrowPhaseQuery*>(&system->physicsSystem->GetNarrowPhaseQuery());
}
const JPH_NarrowPhaseQuery* JPH_PhysicsSystem_GetNarrowPhaseQueryNoLock(const JPH_PhysicsSystem* system)
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

JPH_Bool32 JPH_PhysicsSystem_WereBodiesInContact(const JPH_PhysicsSystem* system, JPH_BodyID body1, JPH_BodyID body2)
{
	JPH_ASSERT(system);

	return system->physicsSystem->WereBodiesInContact(JPH::BodyID(body1), JPH::BodyID(body2));
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

JPH_Body* JPH_BodyInterface_CreateSoftBody(JPH_BodyInterface* interface, const JPH_SoftBodyCreationSettings* settings)
{
    auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
	auto joltSettings = reinterpret_cast<const JPH::SoftBodyCreationSettings*>(settings);

    JPH::Body* body = joltBodyInterface->CreateSoftBody(*joltSettings);
    return reinterpret_cast<JPH_Body*>(body);
}

JPH_Body* JPH_BodyInterface_CreateSoftBodyWithID(JPH_BodyInterface* interface, JPH_BodyID bodyID, const JPH_SoftBodyCreationSettings* settings)
{
	auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
	auto joltSettings = reinterpret_cast<const JPH::SoftBodyCreationSettings*>(settings);

    JPH::Body* body = joltBodyInterface->CreateSoftBodyWithID(JPH::BodyID(bodyID),*joltSettings);
    return reinterpret_cast<JPH_Body*>(body);
}

JPH_Body* JPH_BodyInterface_CreateSoftBodyWithoutID(JPH_BodyInterface* interface, const JPH_SoftBodyCreationSettings* settings)
{
	auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
	auto joltSettings = reinterpret_cast<const JPH::SoftBodyCreationSettings*>(settings);

    JPH::Body* body = joltBodyInterface->CreateSoftBodyWithoutID(*joltSettings);
    return reinterpret_cast<JPH_Body*>(body);
}

JPH_BodyID JPH_BodyInterface_CreateAndAddSoftBody(JPH_BodyInterface* interface, const JPH_SoftBodyCreationSettings* settings, JPH_Activation activationMode)
{
	auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);
	auto joltSettings = reinterpret_cast<const JPH::SoftBodyCreationSettings*>(settings);

    JPH::BodyID bodyID = joltBodyInterface->CreateAndAddSoftBody(*joltSettings, static_cast<JPH::EActivation>(activationMode));
    return bodyID.GetIndexAndSequenceNumber();
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

void JPH_BodyInterface_GetPositionAndRotation(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* position, JPH_Quat* rotation)
{
	JPH_ASSERT(interface);
	auto joltBodyInterface = reinterpret_cast<JPH::BodyInterface*>(interface);

	JPH::RVec3 joltPosition;
	JPH::Quat joltRotation;
	joltBodyInterface->GetPositionAndRotation(JPH::BodyID(bodyId), joltPosition, joltRotation);
	FromJolt(joltPosition, position);
	FromJolt(joltRotation, rotation);
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
// JPH_BroadPhaseQuery
//--------------------------------------------------------------------------------------------------
class RayCastBodyCollectorCallback : public RayCastBodyCollector
{
public:
    RayCastBodyCollectorCallback(JPH_RayCastBodyCollector* proc, void* userData) : proc(proc), userData(userData) {}

    virtual void AddHit(const BroadPhaseCastResult& result)
    {
        JPH_BroadPhaseCastResult hit;
        hit.bodyID = result.mBodyID.GetIndexAndSequenceNumber();
        hit.fraction = result.mFraction;

        float fraction = proc(userData, &hit);
        UpdateEarlyOutFraction(fraction);
        hadHit = true;
    }

    JPH_RayCastBodyCollector* proc;
    void* userData;
    JPH_Bool32 hadHit = false;
	uint32_t _padding;
};

class CollideShapeBodyCollectorCallback : public CollideShapeBodyCollector
{
public:
    CollideShapeBodyCollectorCallback(JPH_CollideShapeBodyCollector* proc, void* userData) : proc(proc), userData(userData) {}

    virtual void AddHit(const BodyID& result)
    {
        proc(userData, result.GetIndexAndSequenceNumber());
        hadHit = true;
    }

    JPH_CollideShapeBodyCollector* proc;
    void* userData;
    JPH_Bool32 hadHit = false;
	uint32_t _padding;
};

JPH_Bool32 JPH_BroadPhaseQuery_CastRay(const JPH_BroadPhaseQuery* query,
    const JPH_Vec3* origin, const JPH_Vec3* direction,
    JPH_RayCastBodyCollector* callback, void* userData,
    JPH_BroadPhaseLayerFilter* broadPhaseLayerFilter,
    JPH_ObjectLayerFilter* objectLayerFilter)
{
    JPH_ASSERT(query && origin && direction && callback);
    auto joltQuery = reinterpret_cast<const JPH::BroadPhaseQuery*>(query);
    JPH::RayCast ray(ToJolt(origin), ToJolt(direction));
    RayCastBodyCollectorCallback collector(callback, userData);
    joltQuery->CastRay(ray, collector, ToJolt(broadPhaseLayerFilter), ToJolt(objectLayerFilter));
    return collector.hadHit;
}

JPH_Bool32 JPH_BroadPhaseQuery_CollideAABox(const JPH_BroadPhaseQuery* query,
    const JPH_AABox* box, JPH_CollideShapeBodyCollector* callback, void* userData,
    JPH_BroadPhaseLayerFilter* broadPhaseLayerFilter,
    JPH_ObjectLayerFilter* objectLayerFilter)
{
    JPH_ASSERT(query && box && callback);
    auto joltQuery = reinterpret_cast<const JPH::BroadPhaseQuery*>(query);
    JPH::AABox joltBox(ToJolt(&box->min), ToJolt(&box->max));
    CollideShapeBodyCollectorCallback collector(callback, userData);
    joltQuery->CollideAABox(joltBox, collector, ToJolt(broadPhaseLayerFilter), ToJolt(objectLayerFilter));
    return collector.hadHit;
}

JPH_Bool32 JPH_BroadPhaseQuery_CollideSphere(const JPH_BroadPhaseQuery* query,
    const JPH_Vec3* center, float radius, JPH_CollideShapeBodyCollector* callback, void* userData,
    JPH_BroadPhaseLayerFilter* broadPhaseLayerFilter,
    JPH_ObjectLayerFilter* objectLayerFilter)
{
    JPH_ASSERT(query && center && callback);
    auto joltQuery = reinterpret_cast<const JPH::BroadPhaseQuery*>(query);
    CollideShapeBodyCollectorCallback collector(callback, userData);
    joltQuery->CollideSphere(ToJolt(center), radius, collector, ToJolt(broadPhaseLayerFilter), ToJolt(objectLayerFilter));
    return collector.hadHit;
}

JPH_Bool32 JPH_BroadPhaseQuery_CollidePoint(const JPH_BroadPhaseQuery* query,
    const JPH_Vec3* point, JPH_CollideShapeBodyCollector* callback, void* userData,
    JPH_BroadPhaseLayerFilter* broadPhaseLayerFilter,
    JPH_ObjectLayerFilter* objectLayerFilter)
{
    JPH_ASSERT(query && point && callback);
    auto joltQuery = reinterpret_cast<const JPH::BroadPhaseQuery*>(query);
    CollideShapeBodyCollectorCallback collector(callback, userData);
    joltQuery->CollidePoint(ToJolt(point), collector, ToJolt(broadPhaseLayerFilter), ToJolt(objectLayerFilter));
    return collector.hadHit;
}

//--------------------------------------------------------------------------------------------------
// JPH_NarrowPhaseQuery
//--------------------------------------------------------------------------------------------------
class CastRayCollectorCallback : public CastRayCollector
{
public:
    CastRayCollectorCallback(JPH_CastRayCollector* proc, void* userData) : proc(proc), userData(userData) {}

    virtual void AddHit(const RayCastResult& result)
    {
        JPH_RayCastResult hit;
        hit.bodyID = result.mBodyID.GetIndexAndSequenceNumber();
        hit.fraction = result.mFraction;
        hit.subShapeID2 = result.mSubShapeID2.GetValue();

        float fraction = proc(userData, &hit);
        UpdateEarlyOutFraction(fraction);
        hadHit = true;
    }

    JPH_CastRayCollector* proc;
    void* userData;
    JPH_Bool32 hadHit = false;
	uint32_t _padding;
};

class CollidePointCollectorCallback : public CollidePointCollector
{
public:
    CollidePointCollectorCallback(JPH_CollidePointCollector* proc, void* userData) : proc(proc), userData(userData) {}

    virtual void AddHit(const CollidePointResult& result)
    {
        JPH_CollidePointResult hit;
        hit.bodyID = result.mBodyID.GetIndexAndSequenceNumber();
        hit.subShapeID2 = result.mSubShapeID2.GetValue();

        float fraction = proc(userData, &hit);
        UpdateEarlyOutFraction(fraction);
        hadHit = true;
    }

    JPH_CollidePointCollector* proc;
    void* userData;
    JPH_Bool32 hadHit = false;
	uint32_t _padding;
};

class CollideShapeCollectorCallback : public CollideShapeCollector
{
public:
    CollideShapeCollectorCallback(JPH_CollideShapeCollector* proc, void* userData) : proc(proc), userData(userData) {}

    virtual void AddHit(const CollideShapeResult& result)
    {
        JPH_CollideShapeResult hit;
        FromJolt(result.mContactPointOn1, &hit.contactPointOn1);
        FromJolt(result.mContactPointOn2, &hit.contactPointOn2);
        FromJolt(result.mPenetrationAxis, &hit.penetrationAxis);
        hit.penetrationDepth = result.mPenetrationDepth;
        hit.subShapeID1 = result.mSubShapeID1.GetValue();
        hit.subShapeID2 = result.mSubShapeID2.GetValue();
        hit.bodyID2 = result.mBodyID2.GetIndexAndSequenceNumber();

        float fraction = proc(userData, &hit);
        UpdateEarlyOutFraction(fraction);
        hadHit = true;
    }

    JPH_CollideShapeCollector* proc;
    void* userData;
    JPH_Bool32 hadHit = false;
	uint32_t _padding;
};

class CastShapeCollectorCallback : public CastShapeCollector
{
public:
    CastShapeCollectorCallback(JPH_CastShapeCollector* proc, void* userData) : proc(proc), userData(userData) {}

    virtual void AddHit(const ShapeCastResult& result)
    {
        JPH_ShapeCastResult hit;
        FromJolt(result.mContactPointOn1, &hit.contactPointOn1);
        FromJolt(result.mContactPointOn2, &hit.contactPointOn2);
        FromJolt(result.mPenetrationAxis, &hit.penetrationAxis);
        hit.penetrationDepth = result.mPenetrationDepth;
        hit.subShapeID1 = result.mSubShapeID1.GetValue();
        hit.subShapeID2 = result.mSubShapeID2.GetValue();
        hit.bodyID2 = result.mBodyID2.GetIndexAndSequenceNumber();
        hit.fraction = result.mFraction;
        hit.isBackFaceHit = result.mIsBackFaceHit;

        float fraction = proc(userData, &hit);
        UpdateEarlyOutFraction(fraction);
        hadHit = true;
    }

    JPH_CastShapeCollector* proc;
    void* userData;
    JPH_Bool32 hadHit = false;
	uint32_t _padding;
};

JPH_Bool32 JPH_NarrowPhaseQuery_CastRay(const JPH_NarrowPhaseQuery* query,
    const JPH_RVec3* origin, const JPH_Vec3* direction,
    JPH_RayCastResult* hit,
    JPH_BroadPhaseLayerFilter* broadPhaseLayerFilter,
    JPH_ObjectLayerFilter* objectLayerFilter,
    JPH_BodyFilter* bodyFilter)
{
    JPH_ASSERT(query && origin && direction && hit);
    auto joltQuery = reinterpret_cast<const JPH::NarrowPhaseQuery*>(query);

    JPH::RRayCast ray(ToJolt(origin), ToJolt(direction));
    RayCastResult result;

    bool hadHit = joltQuery->CastRay(
        ray,
        result,
        ToJolt(broadPhaseLayerFilter),
        ToJolt(objectLayerFilter),
        ToJolt(bodyFilter)
    );

    if (hadHit)
    {
        hit->fraction = result.mFraction;
        hit->bodyID = result.mBodyID.GetIndexAndSequenceNumber();
        hit->subShapeID2 = result.mSubShapeID2.GetValue();
    }

    return static_cast<JPH_Bool32>(hadHit);
}

JPH_Bool32 JPH_NarrowPhaseQuery_CastRay2(const JPH_NarrowPhaseQuery* query,
    const JPH_RVec3* origin, const JPH_Vec3* direction,
    JPH_CastRayCollector* callback, void* userData,
    JPH_BroadPhaseLayerFilter* broadPhaseLayerFilter,
    JPH_ObjectLayerFilter* objectLayerFilter,
    JPH_BodyFilter* bodyFilter)
{
    JPH_ASSERT(query && origin && direction && callback);
    auto joltQuery = reinterpret_cast<const JPH::NarrowPhaseQuery*>(query);

    JPH::RRayCast ray(ToJolt(origin), ToJolt(direction));
    RayCastSettings ray_settings;
    CastRayCollectorCallback collector(callback, userData);

    joltQuery->CastRay(
        ray,
        ray_settings,
        collector,
        ToJolt(broadPhaseLayerFilter),
        ToJolt(objectLayerFilter),
        ToJolt(bodyFilter)
    );

    return collector.hadHit;
}

JPH_Bool32 JPH_NarrowPhaseQuery_CollidePoint(const JPH_NarrowPhaseQuery* query,
	const JPH_RVec3* point,
	JPH_CollidePointCollector* callback, void* userData,
	JPH_BroadPhaseLayerFilter* broadPhaseLayerFilter,
	JPH_ObjectLayerFilter* objectLayerFilter,
	JPH_BodyFilter* bodyFilter)
{
    JPH_ASSERT(query && point && callback);
    auto joltQuery = reinterpret_cast<const JPH::NarrowPhaseQuery*>(query);
    auto joltPoint = ToJolt(point);

    CollidePointCollectorCallback collector(callback, userData);

    joltQuery->CollidePoint(
        joltPoint,
        collector,
        ToJolt(broadPhaseLayerFilter),
        ToJolt(objectLayerFilter),
        ToJolt(bodyFilter)
    );

    return collector.hadHit;
}

JPH_Bool32 JPH_NarrowPhaseQuery_CollideShape(const JPH_NarrowPhaseQuery* query,
    const JPH_Shape* shape, const JPH_Vec3* scale, const JPH_RMatrix4x4* centerOfMassTransform,
    JPH_RVec3* baseOffset,
    JPH_CollideShapeCollector* callback, void* userData,
    JPH_BroadPhaseLayerFilter* broadPhaseLayerFilter,
    JPH_ObjectLayerFilter* objectLayerFilter,
    JPH_BodyFilter* bodyFilter)
{
    JPH_ASSERT(query && shape && scale && centerOfMassTransform && callback);
    auto joltQuery = reinterpret_cast<const JPH::NarrowPhaseQuery*>(query);
    auto joltShape = reinterpret_cast<const JPH::Shape*>(shape);
    auto joltScale = ToJolt(scale);
    auto joltTransform = ToJolt(*centerOfMassTransform);

    CollideShapeSettings settings;
    settings.mActiveEdgeMode = EActiveEdgeMode::CollideWithAll;

    auto joltBaseOffset = ToJolt(baseOffset);

    CollideShapeCollectorCallback collector(callback, userData);

    joltQuery->CollideShape(
        joltShape,
        joltScale,
        joltTransform,
        settings,
        joltBaseOffset,
        collector,
        ToJolt(broadPhaseLayerFilter),
        ToJolt(objectLayerFilter),
        ToJolt(bodyFilter)
    );

    return collector.hadHit;
}

JPH_Bool32 JPH_NarrowPhaseQuery_CastShape(const JPH_NarrowPhaseQuery* query,
    const JPH_Shape* shape,
    const JPH_RMatrix4x4* worldTransform, const JPH_Vec3* direction,
    JPH_RVec3* baseOffset,
    JPH_CastShapeCollector* callback, void* userData,
    JPH_BroadPhaseLayerFilter* broadPhaseLayerFilter,
    JPH_ObjectLayerFilter* objectLayerFilter,
    JPH_BodyFilter* bodyFilter)
{
    JPH_ASSERT(query && shape && worldTransform && direction && callback);
    auto joltQuery = reinterpret_cast<const JPH::NarrowPhaseQuery*>(query);
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

    auto joltBaseOffset = ToJolt(baseOffset);
    CastShapeCollectorCallback collector(callback, userData);

    joltQuery->CastShape(
        shape_cast,
        settings,
        joltBaseOffset,
        collector,
        ToJolt(broadPhaseLayerFilter),
        ToJolt(objectLayerFilter),
        ToJolt(bodyFilter)
    );

    return collector.hadHit;
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

JPH_Body* JPH_Body_GetFixedToWorldBody(void)
{
	return reinterpret_cast<JPH_Body*>(&JPH::Body::sFixedToWorld);
}

/* Contact Listener */
class ManagedContactListener final : public JPH::ContactListener
{
public:
    ValidateResult OnContactValidate(const Body& inBody1, const Body& inBody2, RVec3Arg inBaseOffset, const CollideShapeResult& inCollisionResult) override
    {
        JPH_UNUSED(inCollisionResult);
        JPH_RVec3 baseOffset;
        FromJolt(inBaseOffset, &baseOffset);

        if (procs.OnContactValidate)
        {
            JPH_ValidateResult result = procs.OnContactValidate(
                userData,
                reinterpret_cast<const JPH_Body*>(&inBody1),
                reinterpret_cast<const JPH_Body*>(&inBody2),
                &baseOffset,
                nullptr
            );

            return (JPH::ValidateResult)result;
        }

        return JPH::ValidateResult::AcceptAllContactsForThisBodyPair;
    }

    void OnContactAdded(const Body& inBody1, const Body& inBody2, const ContactManifold& inManifold, ContactSettings& ioSettings) override
    {
        JPH_UNUSED(inManifold);
        JPH_UNUSED(ioSettings);

        if (procs.OnContactAdded)
        {
            procs.OnContactAdded(
                userData,
                reinterpret_cast<const JPH_Body*>(&inBody1),
                reinterpret_cast<const JPH_Body*>(&inBody2),
                reinterpret_cast<const JPH_ContactManifold*>(&inManifold),
                reinterpret_cast<JPH_ContactSettings*>(&ioSettings)
            );
        }
    }

    void OnContactPersisted(const Body& inBody1, const Body& inBody2, const ContactManifold& inManifold, ContactSettings& ioSettings) override
    {
        JPH_UNUSED(inManifold);
        JPH_UNUSED(ioSettings);

        if (procs.OnContactPersisted)
        {
            procs.OnContactPersisted(
                userData,
                reinterpret_cast<const JPH_Body*>(&inBody1),
                reinterpret_cast<const JPH_Body*>(&inBody2),
                reinterpret_cast<const JPH_ContactManifold*>(&inManifold),
                reinterpret_cast<JPH_ContactSettings*>(&ioSettings)
            );
        }
    }

    void OnContactRemoved(const SubShapeIDPair& inSubShapePair) override
    {
        if (procs.OnContactRemoved)
        {
            procs.OnContactRemoved(
                userData,
                reinterpret_cast<const JPH_SubShapeIDPair*>(&inSubShapePair)
            );
        }
    }

    JPH_ContactListener_Procs procs = {};
    void* userData = nullptr;
};

void JPH_ContactListener_SetProcs(JPH_ContactListener* listener, JPH_ContactListener_Procs procs, void* userData)
{
    auto managedListener = reinterpret_cast<ManagedContactListener*>(listener);
    managedListener->procs = procs;
    managedListener->userData = userData;
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
class ManagedBodyActivationListener final : public JPH::BodyActivationListener
{
public:
    void OnBodyActivated(const BodyID& inBodyID, uint64 inBodyUserData) override
    {
        if (procs.OnBodyDeactivated)
        {
            procs.OnBodyActivated(
                userData,
                inBodyID.GetIndexAndSequenceNumber(),
                inBodyUserData
            );
        }
    }

    void OnBodyDeactivated(const BodyID& inBodyID, uint64 inBodyUserData) override
    {
        if (procs.OnBodyDeactivated)
        {
            procs.OnBodyDeactivated(
                userData,
                inBodyID.GetIndexAndSequenceNumber(),
                inBodyUserData
            );
        }
    }

    JPH_BodyActivationListener_Procs procs = {};
    void* userData = nullptr;
};

void JPH_BodyActivationListener_SetProcs(JPH_BodyActivationListener* listener, JPH_BodyActivationListener_Procs procs, void* userData)
{
    auto managedListener = reinterpret_cast<ManagedBodyActivationListener*>(listener);
    managedListener->procs = procs;
    managedListener->userData = userData;
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

/* ContactManifold */
void JPH_ContactManifold_GetWorldSpaceNormal(const JPH_ContactManifold* manifold, JPH_Vec3* result)
{
	FromJolt(reinterpret_cast<const JPH::ContactManifold*>(manifold)->mWorldSpaceNormal, result);
}

float JPH_ContactManifold_GetPenetrationDepth(const JPH_ContactManifold* manifold)
{
	return reinterpret_cast<const JPH::ContactManifold*>(manifold)->mPenetrationDepth;
}

JPH_SubShapeID JPH_ContactManifold_GetSubShapeID1(const JPH_ContactManifold* manifold)
{
	return reinterpret_cast<const JPH::ContactManifold*>(manifold)->mSubShapeID1.GetValue();
}

JPH_SubShapeID JPH_ContactManifold_GetSubShapeID2(const JPH_ContactManifold* manifold)
{
	return reinterpret_cast<const JPH::ContactManifold*>(manifold)->mSubShapeID2.GetValue();
}

uint32_t JPH_ContactManifold_GetPointCount(const JPH_ContactManifold* manifold)
{
	return reinterpret_cast<const JPH::ContactManifold*>(manifold)->mRelativeContactPointsOn1.size();
}

void JPH_ContactManifold_GetWorldSpaceContactPointOn1(const JPH_ContactManifold* manifold, uint32_t index, JPH_RVec3* result)
{
	FromJolt(reinterpret_cast<const JPH::ContactManifold*>(manifold)->GetWorldSpaceContactPointOn1(index), result);
}

void JPH_ContactManifold_GetWorldSpaceContactPointOn2(const JPH_ContactManifold* manifold, uint32_t index, JPH_RVec3* result)
{
	FromJolt(reinterpret_cast<const JPH::ContactManifold*>(manifold)->GetWorldSpaceContactPointOn2(index), result);
}

/* ContactSettings */
float JPH_ContactSettings_GetFriction(JPH_ContactSettings* settings)
{
	return reinterpret_cast<JPH::ContactSettings*>(settings)->mCombinedFriction;
}

void JPH_ContactSettings_SetFriction(JPH_ContactSettings* settings, float friction)
{
	reinterpret_cast<JPH::ContactSettings*>(settings)->mCombinedFriction = friction;
}

float JPH_ContactSettings_GetRestitution(JPH_ContactSettings* settings)
{
	return reinterpret_cast<JPH::ContactSettings*>(settings)->mCombinedRestitution;
}

void JPH_ContactSettings_SetRestitution(JPH_ContactSettings* settings, float restitution)
{
	reinterpret_cast<JPH::ContactSettings*>(settings)->mCombinedRestitution = restitution;
}

float JPH_ContactSettings_GetInvMassScale1(JPH_ContactSettings* settings)
{
	return reinterpret_cast<JPH::ContactSettings*>(settings)->mInvMassScale1;
}

void JPH_ContactSettings_SetInvMassScale1(JPH_ContactSettings* settings, float scale)
{
	reinterpret_cast<JPH::ContactSettings*>(settings)->mInvMassScale1 = scale;
}

float JPH_ContactSettings_GetInvInertiaScale1(JPH_ContactSettings* settings)
{
	return reinterpret_cast<JPH::ContactSettings*>(settings)->mInvInertiaScale1;
}

void JPH_ContactSettings_SetInvInertiaScale1(JPH_ContactSettings* settings, float scale)
{
	reinterpret_cast<JPH::ContactSettings*>(settings)->mInvInertiaScale1 = scale;
}

float JPH_ContactSettings_GetInvMassScale2(JPH_ContactSettings* settings)
{
	return reinterpret_cast<JPH::ContactSettings*>(settings)->mInvMassScale2;
}

void JPH_ContactSettings_SetInvMassScale2(JPH_ContactSettings* settings, float scale)
{
	reinterpret_cast<JPH::ContactSettings*>(settings)->mInvMassScale2 = scale;
}

float JPH_ContactSettings_GetInvInertiaScale2(JPH_ContactSettings* settings)
{
	return reinterpret_cast<JPH::ContactSettings*>(settings)->mInvInertiaScale2;
}

void JPH_ContactSettings_SetInvInertiaScale2(JPH_ContactSettings* settings, float scale)
{
	reinterpret_cast<JPH::ContactSettings*>(settings)->mInvInertiaScale2 = scale;
}

JPH_Bool32 JPH_ContactSettings_GetIsSensor(JPH_ContactSettings* settings)
{
	return static_cast<JPH_Bool32>(reinterpret_cast<JPH::ContactSettings*>(settings)->mIsSensor);
}

void JPH_ContactSettings_SetIsSensor(JPH_ContactSettings* settings, JPH_Bool32 sensor)
{
	reinterpret_cast<JPH::ContactSettings*>(settings)->mIsSensor = static_cast<bool>(sensor);
}

void JPH_ContactSettings_GetRelativeLinearSurfaceVelocity(JPH_ContactSettings* settings, JPH_Vec3* result)
{
	FromJolt(reinterpret_cast<JPH::ContactSettings*>(settings)->mRelativeLinearSurfaceVelocity, result);
}

void JPH_ContactSettings_SetRelativeLinearSurfaceVelocity(JPH_ContactSettings* settings, JPH_Vec3* velocity)
{
	reinterpret_cast<JPH::ContactSettings*>(settings)->mRelativeLinearSurfaceVelocity = ToJolt(velocity);
}

void JPH_ContactSettings_GetRelativeAngularSurfaceVelocity(JPH_ContactSettings* settings, JPH_Vec3* result)
{
	FromJolt(reinterpret_cast<JPH::ContactSettings*>(settings)->mRelativeAngularSurfaceVelocity, result);
}

void JPH_ContactSettings_SetRelativeAngularSurfaceVelocity(JPH_ContactSettings* settings, JPH_Vec3* velocity)
{
	reinterpret_cast<JPH::ContactSettings*>(settings)->mRelativeAngularSurfaceVelocity = ToJolt(velocity);
}

/* CharacterBaseSettings */
void JPH_CharacterBaseSettings_Destroy(JPH_CharacterBaseSettings* settings)
{
    if (settings)
    {
        auto joltSettings = reinterpret_cast<JPH::CharacterBaseSettings*>(settings);
        joltSettings->Release();
    }
}

void JPH_CharacterBaseSettings_GetUp(JPH_CharacterBaseSettings* settings, JPH_Vec3* result)
{
	FromJolt(reinterpret_cast<JPH::CharacterBaseSettings*>(settings)->mUp, result);
}

void JPH_CharacterBaseSettings_SetUp(JPH_CharacterBaseSettings* settings, const JPH_Vec3* value)
{
	auto joltSettings = reinterpret_cast<JPH::CharacterBaseSettings*>(settings);
    joltSettings->mUp = ToJolt(value);
}

void JPH_CharacterBaseSettings_GetSupportingVolume(JPH_CharacterBaseSettings* settings, JPH_Plane* result)
{
	FromJolt(reinterpret_cast<JPH::CharacterBaseSettings*>(settings)->mSupportingVolume, result);
}

void JPH_CharacterBaseSettings_SetSupportingVolume(JPH_CharacterBaseSettings* settings, const JPH_Plane* value)
{
    auto joltSettings = reinterpret_cast<JPH::CharacterBaseSettings*>(settings);
    joltSettings->mSupportingVolume = ToJolt(value);
}

float JPH_CharacterBaseSettings_GetMaxSlopeAngle(JPH_CharacterBaseSettings* settings)
{
	return reinterpret_cast<JPH::CharacterBaseSettings*>(settings)->mMaxSlopeAngle;
}

void JPH_CharacterBaseSettings_SetMaxSlopeAngle(JPH_CharacterBaseSettings* settings, float maxSlopeAngle)
{
    auto joltSettings = reinterpret_cast<JPH::CharacterBaseSettings*>(settings);
    joltSettings->mMaxSlopeAngle = maxSlopeAngle;
}

JPH_Bool32 JPH_CharacterBaseSettings_GetEnhancedInternalEdgeRemoval(JPH_CharacterBaseSettings* settings)
{
	auto joltSettings = reinterpret_cast<JPH::CharacterBaseSettings*>(settings);
    return FromJolt(joltSettings->mEnhancedInternalEdgeRemoval);
}

void JPH_CharacterBaseSettings_SetEnhancedInternalEdgeRemoval(JPH_CharacterBaseSettings* settings, JPH_Bool32 value)
{
	auto joltSettings = reinterpret_cast<JPH::CharacterBaseSettings*>(settings);
    joltSettings->mEnhancedInternalEdgeRemoval = ToJolt(value);
}

const JPH_Shape* JPH_CharacterBaseSettings_GetShape(JPH_CharacterBaseSettings* settings)
{
	auto joltSettings = reinterpret_cast<JPH::CharacterBaseSettings*>(settings);
	return reinterpret_cast<const JPH_Shape*>(joltSettings->mShape.GetPtr());
}

void JPH_CharacterBaseSettings_SetShape(JPH_CharacterBaseSettings* settings, const JPH_Shape* shape)
{
    auto joltSettings = reinterpret_cast<JPH::CharacterBaseSettings*>(settings);
    auto joltShape = reinterpret_cast<const JPH::Shape*>(shape);

    joltSettings->mShape = joltShape;
}

/* CharacterBase */
void JPH_CharacterBase_Destroy(JPH_CharacterBase* character)
{
    if (character)
    {
        auto joltCharacter = reinterpret_cast<JPH::CharacterBase*>(character);
        joltCharacter->Release();
    }
}

float JPH_CharacterBase_GetCosMaxSlopeAngle(JPH_CharacterBase* character)
{
	 auto joltCharacter = reinterpret_cast<JPH::CharacterBase*>(character);
	 return joltCharacter->GetCosMaxSlopeAngle();
}

void JPH_CharacterBase_SetMaxSlopeAngle(JPH_CharacterBase* character, float maxSlopeAngle)
{
	auto joltCharacter = reinterpret_cast<JPH::CharacterBase*>(character);
	joltCharacter->SetMaxSlopeAngle(maxSlopeAngle);
}

void JPH_CharacterBase_GetUp(JPH_CharacterBase* character, JPH_Vec3* result)
{
	FromJolt(reinterpret_cast<JPH::CharacterBase*>(character)->GetUp(), result);
}

void JPH_CharacterBase_SetUp(JPH_CharacterBase* character, const JPH_Vec3* value)
{
	auto joltCharacter = reinterpret_cast<JPH::CharacterBase*>(character);
    joltCharacter->SetUp(ToJolt(value));
}

JPH_Bool32 JPH_CharacterBase_IsSlopeTooSteep(JPH_CharacterBase* character, const JPH_Vec3* value)
{
	auto joltCharacter = reinterpret_cast<JPH::CharacterBase*>(character);
	return FromJolt(joltCharacter->IsSlopeTooSteep(ToJolt(value)));
}

const JPH_Shape* JPH_CharacterBase_GetShape(JPH_CharacterBase* character)
{
	auto joltCharacter = reinterpret_cast<JPH::CharacterBase*>(character);
	return reinterpret_cast<const JPH_Shape*>(joltCharacter->GetShape());
}

JPH_GroundState JPH_CharacterBase_GetGroundState(JPH_CharacterBase* character)
{
    auto joltCharacter = reinterpret_cast<JPH::CharacterBase*>(character);
    return static_cast<JPH_GroundState>(joltCharacter->GetGroundState());
}

JPH_Bool32 JPH_CharacterBase_IsSupported(JPH_CharacterBase* character)
{
    auto joltCharacter = reinterpret_cast<JPH::CharacterBase*>(character);
    return joltCharacter->IsSupported();
}

void JPH_CharacterBase_GetGroundPosition(JPH_CharacterBase* character, JPH_RVec3* position)
{
    auto joltCharacter = reinterpret_cast<JPH::CharacterBase*>(character);
    auto jolt_vector = joltCharacter->GetGroundPosition();
    FromJolt(jolt_vector, position);
}

void JPH_CharacterBase_GetGroundNormal(JPH_CharacterBase* character, JPH_Vec3* normal)
{
    auto joltCharacter = reinterpret_cast<JPH::CharacterBase*>(character);
    auto jolt_vector = joltCharacter->GetGroundNormal();
    FromJolt(jolt_vector, normal);
}

void JPH_CharacterBase_GetGroundVelocity(JPH_CharacterBase* character, JPH_Vec3* velocity)
{
    auto joltCharacter = reinterpret_cast<JPH::CharacterBase*>(character);
    auto jolt_vector = joltCharacter->GetGroundVelocity();
    FromJolt(jolt_vector, velocity);
}

const JPH_PhysicsMaterial* JPH_CharacterBase_GetGroundMaterial(JPH_CharacterBase* character)
{
	auto joltCharacter = reinterpret_cast<JPH::CharacterBase*>(character);
	return reinterpret_cast<const JPH_PhysicsMaterial*>(joltCharacter->GetGroundMaterial());
}

JPH_BodyID JPH_CharacterBase_GetGroundBodyId(JPH_CharacterBase* character)
{
    auto joltCharacter = reinterpret_cast<JPH::CharacterBase*>(character);
    return joltCharacter->GetGroundBodyID().GetIndexAndSequenceNumber();
}

JPH_SubShapeID JPH_CharacterBase_GetGroundSubShapeId(JPH_CharacterBase* character)
{
    auto joltCharacter = reinterpret_cast<JPH::CharacterBase*>(character);
    return joltCharacter->GetGroundSubShapeID().GetValue();
}

uint64_t JPH_CharacterBase_GetGroundUserData(JPH_CharacterBase* character)
{
	auto joltCharacter = reinterpret_cast<JPH::CharacterBase*>(character);
    return joltCharacter->GetGroundUserData();
}

/* CharacterVirtualSettings */
JPH_CharacterVirtualSettings* JPH_CharacterVirtualSettings_Create(void)
{
    auto settings = new JPH::CharacterVirtualSettings();
    settings->AddRef();

    return reinterpret_cast<JPH_CharacterVirtualSettings*>(settings);
}

float JPH_CharacterVirtualSettings_GetMass(JPH_CharacterVirtualSettings* settings)
{
	return reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mMass;
}

void JPH_CharacterVirtualSettings_SetMass(JPH_CharacterVirtualSettings* settings, float value)
{
	reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mMass = value;
}

float JPH_CharacterVirtualSettings_GetMaxStrength(JPH_CharacterVirtualSettings* settings)
{
	return reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mMaxStrength;
}

void JPH_CharacterVirtualSettings_SetMaxStrength(JPH_CharacterVirtualSettings* settings, float value)
{
	reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mMaxStrength = value;
}

void JPH_CharacterVirtualSettings_GetShapeOffset(JPH_CharacterVirtualSettings* settings, JPH_Vec3* result)
{
	FromJolt(reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mShapeOffset, result);
}

void JPH_CharacterVirtualSettings_SetShapeOffset(JPH_CharacterVirtualSettings* settings, const JPH_Vec3* value)
{
	auto joltSettings = reinterpret_cast<JPH::CharacterVirtualSettings*>(settings);
    joltSettings->mShapeOffset = ToJolt(value);
}

JPH_BackFaceMode JPH_CharacterVirtualSettings_GetBackFaceMode(JPH_CharacterVirtualSettings* settings)
{
	auto joltSettings = reinterpret_cast<JPH::CharacterVirtualSettings*>(settings);
	return static_cast<JPH_BackFaceMode>(joltSettings->mBackFaceMode);
}

void JPH_CharacterVirtualSettings_SetBackFaceMode(JPH_CharacterVirtualSettings* settings, JPH_BackFaceMode value)
{
	auto joltSettings = reinterpret_cast<JPH::CharacterVirtualSettings*>(settings);
	joltSettings->mBackFaceMode = static_cast<JPH::EBackFaceMode>(value);
}

float JPH_CharacterVirtualSettings_GetPredictiveContactDistance(JPH_CharacterVirtualSettings* settings)
{
	return reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mPredictiveContactDistance;
}

void JPH_CharacterVirtualSettings_SetPredictiveContactDistance(JPH_CharacterVirtualSettings* settings, float value)
{
	reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mPredictiveContactDistance = value;
}

uint32_t JPH_CharacterVirtualSettings_GetMaxCollisionIterations(JPH_CharacterVirtualSettings* settings)
{
	return reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mMaxCollisionIterations;
}

void JPH_CharacterVirtualSettings_SetMaxCollisionIterations(JPH_CharacterVirtualSettings* settings, uint32_t value)
{
	reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mMaxCollisionIterations = value;
}

uint32_t JPH_CharacterVirtualSettings_GetMaxConstraintIterations(JPH_CharacterVirtualSettings* settings)
{
	return reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mMaxConstraintIterations;
}

void JPH_CharacterVirtualSettings_SetMaxConstraintIterations(JPH_CharacterVirtualSettings* settings, uint32_t value)
{
	reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mMaxConstraintIterations = value;
}

float JPH_CharacterVirtualSettings_GetMinTimeRemaining(JPH_CharacterVirtualSettings* settings)
{
	return reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mMinTimeRemaining;
}

void JPH_CharacterVirtualSettings_SetMinTimeRemaining(JPH_CharacterVirtualSettings* settings, float value)
{
	reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mMinTimeRemaining = value;
}

float JPH_CharacterVirtualSettings_GetCollisionTolerance(JPH_CharacterVirtualSettings* settings)
{
	return reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mCollisionTolerance;
}

void JPH_CharacterVirtualSettings_SetCollisionTolerance(JPH_CharacterVirtualSettings* settings, float value)
{
	reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mCollisionTolerance = value;
}

float JPH_CharacterVirtualSettings_GetCharacterPadding(JPH_CharacterVirtualSettings* settings)
{
	return reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mCharacterPadding;
}

void JPH_CharacterVirtualSettings_SetCharacterPadding(JPH_CharacterVirtualSettings* settings, float value)
{
	reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mCharacterPadding = value;
}

uint32_t JPH_CharacterVirtualSettings_GetMaxNumHits(JPH_CharacterVirtualSettings* settings)
{
	return reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mMaxNumHits;
}

void JPH_CharacterVirtualSettings_SetMaxNumHits(JPH_CharacterVirtualSettings* settings, uint32_t value)
{
	reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mMaxNumHits = value;
}

float JPH_CharacterVirtualSettings_GetHitReductionCosMaxAngle(JPH_CharacterVirtualSettings* settings)
{
	return reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mHitReductionCosMaxAngle;
}

void JPH_CharacterVirtualSettings_SetHitReductionCosMaxAngle(JPH_CharacterVirtualSettings* settings, float value)
{
	reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mHitReductionCosMaxAngle = value;
}

float JPH_CharacterVirtualSettings_GetPenetrationRecoverySpeed(JPH_CharacterVirtualSettings* settings)
{
	return reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mPenetrationRecoverySpeed;
}

void JPH_CharacterVirtualSettings_SetPenetrationRecoverySpeed(JPH_CharacterVirtualSettings* settings, float value)
{
	reinterpret_cast<JPH::CharacterVirtualSettings*>(settings)->mPenetrationRecoverySpeed = value;
}


/* CharacterVirtual */
JPH_CharacterVirtual* JPH_CharacterVirtual_Create(const JPH_CharacterVirtualSettings* settings,
    const JPH_RVec3* position,
    const JPH_Quat* rotation,
	uint64_t userData,
    JPH_PhysicsSystem* system)
{
    auto joltSettings = reinterpret_cast<const JPH::CharacterVirtualSettings*>(settings);

    auto joltCharacter = new JPH::CharacterVirtual(joltSettings, ToJolt(position), ToJolt(rotation), userData, system->physicsSystem);
    joltCharacter->AddRef();

    return reinterpret_cast<JPH_CharacterVirtual*>(joltCharacter);
}

void JPH_CharacterVirtual_SetListener(JPH_CharacterVirtual* character, JPH_CharacterContactListener* listener)
{
	auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
	auto joltListener = reinterpret_cast<JPH::CharacterContactListener*>(listener);
	joltCharacter->SetListener(joltListener);
}

void JPH_CharacterVirtual_GetLinearVelocity(JPH_CharacterVirtual* character, JPH_Vec3* velocity)
{
    auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
    auto jolt_vector = joltCharacter->GetLinearVelocity();
    FromJolt(jolt_vector, velocity);
}

void JPH_CharacterVirtual_SetLinearVelocity(JPH_CharacterVirtual* character, const JPH_Vec3* velocity)
{
    auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
    joltCharacter->SetLinearVelocity(ToJolt(velocity));
}

void JPH_CharacterVirtual_GetPosition(JPH_CharacterVirtual* character, JPH_RVec3* position)
{
    auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
    auto jolt_vector = joltCharacter->GetPosition();
    FromJolt(jolt_vector, position);
}

void JPH_CharacterVirtual_SetPosition(JPH_CharacterVirtual* character, const JPH_RVec3* position)
{
    auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
    joltCharacter->SetPosition(ToJolt(position));
}

void JPH_CharacterVirtual_GetRotation(JPH_CharacterVirtual* character, JPH_Quat* rotation)
{
    auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
    auto jolt_quat = joltCharacter->GetRotation();
    FromJolt(jolt_quat, rotation);
}

void JPH_CharacterVirtual_SetRotation(JPH_CharacterVirtual* character, const JPH_Quat* rotation)
{
    auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
    joltCharacter->SetRotation(ToJolt(rotation));
}

void JPH_CharacterVirtual_GetWorldTransform(JPH_CharacterVirtual* character, JPH_RMatrix4x4* result)
{
    auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);

    const JPH::RMat44& mat = joltCharacter->GetWorldTransform();
    FromJolt(mat, result);
}

void JPH_CharacterVirtual_GetCenterOfMassTransform(JPH_CharacterVirtual* character, JPH_RMatrix4x4* result)
{
    auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);

    const JPH::RMat44& mat = joltCharacter->GetCenterOfMassTransform();
    FromJolt(mat, result);
}

float JPH_CharacterVirtual_GetMass(JPH_CharacterVirtual* character)
{
    auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
	return joltCharacter->GetMass();
}

void JPH_CharacterVirtual_SetMass(JPH_CharacterVirtual* character, float value)
{
    auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
	joltCharacter->SetMass(value);
}

float JPH_CharacterVirtual_GetMaxStrength(JPH_CharacterVirtual* character)
{
    auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
	return joltCharacter->GetMaxStrength();
}

void JPH_CharacterVirtual_SetMaxStrength(JPH_CharacterVirtual* character, float value)
{
    auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
	joltCharacter->SetMaxStrength(value);
}

float JPH_CharacterVirtual_GetPenetrationRecoverySpeed(JPH_CharacterVirtual* character)
{
	auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
	return joltCharacter->GetPenetrationRecoverySpeed();
}

void JPH_CharacterVirtual_SetPenetrationRecoverySpeed(JPH_CharacterVirtual* character, float value)
{
	auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
	joltCharacter->SetPenetrationRecoverySpeed(value);
}

JPH_Bool32 JPH_CharacterVirtual_GetEnhancedInternalEdgeRemoval(JPH_CharacterVirtual* character)
{
	auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
	return FromJolt(joltCharacter->GetEnhancedInternalEdgeRemoval());
}

void JPH_CharacterVirtual_SetEnhancedInternalEdgeRemoval(JPH_CharacterVirtual* character, JPH_Bool32 value)
{
	auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
	joltCharacter->SetEnhancedInternalEdgeRemoval(ToJolt(value));
}

float JPH_CharacterVirtual_GetCharacterPadding(JPH_CharacterVirtual* character)
{
	auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
	return joltCharacter->GetCharacterPadding();
}

uint32_t JPH_CharacterVirtual_GetMaxNumHits(JPH_CharacterVirtual* character)
{
	auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
	return joltCharacter->GetMaxNumHits();
}

void JPH_CharacterVirtual_SetMaxNumHits(JPH_CharacterVirtual* character, uint32_t value)
{
	auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
	joltCharacter->SetMaxNumHits(value);
}

float JPH_CharacterVirtual_GetHitReductionCosMaxAngle(JPH_CharacterVirtual* character)
{
	auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
	return joltCharacter->GetHitReductionCosMaxAngle();
}

void JPH_CharacterVirtual_SetHitReductionCosMaxAngle(JPH_CharacterVirtual* character, float value)
{
	auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
	joltCharacter->SetHitReductionCosMaxAngle(value);
}

JPH_Bool32 JPH_CharacterVirtual_GetMaxHitsExceeded(JPH_CharacterVirtual* character)
{
	auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
	return FromJolt(joltCharacter->GetMaxHitsExceeded());
}

uint64_t JPH_CharacterVirtual_GetUserData(JPH_CharacterVirtual* character)
{
	auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
	return joltCharacter->GetUserData();
}

void JPH_CharacterVirtual_SetUserData(JPH_CharacterVirtual* character, uint64_t value)
{
	auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
	joltCharacter->SetUserData(value);
}

void JPH_CharacterVirtual_CancelVelocityTowardsSteepSlopes(JPH_CharacterVirtual* character, const JPH_Vec3* desiredVelocity, JPH_Vec3* velocity)
{
	auto joltCharacter = reinterpret_cast<JPH::CharacterVirtual*>(character);
	FromJolt(joltCharacter->CancelVelocityTowardsSteepSlopes(ToJolt(desiredVelocity)), velocity);
}

void JPH_CharacterVirtual_Update(JPH_CharacterVirtual* character, float deltaTime, JPH_ObjectLayer layer, JPH_PhysicsSystem* system)
{
    auto jolt_character = reinterpret_cast<JPH::CharacterVirtual*>(character);
	auto jolt_object_layer = static_cast<JPH::ObjectLayer>(layer);

    jolt_character->Update(deltaTime,
        system->physicsSystem->GetGravity(),
        system->physicsSystem->GetDefaultBroadPhaseLayerFilter(jolt_object_layer),
        system->physicsSystem->GetDefaultLayerFilter(jolt_object_layer),
        {},
        {},
        *s_TempAllocator
    );
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

/* CharacterContactListener */
class ManagedCharacterContactListener final : public JPH::CharacterContactListener
{
public:
    void OnAdjustBodyVelocity(const CharacterVirtual *inCharacter, const Body &inBody2, Vec3 &ioLinearVelocity, Vec3 &ioAngularVelocity) override
    {
        JPH_Vec3 linearVelocity, angularVelocity;
        FromJolt(ioLinearVelocity, &linearVelocity);
		FromJolt(ioAngularVelocity, &angularVelocity);

        if (procs.OnAdjustBodyVelocity)
        {
            procs.OnAdjustBodyVelocity(
                userData,
				reinterpret_cast<const JPH_CharacterVirtual*>(inCharacter),
                reinterpret_cast<const JPH_Body*>(&inBody2),
                &linearVelocity,
                &angularVelocity
            );
        }
    }

    bool OnContactValidate(const CharacterVirtual *inCharacter, const BodyID &inBodyID2, const SubShapeID &inSubShapeID2) override
    {
        if (procs.OnContactValidate)
        {
            return procs.OnContactValidate(
                userData,
				reinterpret_cast<const JPH_CharacterVirtual*>(inCharacter),
                (JPH_BodyID)inBodyID2.GetIndexAndSequenceNumber(),
                (JPH_SubShapeID)inSubShapeID2.GetValue()
            ) == 1;
        }

		return true;
    }

    void OnContactAdded(const CharacterVirtual *inCharacter, const BodyID &inBodyID2, const SubShapeID &inSubShapeID2, RVec3Arg inContactPosition, Vec3Arg inContactNormal, CharacterContactSettings &ioSettings) override
    {
        JPH_UNUSED(ioSettings);

        if (procs.OnContactAdded)
        {
			JPH_RVec3 contactPosition;
			JPH_Vec3 contactNormal;

			FromJolt(inContactPosition, &contactPosition);
			FromJolt(inContactNormal, &contactNormal);

			JPH_CharacterContactSettings settings = {};
			settings.canPushCharacter = FromJolt(ioSettings.mCanPushCharacter);
			settings.canReceiveImpulses = FromJolt(ioSettings.mCanReceiveImpulses);

            procs.OnContactAdded(
                userData,
				reinterpret_cast<const JPH_CharacterVirtual*>(inCharacter),
                (JPH_BodyID)inBodyID2.GetIndexAndSequenceNumber(),
                (JPH_SubShapeID)inSubShapeID2.GetValue(),
                &contactPosition,
				&contactNormal,
				&settings
            );

			ioSettings.mCanPushCharacter = ToJolt(settings.canPushCharacter);
			ioSettings.mCanReceiveImpulses = ToJolt(settings.canReceiveImpulses);
        }
    }

    void OnContactSolve(const CharacterVirtual *inCharacter, const BodyID &inBodyID2, const SubShapeID &inSubShapeID2,
		RVec3Arg inContactPosition, Vec3Arg inContactNormal, Vec3Arg inContactVelocity,
		const PhysicsMaterial *inContactMaterial, Vec3Arg inCharacterVelocity,
		Vec3 &ioNewCharacterVelocity) override
    {
        if (procs.OnContactSolve)
        {
			JPH_RVec3 contactPosition;
			JPH_Vec3 contactNormal, contactVelocity, characterVelocity;

			FromJolt(inContactPosition, &contactPosition);
			FromJolt(inContactNormal, &contactNormal);
			FromJolt(inContactVelocity, &contactVelocity);
			FromJolt(inCharacterVelocity, &characterVelocity);
			JPH_Vec3 newCharacterVelocity;

            procs.OnContactSolve(
                userData,
				reinterpret_cast<const JPH_CharacterVirtual*>(inCharacter),
                (JPH_BodyID)inBodyID2.GetIndexAndSequenceNumber(),
                (JPH_SubShapeID)inSubShapeID2.GetValue(),
				&contactPosition,
				&contactNormal,
				&contactVelocity,
				reinterpret_cast<const JPH_PhysicsMaterial*>(inContactMaterial),
				&characterVelocity,
				&newCharacterVelocity
            );

			ioNewCharacterVelocity = ToJolt(&newCharacterVelocity);
        }
    }

    JPH_CharacterContactListener_Procs procs = {};
    void* userData = nullptr;
};


JPH_CharacterContactListener* JPH_CharacterContactListener_Create(JPH_CharacterContactListener_Procs procs, void* userData)
{
	auto impl = new ManagedCharacterContactListener();
	impl->procs = procs;
    impl->userData = userData;
    return reinterpret_cast<JPH_CharacterContactListener*>(impl);
}

void JPH_CharacterContactListener_Destroy(JPH_CharacterContactListener* listener)
{
	if (listener)
        delete reinterpret_cast<ManagedCharacterContactListener*>(listener);
}

JPH_SUPPRESS_WARNING_POP
