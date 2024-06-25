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

// EShapeType
static_assert(JPH_ShapeType_Convex == (int)JPH::EShapeType::Convex);
static_assert(JPH_ShapeType_Compound == (int)JPH::EShapeType::Compound);
static_assert(JPH_ShapeType_Decorated == (int)JPH::EShapeType::Decorated);
static_assert(JPH_ShapeType_Mesh == (int)JPH::EShapeType::Mesh);
static_assert(JPH_ShapeType_HeightField == (int)JPH::EShapeType::HeightField);
static_assert(JPH_ShapeType_SoftBody == (int)JPH::EShapeType::SoftBody);
static_assert(JPH_ShapeType_User1 == (int)JPH::EShapeType::User1);
static_assert(JPH_ShapeType_User2 == (int)JPH::EShapeType::User2);
static_assert(JPH_ShapeType_User3 == (int)JPH::EShapeType::User3);
static_assert(JPH_ShapeType_User4 == (int)JPH::EShapeType::User4);

// EShapeSubType
static_assert(JPH_ShapeSubType_Sphere == (int)JPH::EShapeSubType::Sphere);
static_assert(JPH_ShapeSubType_Box == (int)JPH::EShapeSubType::Box);
static_assert(JPH_ShapeSubType_Triangle == (int)JPH::EShapeSubType::Triangle);
static_assert(JPH_ShapeSubType_Capsule == (int)JPH::EShapeSubType::Capsule);
static_assert(JPH_ShapeSubType_TaperedCapsule == (int)JPH::EShapeSubType::TaperedCapsule);
static_assert(JPH_ShapeSubType_Cylinder == (int)JPH::EShapeSubType::Cylinder);
static_assert(JPH_ShapeSubType_ConvexHull == (int)JPH::EShapeSubType::ConvexHull);
static_assert(JPH_ShapeSubType_StaticCompound == (int)JPH::EShapeSubType::StaticCompound);
static_assert(JPH_ShapeSubType_MutableCompound == (int)JPH::EShapeSubType::MutableCompound);
static_assert(JPH_ShapeSubType_RotatedTranslated == (int)JPH::EShapeSubType::RotatedTranslated);
static_assert(JPH_ShapeSubType_Scaled == (int)JPH::EShapeSubType::Scaled);
static_assert(JPH_ShapeSubType_OffsetCenterOfMass == (int)JPH::EShapeSubType::OffsetCenterOfMass);
static_assert(JPH_ShapeSubType_Mesh == (int)JPH::EShapeSubType::Mesh);
static_assert(JPH_ShapeSubType_HeightField == (int)JPH::EShapeSubType::HeightField);
static_assert(JPH_ShapeSubType_SoftBody == (int)JPH::EShapeSubType::SoftBody);

// EConstraintType
static_assert(JPH_ConstraintType_Constraint == (int)JPH::EConstraintType::Constraint);
static_assert(JPH_ConstraintType_TwoBodyConstraint == (int)JPH::EConstraintType::TwoBodyConstraint);

// EConstraintSubType
static_assert(JPH_ConstraintSubType_Fixed == (int)JPH::EConstraintSubType::Fixed);
static_assert(JPH_ConstraintSubType_Point == (int)JPH::EConstraintSubType::Point);
static_assert(JPH_ConstraintSubType_Hinge == (int)JPH::EConstraintSubType::Hinge);
static_assert(JPH_ConstraintSubType_Slider == (int)JPH::EConstraintSubType::Slider);
static_assert(JPH_ConstraintSubType_Distance == (int)JPH::EConstraintSubType::Distance);
static_assert(JPH_ConstraintSubType_Cone == (int)JPH::EConstraintSubType::Cone);
static_assert(JPH_ConstraintSubType_SwingTwist == (int)JPH::EConstraintSubType::SwingTwist);
static_assert(JPH_ConstraintSubType_SixDOF == (int)JPH::EConstraintSubType::SixDOF);
static_assert(JPH_ConstraintSubType_Path  == (int)JPH::EConstraintSubType::Path);
static_assert(JPH_ConstraintSubType_Vehicle == (int)JPH::EConstraintSubType::Vehicle);
static_assert(JPH_ConstraintSubType_RackAndPinion == (int)JPH::EConstraintSubType::RackAndPinion);
static_assert(JPH_ConstraintSubType_Gear == (int)JPH::EConstraintSubType::Gear);
static_assert(JPH_ConstraintSubType_Pulley == (int)JPH::EConstraintSubType::Pulley);

static_assert(JPH_ConstraintSubType_User1 == (int)JPH::EConstraintSubType::User1);
static_assert(JPH_ConstraintSubType_User2 == (int)JPH::EConstraintSubType::User2);
static_assert(JPH_ConstraintSubType_User3 == (int)JPH::EConstraintSubType::User3);
static_assert(JPH_ConstraintSubType_User4 == (int)JPH::EConstraintSubType::User4);

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

// JPH_SpringMode
static_assert(sizeof(JPH_SpringMode) == sizeof(uint32_t));
static_assert(JPH_SpringMode_FrequencyAndDamping == (int)JPH::ESpringMode::FrequencyAndDamping);
static_assert(JPH_SpringMode_StiffnessAndDamping == (int)JPH::ESpringMode::StiffnessAndDamping);

// EGroundState
static_assert(sizeof(JPH::CharacterBase::EGroundState) == sizeof(JPH_GroundState));
static_assert(JPH_GroundState_OnGround == (int)JPH::CharacterBase::EGroundState::OnGround);
static_assert(JPH_GroundState_OnSteepGround == (int)JPH::CharacterBase::EGroundState::OnSteepGround);
static_assert(JPH_GroundState_NotSupported == (int)JPH::CharacterBase::EGroundState::NotSupported);
static_assert(JPH_GroundState_InAir == (int)JPH::CharacterBase::EGroundState::InAir);

// EBackFaceMode
static_assert(JPH_BackFaceMode_IgnoreBackFaces == (int)JPH::EBackFaceMode::IgnoreBackFaces);
static_assert(JPH_BackFaceMode_CollideWithBackFaces == (int)JPH::EBackFaceMode::CollideWithBackFaces);

static_assert(sizeof(JPH::SubShapeIDPair) == sizeof(JPH_SubShapeIDPair));
static_assert(alignof(JPH::SubShapeIDPair) == alignof(JPH_SubShapeIDPair));

//static_assert(offsetof(JPH::MassProperties, mMass) == offsetof(JPH_MassProperties, mass));
