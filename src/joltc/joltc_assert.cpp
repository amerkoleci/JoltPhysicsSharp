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
#include <Jolt/Physics/Body/BodyCreationSettings.h>
#include <Jolt/Physics/Body/BodyActivationListener.h>

static_assert(sizeof(JPH::ObjectLayer) == sizeof(JPH_ObjectLayer));
static_assert(sizeof(JPH::BroadPhaseLayer) == sizeof(JPH_BroadPhaseLayer));
static_assert(sizeof(JPH::BodyID) == sizeof(JPH_BodyID));
static_assert(sizeof(JPH::SubShapeID) == sizeof(JPH_SubShapeID));

// EMotionType
static_assert(JPH_MOTION_TYPE_STATIC == (int)JPH::EMotionType::Static);
static_assert(JPH_MOTION_TYPE_KINEMATIC == (int)JPH::EMotionType::Kinematic);
static_assert(JPH_MOTION_TYPE_DYNAMIC == (int)JPH::EMotionType::Dynamic);

// EActivation
static_assert(sizeof(JPH::EActivation) == sizeof(JPH_ActivationMode));
static_assert(JPH_ACTIVATION_MODE_ACTIVATE == (int)JPH::EActivation::Activate);
static_assert(JPH_ACTIVATION_MODE_DONT_ACTIVATE == (int)JPH::EActivation::DontActivate);


// EActivation
static_assert(sizeof(JPH::ValidateResult) == sizeof(JPH_ValidateResult));
static_assert(JPH_VALIDATE_RESULT_ACCEPT_ALL_CONTACTS == (int)JPH::ValidateResult::AcceptAllContactsForThisBodyPair);
static_assert(JPH_VALIDATE_RESULT_ACCEPT_CONTACT == (int)JPH::ValidateResult::AcceptContact);
static_assert(JPH_VALIDATE_RESULT_REJECT_CONTACT == (int)JPH::ValidateResult::RejectContact);
static_assert(JPH_VALIDATE_RESULT_REJECT_ALL_CONTACTS == (int)JPH::ValidateResult::RejectAllContactsForThisBodyPair);

static_assert(sizeof(JPH::SubShapeIDPair) == sizeof(JPH_SubShapeIDPair));
static_assert(alignof(JPH::SubShapeIDPair) == alignof(JPH_SubShapeIDPair));
