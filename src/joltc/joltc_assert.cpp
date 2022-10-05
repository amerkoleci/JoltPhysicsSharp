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

static_assert(sizeof(JPH::ObjectLayer) == sizeof(uint16_t));

static_assert(JPH_MOTION_TYPE_STATIC == (int)JPH::EMotionType::Static);
static_assert(JPH_MOTION_TYPE_KINEMATIC == (int)JPH::EMotionType::Kinematic);
static_assert(JPH_MOTION_TYPE_DYNAMIC == (int)JPH::EMotionType::Dynamic);

static_assert(sizeof(JPH::EActivation) == sizeof(JPH_ActivationMode));

static_assert(JPH_ACTIVATION_MODE_ACTIVATE == (int)JPH::EActivation::Activate);
static_assert(JPH_ACTIVATION_MODE_DONT_ACTIVATE == (int)JPH::EActivation::DontActivate);

static_assert(sizeof(JPH::BodyID) == sizeof(JPH_BodyID));

