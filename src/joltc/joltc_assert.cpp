// Copyright (c) Amer Koleci and Contributors.
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
#include <Jolt/Physics/Collision/CastResult.h>
#include <Jolt/Physics/Body/BodyCreationSettings.h>
#include <Jolt/Physics/Body/BodyActivationListener.h>
#include <Jolt/Physics/Body/AllowedDOFs.h>
#include <Jolt/Physics/Constraints/SixDOFConstraint.h>
#include <Jolt/Physics/Character/CharacterBase.h>

#ifdef _MSC_VER
__pragma(warning(pop))
#endif

#ifdef JPH_COMPILER_GCC
JPH_GCC_SUPPRESS_WARNING("-Winvalid-offsetof")
#endif

#define ENSURE_SIZE_ALIGN(type0, type1) \
    static_assert(sizeof(type0) == sizeof(type1)); \
    static_assert(alignof(type0) == alignof(type1))

static_assert(sizeof(JPH::ObjectLayer) == sizeof(JPH_ObjectLayer));
static_assert(sizeof(JPH::BroadPhaseLayer) == sizeof(JPH_BroadPhaseLayer));
static_assert(sizeof(JPH::BodyID) == sizeof(JPH_BodyID));
static_assert(sizeof(JPH::SubShapeID) == sizeof(JPH_SubShapeID));

// EPhysicsUpdateError
static_assert(sizeof(JPH_PhysicsUpdateError) == sizeof(JPH::EPhysicsUpdateError));
static_assert(JPH_PhysicsUpdateError_None == (int)JPH::EPhysicsUpdateError::None);
static_assert(JPH_PhysicsUpdateError_ManifoldCacheFull == (int)JPH::EPhysicsUpdateError::ManifoldCacheFull);
static_assert(JPH_PhysicsUpdateError_BodyPairCacheFull == (int)JPH::EPhysicsUpdateError::BodyPairCacheFull);
static_assert(JPH_PhysicsUpdateError_ContactConstraintsFull == (int)JPH::EPhysicsUpdateError::ContactConstraintsFull);

// EBodyType
static_assert(JPH_BodyType_Rigid == (int)JPH::EBodyType::RigidBody);
static_assert(JPH_BodyType_Soft == (int)JPH::EBodyType::SoftBody);

// EMotionType
static_assert(JPH_MotionType_Static == (int)JPH::EMotionType::Static);
static_assert(JPH_MotionType_Kinematic == (int)JPH::EMotionType::Kinematic);
static_assert(JPH_MotionType_Dynamic == (int)JPH::EMotionType::Dynamic);

// EActivation
static_assert(sizeof(JPH::EActivation) == sizeof(JPH_Activation));
static_assert(JPH_Activation_Activate == (int)JPH::EActivation::Activate);
static_assert(JPH_Activation_DontActivate == (int)JPH::EActivation::DontActivate);

// EActivation
static_assert(sizeof(JPH::ValidateResult) == sizeof(JPH_ValidateResult));
static_assert(JPH_ValidateResult_AcceptAllContactsForThisBodyPair == (int)JPH::ValidateResult::AcceptAllContactsForThisBodyPair);
static_assert(JPH_ValidateResult_AcceptContact == (int)JPH::ValidateResult::AcceptContact);
static_assert(JPH_ValidateResult_RejectContact == (int)JPH::ValidateResult::RejectContact);
static_assert(JPH_ValidateResult_RejectAllContactsForThisBodyPair == (int)JPH::ValidateResult::RejectAllContactsForThisBodyPair);

// EActivation
static_assert(sizeof(JPH::EConstraintSpace) == sizeof(JPH_ConstraintSpace));
static_assert(JPH_ConstraintSpace_LocalToBodyCOM == (int)JPH::EConstraintSpace::LocalToBodyCOM);
static_assert(JPH_ConstraintSpace_WorldSpace == (int)JPH::EConstraintSpace::WorldSpace);

// EMotionQuality
static_assert(JPH_MotionQuality_Discrete == (int)JPH::EMotionQuality::Discrete);
static_assert(JPH_MotionQuality_LinearCast == (int)JPH::EMotionQuality::LinearCast);

// JPH_AllowedDOFs
static_assert(sizeof(JPH_AllowedDOFs) == sizeof(uint32_t));
static_assert(JPH_AllowedDOFs_All == (int)JPH::EAllowedDOFs::All);
static_assert(JPH_AllowedDOFs_TranslationX == (int)JPH::EAllowedDOFs::TranslationX);
static_assert(JPH_AllowedDOFs_TranslationY == (int)JPH::EAllowedDOFs::TranslationY);
static_assert(JPH_AllowedDOFs_TranslationZ == (int)JPH::EAllowedDOFs::TranslationZ);
static_assert(JPH_AllowedDOFs_RotationX == (int)JPH::EAllowedDOFs::RotationX);
static_assert(JPH_AllowedDOFs_RotationY == (int)JPH::EAllowedDOFs::RotationY);
static_assert(JPH_AllowedDOFs_RotationZ == (int)JPH::EAllowedDOFs::RotationZ);
static_assert(JPH_AllowedDOFs_Plane2D == (int)JPH::EAllowedDOFs::Plane2D);

// JPH_MotorState
static_assert(sizeof(JPH_MotorState) == sizeof(uint32_t));
static_assert(JPH_MotorState_Off == (int)JPH::EMotorState::Off);
static_assert(JPH_MotorState_Velocity == (int)JPH::EMotorState::Velocity);
static_assert(JPH_MotorState_Position == (int)JPH::EMotorState::Position);

// JPH_SixDOFConstraintAxis
static_assert(sizeof(JPH_SixDOFConstraintAxis) == sizeof(uint32_t));
static_assert(JPH_SixDOFConstraintAxis_TranslationX == (int)JPH::SixDOFConstraintSettings::EAxis::TranslationX);
static_assert(JPH_SixDOFConstraintAxis_TranslationY == (int)JPH::SixDOFConstraintSettings::EAxis::TranslationY);
static_assert(JPH_SixDOFConstraintAxis_TranslationZ == (int)JPH::SixDOFConstraintSettings::EAxis::TranslationZ);
static_assert(JPH_SixDOFConstraintAxis_RotationX == (int)JPH::SixDOFConstraintSettings::EAxis::RotationX);
static_assert(JPH_SixDOFConstraintAxis_RotationY == (int)JPH::SixDOFConstraintSettings::EAxis::RotationY);
static_assert(JPH_SixDOFConstraintAxis_RotationZ == (int)JPH::SixDOFConstraintSettings::EAxis::RotationZ);

// JPH_GroundState
static_assert(sizeof(JPH::CharacterBase::EGroundState) == sizeof(JPH_GroundState));
static_assert(JPH_GroundState_OnGround == (int)JPH::CharacterBase::EGroundState::OnGround);
static_assert(JPH_GroundState_OnSteepGround == (int)JPH::CharacterBase::EGroundState::OnSteepGround);
static_assert(JPH_GroundState_NotSupported == (int)JPH::CharacterBase::EGroundState::NotSupported);
static_assert(JPH_GroundState_InAir == (int)JPH::CharacterBase::EGroundState::InAir);

// JPH_StateRecorderState
static_assert(sizeof(JPH_StateRecorderState) == sizeof(uint32_t));
static_assert(JPH_StateRecorderState_None == (int)JPH::EStateRecorderState::None);
static_assert(JPH_StateRecorderState_Global == (int)JPH::EStateRecorderState::Global);
static_assert(JPH_StateRecorderState_Bodies == (int)JPH::EStateRecorderState::Bodies);
static_assert(JPH_StateRecorderState_Bodies == (int)JPH::EStateRecorderState::Bodies);
static_assert(JPH_StateRecorderState_Contacts == (int)JPH::EStateRecorderState::Contacts);
static_assert(JPH_StateRecorderState_Constraints == (int)JPH::EStateRecorderState::Constraints);
static_assert(JPH_StateRecorderState_All == (int)JPH::EStateRecorderState::All);

static_assert(sizeof(JPH::SubShapeIDPair) == sizeof(JPH_SubShapeIDPair));
static_assert(alignof(JPH::SubShapeIDPair) == alignof(JPH_SubShapeIDPair));

ENSURE_SIZE_ALIGN(JPH::RayCastResult, JPH_RayCastResult);
static_assert(offsetof(JPH::RayCastResult, mBodyID) == offsetof(JPH_RayCastResult, bodyID));
static_assert(offsetof(JPH::RayCastResult, mFraction) == offsetof(JPH_RayCastResult, fraction));
static_assert(offsetof(JPH::RayCastResult, mSubShapeID2) == offsetof(JPH_RayCastResult, subShapeID2));

//static_assert(offsetof(JPH::MassProperties, mMass) == offsetof(JPH_MassProperties, mass));
