// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

#ifndef JOLT_C_H_
#define JOLT_C_H_ 1

#if defined(JPH_SHARED_LIBRARY_BUILD)
#   if defined(_MSC_VER)
#       define _JPH_EXPORT __declspec(dllexport)
#   elif defined(__GNUC__)
#       define _JPH_EXPORT __attribute__((visibility("default")))
#   else
#       define _JPH_EXPORT
#       pragma warning "Unknown dynamic link import/export semantics."
#   endif
#elif defined(VIMAGE_SHARED_LIBRARY_INCLUDE)
#   if defined(_MSC_VER)
#       define _JPH_EXPORT __declspec(dllimport)
#   else
#       define _JPH_EXPORT
#   endif
#else
#   define _JPH_EXPORT
#endif

#ifdef __cplusplus
#    define _JPH_EXTERN extern "C"
#else
#    define _JPH_EXTERN extern
#endif

#ifdef _WIN32
#   define JPH_API_CALL __cdecl
#else
#   define JPH_API_CALL
#endif

#define JPH_CAPI _JPH_EXTERN _JPH_EXPORT

#include <stddef.h>
#include <stdint.h>

typedef uint32_t JPH_Bool32;
typedef uint32_t JPH_BodyID;
typedef uint32_t JPH_SubShapeID;
typedef uint16_t JPH_ObjectLayer;
typedef uint8_t  JPH_BroadPhaseLayer;

typedef enum JPH_PhysicsUpdateError {
    JPH_PhysicsUpdateError_None = 0,
    JPH_PhysicsUpdateError_ManifoldCacheFull = 1 << 0,
    JPH_PhysicsUpdateError_BodyPairCacheFull = 1 << 1,
    JPH_PhysicsUpdateError_ContactConstraintsFull = 1 << 2,

    _JPH_PhysicsUpdateError_Count,
    _JPH_PhysicsUpdateError_Force32 = 0x7fffffff
} JPH_PhysicsUpdateError;

typedef enum JPH_BodyType {
    JPH_BodyType_Rigid = 0,
    JPH_BodyType_Soft = 1,

    _JPH_BodyType_Count,
    _JPH_BodyType_Force32 = 0x7fffffff
} JPH_BodyType;

typedef enum JPH_MotionType {
    JPH_MotionType_Static = 0,
    JPH_MotionType_Kinematic = 1,
    JPH_MotionType_Dynamic = 2,

    _JPH_MotionType_Count,
    _JPH_MotionType_Force32 = 0x7fffffff
} JPH_MotionType;

typedef enum JPH_Activation
{
    JPH_Activation_Activate = 0,
    JPH_Activation_DontActivate = 1,

    _JPH_Activation_Count,
    _JPH_Activation_Force32 = 0x7fffffff
} JPH_Activation;

typedef enum JPH_ValidateResult {
    JPH_ValidateResult_AcceptAllContactsForThisBodyPair = 0,
    JPH_ValidateResult_AcceptContact = 1,
    JPH_ValidateResult_RejectContact = 2,
    JPH_ValidateResult_RejectAllContactsForThisBodyPair = 3,

    _JPH_ValidateResult_Count,
    _JPH_ValidateResult_Force32 = 0x7fffffff
} JPH_ValidateResult;

typedef enum JPH_ShapeType {
	JPH_ShapeType_Convex = 0,
	JPH_ShapeType_Compound = 1,
	JPH_ShapeType_Decorated = 2,
	JPH_ShapeType_Mesh = 3,
	JPH_ShapeType_HeightField = 4,
	JPH_ShapeType_SoftBody = 5,

	JPH_ShapeType_User1 = 6,
	JPH_ShapeType_User2 = 7,
	JPH_ShapeType_User3 = 8,
	JPH_ShapeType_User4 = 9,

    _JPH_ShapeType_Count,
    _JPH_ShapeType_Force32 = 0x7fffffff
} JPH_ShapeType;

typedef enum JPH_ShapeSubType {
	JPH_ShapeSubType_Sphere = 0,
	JPH_ShapeSubType_Box = 1,
	JPH_ShapeSubType_Triangle = 2,
	JPH_ShapeSubType_Capsule = 3,
	JPH_ShapeSubType_TaperedCapsule = 4,
	JPH_ShapeSubType_Cylinder = 5,
	JPH_ShapeSubType_ConvexHull = 6,
	JPH_ShapeSubType_StaticCompound = 7,
	JPH_ShapeSubType_MutableCompound = 8,
	JPH_ShapeSubType_RotatedTranslated = 9,
	JPH_ShapeSubType_Scaled = 10,
	JPH_ShapeSubType_OffsetCenterOfMass = 11,
	JPH_ShapeSubType_Mesh = 12,
	JPH_ShapeSubType_HeightField = 13,
	JPH_ShapeSubType_SoftBody = 14,

    _JPH_ShapeSubType_Count,
    _JPH_ShapeSubType_Force32 = 0x7fffffff
} JPH_ShapeSubType;

typedef enum JPH_ConstraintType {
	JPH_ConstraintType_Constraint = 0,
	JPH_ConstraintType_TwoBodyConstraint = 1,

    _JPH_ConstraintType_Count,
    _JPH_ConstraintType_Force32 = 0x7fffffff
} JPH_ConstraintType;

typedef enum JPH_ConstraintSubType {
	JPH_ConstraintSubType_Fixed = 0,
	JPH_ConstraintSubType_Point = 1,
	JPH_ConstraintSubType_Hinge = 2,
	JPH_ConstraintSubType_Slider = 3,
	JPH_ConstraintSubType_Distance = 4,
	JPH_ConstraintSubType_Cone = 5,
	JPH_ConstraintSubType_SwingTwist = 6,
	JPH_ConstraintSubType_SixDOF = 7,
	JPH_ConstraintSubType_Path = 8,
	JPH_ConstraintSubType_Vehicle = 9,
	JPH_ConstraintSubType_RackAndPinion = 10,
	JPH_ConstraintSubType_Gear = 11,
	JPH_ConstraintSubType_Pulley = 12,

	JPH_ConstraintSubType_User1 = 13,
	JPH_ConstraintSubType_User2 = 14,
	JPH_ConstraintSubType_User3 = 15,
	JPH_ConstraintSubType_User4 = 16,

    _JPH_ConstraintSubType_Count,
    _JPH_ConstraintSubType_Force32 = 0x7fffffff
} JPH_ConstraintSubType;

typedef enum JPH_ConstraintSpace {
    JPH_ConstraintSpace_LocalToBodyCOM = 0,
    JPH_ConstraintSpace_WorldSpace = 1,

    _JPH_ConstraintSpace_Count,
    _JPH_ConstraintSpace_Force32 = 0x7fffffff
} JPH_ConstraintSpace;

typedef enum JPH_MotionQuality {
    JPH_MotionQuality_Discrete = 0,
    JPH_MotionQuality_LinearCast = 1,

    _JPH_MotionQuality_Count,
    _JPH_MotionQuality_Force32 = 0x7fffffff
} JPH_MotionQuality;

typedef enum JPH_AllowedDOFs {
    JPH_AllowedDOFs_All = 0b111111,
    JPH_AllowedDOFs_TranslationX = 0b000001,
    JPH_AllowedDOFs_TranslationY = 0b000010,
    JPH_AllowedDOFs_TranslationZ = 0b000100,
    JPH_AllowedDOFs_RotationX = 0b001000,
    JPH_AllowedDOFs_RotationY = 0b010000,
    JPH_AllowedDOFs_RotationZ = 0b100000,
    JPH_AllowedDOFs_Plane2D = JPH_AllowedDOFs_TranslationX | JPH_AllowedDOFs_TranslationY | JPH_AllowedDOFs_RotationZ,

    _JPH_AllowedDOFs_Count,
    _JPH_AllowedDOFs_Force32 = 0x7FFFFFFF
} JPH_AllowedDOFs;

typedef enum JPH_GroundState {
    JPH_GroundState_OnGround = 0,
    JPH_GroundState_OnSteepGround = 1,
    JPH_GroundState_NotSupported = 2,
    JPH_GroundState_InAir = 3,

    _JPH_GroundState_Count,
    _JPH_GroundState_Force32 = 0x7FFFFFFF
} JPH_GroundState;

typedef enum JPH_BackFaceMode {
	JPH_BackFaceMode_IgnoreBackFaces,
	JPH_BackFaceMode_CollideWithBackFaces,

	_JPH_BackFaceMode_Count,
    _JPH_BackFaceMode_Force32 = 0x7FFFFFFF
} JPH_BackFaceMode;

typedef enum JPH_MotorState {
    JPH_MotorState_Off = 0,
    JPH_MotorState_Velocity = 1,
    JPH_MotorState_Position = 2,

    _JPH_MotorState_Count,
    _JPH_MotorState_Force32 = 0x7FFFFFFF
} JPH_MotorState;

typedef enum JPH_SixDOFConstraintAxis {
    JPH_SixDOFConstraintAxis_TranslationX,
	JPH_SixDOFConstraintAxis_TranslationY,
	JPH_SixDOFConstraintAxis_TranslationZ,

	JPH_SixDOFConstraintAxis_RotationX,
	JPH_SixDOFConstraintAxis_RotationY,
	JPH_SixDOFConstraintAxis_RotationZ,

    _JPH_SixDOFConstraintAxis_Count,
    _JPH_SixDOFConstraintAxis_Force32 = 0x7FFFFFFF
} JPH_SixDOFConstraintAxis;

typedef enum JPH_SpringMode {
	JPH_SpringMode_FrequencyAndDamping = 0,
	JPH_SpringMode_StiffnessAndDamping = 1,

	_JPH_SpringMode_Count,
    _JPH_SpringMode_Force32 = 0x7FFFFFFF
} JPH_SpringMode;

typedef struct JPH_Vec3 {
    float x;
    float y;
    float z;
} JPH_Vec3;

typedef struct JPH_Vec4 {
    float x;
    float y;
    float z;
    float w;
} JPH_Vec4;

typedef struct JPH_Quat {
    float x;
    float y;
    float z;
    float w;
} JPH_Quat;

typedef struct JPH_Plane {
	JPH_Vec3 normal;
	float distance;
} JPH_Plane;

typedef struct JPH_Matrix4x4 {
    float m11, m12, m13, m14;
    float m21, m22, m23, m24;
    float m31, m32, m33, m34;
    float m41, m42, m43, m44;
} JPH_Matrix4x4;

#if defined(JPH_DOUBLE_PRECISION)
typedef struct JPH_RVec3 {
    double x;
    double y;
    double z;
} JPH_RVec3;

typedef struct JPH_RMatrix4x4 {
    float m11, m12, m13, m14;
    float m21, m22, m23, m24;
    float m31, m32, m33, m34;
    double m41, m42, m43, m44;
} JPH_RMatrix4x4;
#else
typedef JPH_Vec3 JPH_RVec3;
typedef JPH_Matrix4x4 JPH_RMatrix4x4;
#endif

typedef struct JPH_AABox {
    JPH_Vec3 min;
    JPH_Vec3 max;
} JPH_AABox;

typedef struct JPH_Triangle {
    JPH_Vec3 v1;
    JPH_Vec3 v2;
    JPH_Vec3 v3;
    uint32_t materialIndex;
} JPH_Triangle;

typedef struct JPH_IndexedTriangleNoMaterial {
    uint32_t i1;
    uint32_t i2;
    uint32_t i3;
} JPH_IndexedTriangleNoMaterial;

typedef struct JPH_IndexedTriangle {
    uint32_t i1;
    uint32_t i2;
    uint32_t i3;
    uint32_t materialIndex;
    uint32_t userData;
} JPH_IndexedTriangle;

typedef struct JPH_MassProperties {
    float mass;
    JPH_Matrix4x4 inertia;
} JPH_MassProperties;

typedef struct JPH_SpringSettings {
	JPH_SpringMode mode;
    float frequencyOrStiffness;
    float damping;
} JPH_SpringSettings;

typedef struct JPH_MotorSettings {
    JPH_SpringSettings springSettings;
    float minForceLimit;
    float maxForceLimit;
    float minTorqueLimit;
    float maxTorqueLimit;
} JPH_MotorSettings;

typedef struct JPH_SubShapeIDPair {
    JPH_BodyID     Body1ID;
    JPH_SubShapeID subShapeID1;
    JPH_BodyID     Body2ID;
    JPH_SubShapeID subShapeID2;
} JPH_SubShapeIDPair;

typedef struct JPH_BroadPhaseCastResult {
    JPH_BodyID     bodyID;
    float          fraction;
} JPH_BroadPhaseCastResult;

typedef struct JPH_RayCastResult {
    JPH_BodyID     bodyID;
    float          fraction;
    JPH_SubShapeID subShapeID2;
} JPH_RayCastResult;

typedef struct JPH_CollidePointResult {
	JPH_BodyID bodyID;
	JPH_SubShapeID subShapeID2;
} JPH_CollidePointResult;

typedef struct JPH_CollideShapeResult
{
    JPH_Vec3           contactPointOn1;
    JPH_Vec3           contactPointOn2;
    JPH_Vec3           penetrationAxis;
    float              penetrationDepth;
    JPH_SubShapeID     subShapeID1;
    JPH_SubShapeID     subShapeID2;
    JPH_BodyID         bodyID2;
} JPH_CollideShapeResult;

typedef struct JPH_ShapeCastResult
{
    JPH_Vec3           contactPointOn1;
    JPH_Vec3           contactPointOn2;
    JPH_Vec3           penetrationAxis;
    float              penetrationDepth;
    JPH_SubShapeID     subShapeID1;
    JPH_SubShapeID     subShapeID2;
    JPH_BodyID         bodyID2;
    float              fraction;
    JPH_Bool32         isBackFaceHit;
} JPH_ShapeCastResult;

typedef float JPH_RayCastBodyCollector(void* context, JPH_BroadPhaseCastResult* result);
typedef void JPH_CollideShapeBodyCollector(void* context, JPH_BodyID result);

typedef float JPH_CastRayCollector(void* context, JPH_RayCastResult* result);
typedef float JPH_CollidePointCollector(void* context, JPH_CollidePointResult* result);
typedef float JPH_CollideShapeCollector(void* context, JPH_CollideShapeResult* result);
typedef float JPH_CastShapeCollector(void* context, JPH_ShapeCastResult* result);

typedef struct JPH_BroadPhaseLayerInterface			JPH_BroadPhaseLayerInterface;
typedef struct JPH_ObjectVsBroadPhaseLayerFilter	JPH_ObjectVsBroadPhaseLayerFilter;
typedef struct JPH_ObjectLayerPairFilter			JPH_ObjectLayerPairFilter;

typedef struct JPH_BroadPhaseLayerFilter            JPH_BroadPhaseLayerFilter;
typedef struct JPH_ObjectLayerFilter                JPH_ObjectLayerFilter;
typedef struct JPH_BodyFilter                       JPH_BodyFilter;

typedef struct JPH_PhysicsSystem                    JPH_PhysicsSystem;

typedef struct JPH_PhysicsMaterial					JPH_PhysicsMaterial;

typedef struct JPH_ShapeSettings                    JPH_ShapeSettings;
typedef struct JPH_ConvexShapeSettings			    JPH_ConvexShapeSettings;
typedef struct JPH_SphereShapeSettings              JPH_SphereShapeSettings;
typedef struct JPH_BoxShapeSettings                 JPH_BoxShapeSettings;
typedef struct JPH_PlaneShapeSettings               JPH_PlaneShapeSettings;
typedef struct JPH_TriangleShapeSettings            JPH_TriangleShapeSettings;
typedef struct JPH_CapsuleShapeSettings             JPH_CapsuleShapeSettings;
typedef struct JPH_TaperedCapsuleShapeSettings      JPH_TaperedCapsuleShapeSettings;
typedef struct JPH_CylinderShapeSettings            JPH_CylinderShapeSettings;
typedef struct JPH_TaperedCylinderShapeSettings     JPH_TaperedCylinderShapeSettings;
typedef struct JPH_ConvexHullShapeSettings          JPH_ConvexHullShapeSettings;
typedef struct JPH_CompoundShapeSettings            JPH_CompoundShapeSettings;
typedef struct JPH_StaticCompoundShapeSettings      JPH_StaticCompoundShapeSettings;
typedef struct JPH_MutableCompoundShapeSettings     JPH_MutableCompoundShapeSettings;
typedef struct JPH_MeshShapeSettings                JPH_MeshShapeSettings;
typedef struct JPH_HeightFieldShapeSettings         JPH_HeightFieldShapeSettings;
typedef struct JPH_RotatedTranslatedShapeSettings   JPH_RotatedTranslatedShapeSettings;
typedef struct JPH_OffsetCenterOfMassShapeSettings  JPH_OffsetCenterOfMassShapeSettings;

typedef struct JPH_Shape                        JPH_Shape;
typedef struct JPH_ConvexShape                  JPH_ConvexShape;
typedef struct JPH_SphereShape                  JPH_SphereShape;
typedef struct JPH_BoxShape                     JPH_BoxShape;
typedef struct JPH_PlaneShape                   JPH_PlaneShape;
typedef struct JPH_CapsuleShape                 JPH_CapsuleShape;
typedef struct JPH_CylinderShape                JPH_CylinderShape;
typedef struct JPH_TaperedCylinderShape         JPH_TaperedCylinderShape;
typedef struct JPH_TriangleShape				JPH_TriangleShape;
typedef struct JPH_TaperedCapsuleShape			JPH_TaperedCapsuleShape;
typedef struct JPH_ConvexHullShape              JPH_ConvexHullShape;
typedef struct JPH_CompoundShape                JPH_CompoundShape;
typedef struct JPH_StaticCompoundShape          JPH_StaticCompoundShape;
typedef struct JPH_MutableCompoundShape         JPH_MutableCompoundShape;
typedef struct JPH_MeshShape                    JPH_MeshShape;
typedef struct JPH_HeightFieldShape             JPH_HeightFieldShape;
typedef struct JPH_DecoratedShape               JPH_DecoratedShape;
typedef struct JPH_RotatedTranslatedShape       JPH_RotatedTranslatedShape;
typedef struct JPH_OffsetCenterOfMassShape      JPH_OffsetCenterOfMassShape;

typedef struct JPH_BodyCreationSettings         JPH_BodyCreationSettings;
typedef struct JPH_SoftBodyCreationSettings     JPH_SoftBodyCreationSettings;
typedef struct JPH_BodyInterface                JPH_BodyInterface;
typedef struct JPH_BodyLockInterface            JPH_BodyLockInterface;
typedef struct JPH_BroadPhaseQuery              JPH_BroadPhaseQuery;
typedef struct JPH_NarrowPhaseQuery             JPH_NarrowPhaseQuery;
typedef struct JPH_MotionProperties             JPH_MotionProperties;
typedef struct JPH_MassProperties               JPH_MassProperties;
typedef struct JPH_Body                         JPH_Body;

typedef struct JPH_ConstraintSettings				JPH_ConstraintSettings;
typedef struct JPH_FixedConstraintSettings			JPH_FixedConstraintSettings;
typedef struct JPH_TwoBodyConstraintSettings		JPH_TwoBodyConstraintSettings;
typedef struct JPH_FixedConstraintSettings			JPH_FixedConstraintSettings;
typedef struct JPH_DistanceConstraintSettings		JPH_DistanceConstraintSettings;
typedef struct JPH_HingeConstraintSettings			JPH_HingeConstraintSettings;
typedef struct JPH_SliderConstraintSettings			JPH_SliderConstraintSettings;
typedef struct JPH_PointConstraintSettings			JPH_PointConstraintSettings;
typedef struct JPH_ConeConstraintSettings			JPH_ConeConstraintSettings;
typedef struct JPH_SwingTwistConstraintSettings     JPH_SwingTwistConstraintSettings;
typedef struct JPH_SixDOFConstraintSettings			JPH_SixDOFConstraintSettings;

typedef struct JPH_Constraint                   JPH_Constraint;
typedef struct JPH_TwoBodyConstraint            JPH_TwoBodyConstraint;
typedef struct JPH_FixedConstraint              JPH_FixedConstraint;
typedef struct JPH_DistanceConstraint           JPH_DistanceConstraint;
typedef struct JPH_PointConstraint              JPH_PointConstraint;
typedef struct JPH_HingeConstraint              JPH_HingeConstraint;
typedef struct JPH_SliderConstraint             JPH_SliderConstraint;
typedef struct JPH_ConeConstraint               JPH_ConeConstraint;
typedef struct JPH_SwingTwistConstraint         JPH_SwingTwistConstraint;
typedef struct JPH_SixDOFConstraint				JPH_SixDOFConstraint;

typedef struct JPH_AllHit_CastRayCollector      JPH_AllHit_CastRayCollector;
typedef struct JPH_AllHit_CastShapeCollector    JPH_AllHit_CastShapeCollector;
typedef struct JPH_ShapeCastSettings            JPH_ShapeCastSettings;

typedef struct JPH_CollideShapeResult           JPH_CollideShapeResult;
typedef struct JPH_ContactListener              JPH_ContactListener;
typedef struct JPH_ContactManifold              JPH_ContactManifold;
typedef struct JPH_ContactSettings              JPH_ContactSettings;

typedef struct JPH_BodyActivationListener       JPH_BodyActivationListener;

typedef struct JPH_SharedMutex                  JPH_SharedMutex;

typedef struct JPH_BodyLockRead
{
    const JPH_BodyLockInterface* lockInterface;
    JPH_SharedMutex* mutex;
    const JPH_Body* body;
} JPH_BodyLockRead;

typedef struct JPH_BodyLockWrite
{
    const JPH_BodyLockInterface* lockInterface;
    JPH_SharedMutex* mutex;
    JPH_Body* body;
} JPH_BodyLockWrite;


typedef struct JPH_ExtendedUpdateSettings  {
	JPH_Vec3	stickToFloorStepDown;
	JPH_Vec3	walkStairsStepUp;
	float		walkStairsMinStepForward ;
	float		walkStairsStepForwardTest;
	float		walkStairsCosAngleForwardContact ;
	JPH_Vec3	walkStairsStepDownExtra;
} JPH_ExtendedUpdateSettings;

/* CharacterBase */
typedef struct JPH_CharacterBaseSettings            JPH_CharacterBaseSettings;
typedef struct JPH_CharacterBase                    JPH_CharacterBase;

/* CharacterVirtual */
typedef struct JPH_CharacterContactSettings {
	JPH_Bool32 canPushCharacter;
	JPH_Bool32 canReceiveImpulses;
} JPH_CharacterContactSettings;

typedef struct JPH_CharacterContactListener			JPH_CharacterContactListener;
typedef struct JPH_CharacterVirtualSettings         JPH_CharacterVirtualSettings; /* Inherics JPH_CharacterBaseSettings */
typedef struct JPH_CharacterVirtual                 JPH_CharacterVirtual;  /* Inherics JPH_CharacterBase */

typedef void(JPH_API_CALL* JPH_TraceFunc)(const char* mssage);
typedef JPH_Bool32(JPH_API_CALL* JPH_AssertFailureFunc)(const char* expression, const char* mssage, const char* file, uint32_t line);

JPH_CAPI JPH_Bool32 JPH_Init(void);
JPH_CAPI void JPH_Shutdown(void);
JPH_CAPI void JPH_SetTraceHandler(JPH_TraceFunc handler);
JPH_CAPI void JPH_SetAssertFailureHandler(JPH_AssertFailureFunc handler);

/* JPH_BroadPhaseLayerInterface */
JPH_CAPI JPH_BroadPhaseLayerInterface* JPH_BroadPhaseLayerInterfaceMask_Create(uint32_t numBroadPhaseLayers);
JPH_CAPI void JPH_BroadPhaseLayerInterfaceMask_ConfigureLayer(JPH_BroadPhaseLayerInterface* bpInterface, JPH_BroadPhaseLayer broadPhaseLayer, uint32_t groupsToInclude, uint32_t groupsToExclude);

JPH_CAPI JPH_BroadPhaseLayerInterface* JPH_BroadPhaseLayerInterfaceTable_Create(uint32_t numObjectLayers, uint32_t numBroadPhaseLayers);
JPH_CAPI void JPH_BroadPhaseLayerInterfaceTable_MapObjectToBroadPhaseLayer(JPH_BroadPhaseLayerInterface* bpInterface, JPH_ObjectLayer objectLayer, JPH_BroadPhaseLayer broadPhaseLayer);

/* JPH_ObjectLayerPairFilter */
JPH_CAPI JPH_ObjectLayerPairFilter* JPH_ObjectLayerPairFilterMask_Create(void);
JPH_CAPI JPH_ObjectLayer JPH_ObjectLayerPairFilterMask_GetObjectLayer(uint32_t group, uint32_t mask);
JPH_CAPI uint32_t JPH_ObjectLayerPairFilterMask_GetGroup(JPH_ObjectLayer layer);
JPH_CAPI uint32_t JPH_ObjectLayerPairFilterMask_GetMask(JPH_ObjectLayer layer);

JPH_CAPI JPH_ObjectLayerPairFilter* JPH_ObjectLayerPairFilterTable_Create(uint32_t numObjectLayers);
JPH_CAPI void JPH_ObjectLayerPairFilterTable_DisableCollision(JPH_ObjectLayerPairFilter* objectFilter, JPH_ObjectLayer layer1, JPH_ObjectLayer layer2);
JPH_CAPI void JPH_ObjectLayerPairFilterTable_EnableCollision(JPH_ObjectLayerPairFilter* objectFilter, JPH_ObjectLayer layer1, JPH_ObjectLayer layer2);
JPH_CAPI JPH_Bool32 JPH_ObjectLayerPairFilterTable_ShouldCollide(JPH_ObjectLayerPairFilter* objectFilter, JPH_ObjectLayer layer1, JPH_ObjectLayer layer2);

/* JPH_ObjectVsBroadPhaseLayerFilter */
JPH_CAPI JPH_ObjectVsBroadPhaseLayerFilter* JPH_ObjectVsBroadPhaseLayerFilterMask_Create(const JPH_BroadPhaseLayerInterface* broadPhaseLayerInterface);

JPH_CAPI JPH_ObjectVsBroadPhaseLayerFilter* JPH_ObjectVsBroadPhaseLayerFilterTable_Create(
	JPH_BroadPhaseLayerInterface* broadPhaseLayerInterface, uint32_t numBroadPhaseLayers,
	JPH_ObjectLayerPairFilter* objectLayerPairFilter, uint32_t numObjectLayers);

/* JPH_PhysicsSystem */
typedef struct JPH_PhysicsSystemSettings {
	uint32_t maxBodies; /* 10240 */
	uint32_t numBodyMutexes; /* 0 */
	uint32_t maxBodyPairs; /* 65536 */
	uint32_t maxContactConstraints; /* 10240 */
	uint32_t _padding;
	JPH_BroadPhaseLayerInterface* broadPhaseLayerInterface;
	JPH_ObjectLayerPairFilter*	objectLayerPairFilter;
	JPH_ObjectVsBroadPhaseLayerFilter* objectVsBroadPhaseLayerFilter;
} JPH_PhysicsSystemSettings;

typedef struct JPH_PhysicsSettings {
	int maxInFlightBodyPairs;
	int stepListenersBatchSize;
	int stepListenerBatchesPerJob;
	float baumgarte;
	float speculativeContactDistance;
	float penetrationSlop;
	float linearCastThreshold;
	float linearCastMaxPenetration;
	float manifoldToleranceSq;
	float maxPenetrationDistance;
	float bodyPairCacheMaxDeltaPositionSq;
	float bodyPairCacheCosMaxDeltaRotationDiv2;
	float contactNormalCosMaxDeltaRotation;
	float contactPointPreserveLambdaMaxDistSq;
	uint32_t numVelocitySteps;
	uint32_t numPositionSteps;
	float minVelocityForRestitution;
	float timeBeforeSleep;
	float pointVelocitySleepThreshold;
	JPH_Bool32 deterministicSimulation;
	JPH_Bool32 constraintWarmStart;
	JPH_Bool32 useBodyPairContactCache;
	JPH_Bool32 useManifoldReduction;
	JPH_Bool32 useLargeIslandSplitter;
	JPH_Bool32 allowSleeping;
	JPH_Bool32 checkActiveEdges;
} JPH_PhysicsSettings;

JPH_CAPI JPH_PhysicsSystem* JPH_PhysicsSystem_Create(const JPH_PhysicsSystemSettings* settings);
JPH_CAPI void JPH_PhysicsSystem_Destroy(JPH_PhysicsSystem* system);

JPH_CAPI void JPH_PhysicsSystem_SetPhysicsSettings(JPH_PhysicsSystem* system, JPH_PhysicsSettings* settings);
JPH_CAPI void JPH_PhysicsSystem_GetPhysicsSettings(JPH_PhysicsSystem* system, JPH_PhysicsSettings* result);

JPH_CAPI void JPH_PhysicsSystem_OptimizeBroadPhase(JPH_PhysicsSystem* system);
JPH_CAPI JPH_PhysicsUpdateError JPH_PhysicsSystem_Step(JPH_PhysicsSystem* system, float deltaTime, int collisionSteps);

JPH_CAPI JPH_BodyInterface* JPH_PhysicsSystem_GetBodyInterface(JPH_PhysicsSystem* system);
JPH_CAPI JPH_BodyInterface* JPH_PhysicsSystem_GetBodyInterfaceNoLock(JPH_PhysicsSystem* system);

JPH_CAPI const JPH_BodyLockInterface* JPH_PhysicsSystem_GetBodyLockInterface(const JPH_PhysicsSystem* system);
JPH_CAPI const JPH_BodyLockInterface* JPH_PhysicsSystem_GetBodyLockInterfaceNoLock(const JPH_PhysicsSystem* system);

JPH_CAPI const JPH_BroadPhaseQuery* JPH_PhysicsSystem_GetBroadPhaseQuery(const JPH_PhysicsSystem* system);

JPH_CAPI const JPH_NarrowPhaseQuery* JPH_PhysicsSystem_GetNarrowPhaseQuery(const JPH_PhysicsSystem* system);
JPH_CAPI const JPH_NarrowPhaseQuery* JPH_PhysicsSystem_GetNarrowPhaseQueryNoLock(const JPH_PhysicsSystem* system);

JPH_CAPI void JPH_PhysicsSystem_SetContactListener(JPH_PhysicsSystem* system, JPH_ContactListener* listener);
JPH_CAPI void JPH_PhysicsSystem_SetBodyActivationListener(JPH_PhysicsSystem* system, JPH_BodyActivationListener* listener);

JPH_CAPI JPH_Bool32 JPH_PhysicsSystem_WereBodiesInContact(const JPH_PhysicsSystem* system, JPH_BodyID body1, JPH_BodyID body2);

JPH_CAPI uint32_t JPH_PhysicsSystem_GetNumBodies(const JPH_PhysicsSystem* system);
JPH_CAPI uint32_t JPH_PhysicsSystem_GetNumActiveBodies(const JPH_PhysicsSystem* system, JPH_BodyType type);
JPH_CAPI uint32_t JPH_PhysicsSystem_GetMaxBodies(const JPH_PhysicsSystem* system);
JPH_CAPI uint32_t JPH_PhysicsSystem_GetNumConstraints(const JPH_PhysicsSystem* system);

JPH_CAPI void JPH_PhysicsSystem_SetGravity(JPH_PhysicsSystem* system, const JPH_Vec3* value);
JPH_CAPI void JPH_PhysicsSystem_GetGravity(JPH_PhysicsSystem* system, JPH_Vec3* result);

JPH_CAPI void JPH_PhysicsSystem_AddConstraint(JPH_PhysicsSystem* system, JPH_Constraint* constraint);
JPH_CAPI void JPH_PhysicsSystem_RemoveConstraint(JPH_PhysicsSystem* system, JPH_Constraint* constraint);

JPH_CAPI void JPH_PhysicsSystem_AddConstraints(JPH_PhysicsSystem* system, JPH_Constraint** constraints, uint32_t count);
JPH_CAPI void JPH_PhysicsSystem_RemoveConstraints(JPH_PhysicsSystem* system, JPH_Constraint** constraints, uint32_t count);

JPH_CAPI void JPH_PhysicsSystem_GetBodies(const JPH_PhysicsSystem* system, JPH_BodyID* ids, uint32_t count);
JPH_CAPI void JPH_PhysicsSystem_GetConstraints(const JPH_PhysicsSystem* system, const JPH_Constraint** constraints, uint32_t count);

/* Math */
JPH_CAPI void JPH_Quaternion_FromTo(const JPH_Vec3* from, const JPH_Vec3* to, JPH_Quat* quat);

/* Material */
JPH_CAPI JPH_PhysicsMaterial* JPH_PhysicsMaterial_Create(void);
JPH_CAPI void JPH_PhysicsMaterial_Destroy(JPH_PhysicsMaterial* material);

/* JPH_ShapeSettings */
JPH_CAPI void JPH_ShapeSettings_Destroy(JPH_ShapeSettings* settings);

/* JPH_ConvexShape */
JPH_CAPI float JPH_ConvexShapeSettings_GetDensity(const JPH_ConvexShapeSettings* shape);
JPH_CAPI void JPH_ConvexShapeSettings_SetDensity(JPH_ConvexShapeSettings* shape, float value);
JPH_CAPI float JPH_ConvexShape_GetDensity(const JPH_ConvexShape* shape);
JPH_CAPI void JPH_ConvexShape_SetDensity(JPH_ConvexShape* shape, float inDensity);

/* BoxShape */
JPH_CAPI JPH_BoxShapeSettings* JPH_BoxShapeSettings_Create(const JPH_Vec3* halfExtent, float convexRadius);
JPH_CAPI JPH_BoxShape* JPH_BoxShapeSettings_CreateShape(const JPH_BoxShapeSettings* settings);
JPH_CAPI JPH_BoxShape* JPH_BoxShape_Create(const JPH_Vec3* halfExtent, float convexRadius);
JPH_CAPI void JPH_BoxShape_GetHalfExtent(const JPH_BoxShape* shape, JPH_Vec3* halfExtent);
JPH_CAPI float JPH_BoxShape_GetVolume(const JPH_BoxShape* shape);
JPH_CAPI float JPH_BoxShape_GetConvexRadius(const JPH_BoxShape* shape);

/* SphereShape */
JPH_CAPI JPH_SphereShapeSettings* JPH_SphereShapeSettings_Create(float radius);
JPH_CAPI JPH_SphereShape* JPH_SphereShapeSettings_CreateShape(const JPH_SphereShapeSettings* settings);
JPH_CAPI float JPH_SphereShapeSettings_GetRadius(const JPH_SphereShapeSettings* settings);
JPH_CAPI void JPH_SphereShapeSettings_SetRadius(JPH_SphereShapeSettings* settings, float radius);
JPH_CAPI JPH_SphereShape* JPH_SphereShape_Create(float radius);
JPH_CAPI float JPH_SphereShape_GetRadius(const JPH_SphereShape* shape);

/* PlaneShape */
JPH_CAPI JPH_PlaneShapeSettings* JPH_PlaneShapeSettings_Create(const JPH_Plane* plane, const JPH_PhysicsMaterial* material, float halfExtent);
JPH_CAPI JPH_PlaneShape* JPH_PlaneShapeSettings_CreateShape(const JPH_PlaneShapeSettings* settings);
JPH_CAPI JPH_PlaneShape* JPH_PlaneShape_Create(const JPH_Plane* plane, const JPH_PhysicsMaterial* material, float halfExtent);
JPH_CAPI void JPH_PlaneShape_GetPlane(const JPH_PlaneShape* shape, JPH_Plane* result);
JPH_CAPI float JPH_PlaneShape_GetHalfExtent(const JPH_PlaneShape* shape);

/* TriangleShape */
JPH_CAPI JPH_TriangleShapeSettings* JPH_TriangleShapeSettings_Create(const JPH_Vec3* v1, const JPH_Vec3* v2, const JPH_Vec3* v3, float convexRadius);
JPH_CAPI JPH_TriangleShape* JPH_TriangleShapeSettings_CreateShape(const JPH_TriangleShapeSettings* settings);

JPH_CAPI JPH_TriangleShape* JPH_TriangleShape_Create(const JPH_Vec3* v1, const JPH_Vec3* v2, const JPH_Vec3* v3, float convexRadius);
JPH_CAPI float JPH_TriangleShape_GetConvexRadius(const JPH_TriangleShape* shape);

/* CapsuleShape */
JPH_CAPI JPH_CapsuleShapeSettings* JPH_CapsuleShapeSettings_Create(float halfHeightOfCylinder, float radius);
JPH_CAPI JPH_CapsuleShape* JPH_CapsuleShapeSettings_CreateShape(const JPH_CapsuleShapeSettings* settings);
JPH_CAPI JPH_CapsuleShape* JPH_CapsuleShape_Create(float halfHeightOfCylinder, float radius);
JPH_CAPI float JPH_CapsuleShape_GetRadius(const JPH_CapsuleShape* shape);
JPH_CAPI float JPH_CapsuleShape_GetHalfHeightOfCylinder(const JPH_CapsuleShape* shape);

/* CylinderShape */
JPH_CAPI JPH_CylinderShapeSettings* JPH_CylinderShapeSettings_Create(float halfHeight, float radius, float convexRadius);
JPH_CAPI JPH_CylinderShape* JPH_CylinderShapeSettings_CreateShape(const JPH_CylinderShapeSettings* settings);

JPH_CAPI JPH_CylinderShape* JPH_CylinderShape_Create(float halfHeight, float radius);
JPH_CAPI float JPH_CylinderShape_GetRadius(const JPH_CylinderShape* shape);
JPH_CAPI float JPH_CylinderShape_GetHalfHeight(const JPH_CylinderShape* shape);

/* TaperedCylinderShape */
JPH_CAPI JPH_TaperedCylinderShapeSettings* JPH_TaperedCylinderShapeSettings_Create(float halfHeightOfTaperedCylinder, float topRadius, float bottomRadius, float convexRadius/* = cDefaultConvexRadius*/, const JPH_PhysicsMaterial* material /* = NULL*/);
JPH_CAPI JPH_TaperedCylinderShape* JPH_TaperedCylinderShapeSettings_CreateShape(const JPH_TaperedCylinderShapeSettings* settings);
/* ConvexHullShape */
JPH_CAPI JPH_ConvexHullShapeSettings* JPH_ConvexHullShapeSettings_Create(const JPH_Vec3* points, uint32_t pointsCount, float maxConvexRadius);
JPH_CAPI JPH_ConvexHullShape* JPH_ConvexHullShapeSettings_CreateShape(const JPH_ConvexHullShapeSettings* settings);
JPH_CAPI uint32_t JPH_ConvexHullShape_GetNumPoints(const JPH_ConvexHullShape* shape);
JPH_CAPI void JPH_ConvexHullShape_GetPoint(const JPH_ConvexHullShape* shape, uint32_t index, JPH_Vec3* result);
JPH_CAPI uint32_t JPH_ConvexHullShape_GetNumFaces(const JPH_ConvexHullShape* shape);
JPH_CAPI uint32_t JPH_ConvexHullShape_GetNumVerticesInFace(const JPH_ConvexHullShape* shape, uint32_t faceIndex);
JPH_CAPI uint32_t JPH_ConvexHullShape_GetFaceVertices(const JPH_ConvexHullShape* shape, uint32_t faceIndex, uint32_t maxVertices, uint32_t* vertices);

/* MeshShape */
JPH_CAPI JPH_MeshShapeSettings* JPH_MeshShapeSettings_Create(const JPH_Triangle* triangles, uint32_t triangleCount);
JPH_CAPI JPH_MeshShapeSettings* JPH_MeshShapeSettings_Create2(const JPH_Vec3* vertices, uint32_t verticesCount, const JPH_IndexedTriangle* triangles, uint32_t triangleCount);
JPH_CAPI void JPH_MeshShapeSettings_Sanitize(JPH_MeshShapeSettings* settings);
JPH_CAPI JPH_MeshShape* JPH_MeshShapeSettings_CreateShape(const JPH_MeshShapeSettings* settings);

/* HeightFieldShape */
JPH_CAPI JPH_HeightFieldShapeSettings* JPH_HeightFieldShapeSettings_Create(const float* samples, const JPH_Vec3* offset, const JPH_Vec3* scale, uint32_t sampleCount);
JPH_CAPI JPH_HeightFieldShape* JPH_HeightFieldShapeSettings_CreateShape(JPH_HeightFieldShapeSettings* settings);
JPH_CAPI void JPH_HeightFieldShapeSettings_DetermineMinAndMaxSample(const JPH_HeightFieldShapeSettings* settings, float* pOutMinValue, float* pOutMaxValue, float* pOutQuantizationScale);
JPH_CAPI uint32_t JPH_HeightFieldShapeSettings_CalculateBitsPerSampleForError(const JPH_HeightFieldShapeSettings* settings, float maxError);

/* TaperedCapsuleShape */
JPH_CAPI JPH_TaperedCapsuleShapeSettings* JPH_TaperedCapsuleShapeSettings_Create(float halfHeightOfTaperedCylinder, float topRadius, float bottomRadius);
JPH_CAPI JPH_TaperedCapsuleShape* JPH_TaperedCapsuleShapeSettings_CreateShape(JPH_TaperedCapsuleShapeSettings* settings);

/* CompoundShape */
JPH_CAPI void JPH_CompoundShapeSettings_AddShape(JPH_CompoundShapeSettings* settings, const JPH_Vec3* position, const JPH_Quat* rotation, const JPH_ShapeSettings* shape, uint32_t userData);
JPH_CAPI void JPH_CompoundShapeSettings_AddShape2(JPH_CompoundShapeSettings* settings, const JPH_Vec3* position, const JPH_Quat* rotation, const JPH_Shape* shape, uint32_t userData);
JPH_CAPI uint32_t JPH_CompoundShape_GetNumSubShapes(const JPH_CompoundShape* shape);
JPH_CAPI void JPH_CompoundShape_GetSubShape(const JPH_CompoundShape* shape, uint32_t index, const JPH_Shape** subShape, JPH_Vec3* positionCOM, JPH_Quat* rotation, uint32_t* userData);
JPH_CAPI uint32_t JPH_CompoundShape_GetSubShapeIndexFromID(const JPH_CompoundShape* shape, JPH_SubShapeID id, JPH_SubShapeID* remainder);

/* StaticCompoundShape */
JPH_CAPI JPH_StaticCompoundShapeSettings* JPH_StaticCompoundShapeSettings_Create(void);
JPH_CAPI JPH_StaticCompoundShape* JPH_StaticCompoundShape_Create(const JPH_StaticCompoundShapeSettings* settings);

/* MutableCompoundShape */
JPH_CAPI JPH_MutableCompoundShapeSettings* JPH_MutableCompoundShapeSettings_Create(void);
JPH_CAPI JPH_MutableCompoundShape* JPH_MutableCompoundShape_Create(const JPH_MutableCompoundShapeSettings* settings);

JPH_CAPI uint32_t JPH_MutableCompoundShape_AddShape(JPH_MutableCompoundShape* shape, const JPH_Vec3* position, const JPH_Quat* rotation, const JPH_Shape* child, uint32_t userData);
JPH_CAPI void JPH_MutableCompoundShape_RemoveShape(JPH_MutableCompoundShape* shape, uint32_t index);
JPH_CAPI void JPH_MutableCompoundShape_ModifyShape(JPH_MutableCompoundShape* shape, uint32_t index, const JPH_Vec3* position, const JPH_Quat* rotation);
JPH_CAPI void JPH_MutableCompoundShape_ModifyShape2(JPH_MutableCompoundShape* shape, uint32_t index, const JPH_Vec3* position, const JPH_Quat* rotation, const JPH_Shape* newShape);
JPH_CAPI void JPH_MutableCompoundShape_AdjustCenterOfMass(JPH_MutableCompoundShape* shape);

/* DecoratedShape */
JPH_CAPI const JPH_Shape* JPH_DecoratedShape_GetInnerShape(const JPH_DecoratedShape* shape);

/* RotatedTranslatedShape */
JPH_CAPI JPH_RotatedTranslatedShapeSettings* JPH_RotatedTranslatedShapeSettings_Create(const JPH_Vec3* position, const JPH_Quat* rotation, const JPH_ShapeSettings* shapeSettings);
JPH_CAPI JPH_RotatedTranslatedShapeSettings* JPH_RotatedTranslatedShapeSettings_Create2(const JPH_Vec3* position, const JPH_Quat* rotation, const JPH_Shape* shape);
JPH_CAPI JPH_RotatedTranslatedShape* JPH_RotatedTranslatedShapeSettings_CreateShape(const JPH_RotatedTranslatedShapeSettings* settings);
JPH_CAPI JPH_RotatedTranslatedShape* JPH_RotatedTranslatedShape_Create(const JPH_Vec3* position, const JPH_Quat* rotation, const JPH_Shape* shape);
JPH_CAPI void JPH_RotatedTranslatedShape_GetPosition(const JPH_RotatedTranslatedShape* shape, JPH_Vec3* position);
JPH_CAPI void JPH_RotatedTranslatedShape_GetRotation(const JPH_RotatedTranslatedShape* shape, JPH_Quat* rotation);

/* OffsetCenterOfMassShape */
JPH_CAPI JPH_OffsetCenterOfMassShapeSettings* JPH_OffsetCenterOfMassShapeSettings_Create(const JPH_Vec3* offset, const JPH_ShapeSettings* shapeSettings);
JPH_CAPI JPH_OffsetCenterOfMassShapeSettings* JPH_OffsetCenterOfMassShapeSettings_Create2(const JPH_Vec3* offset, const JPH_Shape* shape);
JPH_CAPI JPH_OffsetCenterOfMassShape* JPH_OffsetCenterOfMassShapeSettings_CreateShape(const JPH_OffsetCenterOfMassShapeSettings* settings);

JPH_CAPI JPH_OffsetCenterOfMassShape* JPH_OffsetCenterOfMassShape_Create(JPH_Vec3* offset, JPH_Shape* shape);
JPH_CAPI void JPH_OffsetCenterOfMassShape_GetOffset(const JPH_OffsetCenterOfMassShape* shape, JPH_Vec3* result);

/* Shape */
JPH_CAPI void JPH_Shape_Destroy(JPH_Shape* shape);
JPH_CAPI JPH_ShapeType JPH_Shape_GetType(const JPH_Shape* shape);
JPH_CAPI JPH_ShapeSubType JPH_Shape_GetSubType(const JPH_Shape* shape);
JPH_CAPI uint64_t JPH_Shape_GetUserData(const JPH_Shape* shape);
JPH_CAPI void JPH_Shape_SetUserData(JPH_Shape* shape, uint64_t userData);
JPH_CAPI JPH_Bool32 JPH_Shape_MustBeStatic(const JPH_Shape* shape);
JPH_CAPI void JPH_Shape_GetCenterOfMass(const JPH_Shape* shape, JPH_Vec3* result);
JPH_CAPI void JPH_Shape_GetLocalBounds(const JPH_Shape* shape, JPH_AABox* result);
JPH_CAPI void JPH_Shape_GetWorldSpaceBounds(const JPH_Shape* shape, JPH_RMatrix4x4* centerOfMassTransform, JPH_Vec3* scale, JPH_AABox* result);
JPH_CAPI float JPH_Shape_GetInnerRadius(const JPH_Shape* shape);
JPH_CAPI void JPH_Shape_GetMassProperties(const JPH_Shape* shape, JPH_MassProperties* result);
JPH_CAPI void JPH_Shape_GetSurfaceNormal(const JPH_Shape* shape, JPH_SubShapeID subShapeID, JPH_Vec3* localPosition, JPH_Vec3* normal);
JPH_CAPI float JPH_Shape_GetVolume(const JPH_Shape* shape);
JPH_CAPI JPH_Bool32 JPH_Shape_CastRay(const JPH_Shape* shape, const JPH_Vec3* origin, const JPH_Vec3* direction, JPH_RayCastResult* hit);
JPH_CAPI JPH_Bool32 JPH_Shape_CollidePoint(const JPH_Shape* shape, JPH_Vec3* point);

/* JPH_BodyCreationSettings */
JPH_CAPI JPH_BodyCreationSettings* JPH_BodyCreationSettings_Create(void);
JPH_CAPI JPH_BodyCreationSettings* JPH_BodyCreationSettings_Create2(JPH_ShapeSettings* settings,
    const JPH_RVec3* position,
    const JPH_Quat* rotation,
    JPH_MotionType motionType,
    JPH_ObjectLayer objectLayer);
JPH_CAPI JPH_BodyCreationSettings* JPH_BodyCreationSettings_Create3(const JPH_Shape* shape,
    const JPH_RVec3* position,
    const JPH_Quat* rotation,
    JPH_MotionType motionType,
    JPH_ObjectLayer objectLayer);
JPH_CAPI void JPH_BodyCreationSettings_Destroy(JPH_BodyCreationSettings* settings);

JPH_CAPI void JPH_BodyCreationSettings_GetLinearVelocity(JPH_BodyCreationSettings* settings, JPH_Vec3* velocity);
JPH_CAPI void JPH_BodyCreationSettings_SetLinearVelocity(JPH_BodyCreationSettings* settings, const JPH_Vec3* velocity);

JPH_CAPI void JPH_BodyCreationSettings_GetAngularVelocity(JPH_BodyCreationSettings* settings, JPH_Vec3* velocity);
JPH_CAPI void JPH_BodyCreationSettings_SetAngularVelocity(JPH_BodyCreationSettings* settings, const JPH_Vec3* velocity);

JPH_CAPI JPH_MotionType JPH_BodyCreationSettings_GetMotionType(JPH_BodyCreationSettings* settings);
JPH_CAPI void JPH_BodyCreationSettings_SetMotionType(JPH_BodyCreationSettings* settings, JPH_MotionType value);

JPH_CAPI JPH_AllowedDOFs JPH_BodyCreationSettings_GetAllowedDOFs(JPH_BodyCreationSettings* settings);
JPH_CAPI void JPH_BodyCreationSettings_SetAllowedDOFs(JPH_BodyCreationSettings* settings, JPH_AllowedDOFs value);

/* JPH_SoftBodyCreationSettings */
JPH_CAPI JPH_SoftBodyCreationSettings* JPH_SoftBodyCreationSettings_Create(void);
JPH_CAPI void JPH_SoftBodyCreationSettings_Destroy(JPH_SoftBodyCreationSettings* settings);

/* JPH_ConstraintSettings */
JPH_CAPI void JPH_ConstraintSettings_Destroy(JPH_ConstraintSettings* settings);
JPH_CAPI JPH_Bool32 JPH_ConstraintSettings_GetEnabled(JPH_ConstraintSettings* settings);
JPH_CAPI void JPH_FixedConstraintSettings_SetEnabled(JPH_ConstraintSettings* settings, JPH_Bool32 value);
JPH_CAPI uint32_t JPH_ConstraintSettings_GetConstraintPriority(JPH_ConstraintSettings* settings);
JPH_CAPI void JPH_FixedConstraintSettings_SetConstraintPriority(JPH_ConstraintSettings* settings, uint32_t value);
JPH_CAPI uint32_t JPH_ConstraintSettings_GetNumVelocityStepsOverride(JPH_ConstraintSettings* settings);
JPH_CAPI void JPH_ConstraintSettings_SetNumVelocityStepsOverride(JPH_ConstraintSettings* settings, uint32_t value);
JPH_CAPI uint32_t JPH_ConstraintSettings_GetNumPositionStepsOverride(JPH_ConstraintSettings* settings);
JPH_CAPI void JPH_ConstraintSettings_SetNumPositionStepsOverride(JPH_ConstraintSettings* settings, uint32_t value);
JPH_CAPI float JPH_ConstraintSettings_GetDrawConstraintSize(JPH_ConstraintSettings* settings);
JPH_CAPI void JPH_ConstraintSettings_SetDrawConstraintSize(JPH_ConstraintSettings* settings, float value);
JPH_CAPI uint64_t JPH_ConstraintSettings_GetUserData(JPH_ConstraintSettings* settings);
JPH_CAPI void JPH_ConstraintSettings_SetUserData(JPH_ConstraintSettings* settings, uint64_t value);

/* JPH_Constraint */
JPH_CAPI JPH_ConstraintSettings* JPH_Constraint_GetConstraintSettings(JPH_Constraint* constraint);
JPH_CAPI JPH_ConstraintType JPH_Constraint_GetType(const JPH_Constraint* constraint);
JPH_CAPI JPH_ConstraintSubType JPH_Constraint_GetSubType(const JPH_Constraint* constraint);
JPH_CAPI uint32_t JPH_Constraint_GetConstraintPriority(const JPH_Constraint* constraint);
JPH_CAPI void JPH_Constraint_SetConstraintPriority(JPH_Constraint* constraint, uint32_t priority);
JPH_CAPI JPH_Bool32 JPH_Constraint_GetEnabled(JPH_Constraint* constraint);
JPH_CAPI void JPH_Constraint_SetEnabled(JPH_Constraint* constraint, JPH_Bool32 enabled);
JPH_CAPI uint64_t JPH_Constraint_GetUserData(const JPH_Constraint* constraint);
JPH_CAPI void JPH_Constraint_SetUserData(JPH_Constraint* constraint, uint64_t userData);
JPH_CAPI void JPH_Constraint_NotifyShapeChanged(JPH_Constraint* constraint, JPH_BodyID bodyID, JPH_Vec3* deltaCOM);
JPH_CAPI void JPH_Constraint_Destroy(JPH_Constraint* constraint);

/* JPH_FixedConstraintSettings */
JPH_CAPI JPH_FixedConstraintSettings* JPH_FixedConstraintSettings_Create(void);
JPH_CAPI JPH_ConstraintSpace JPH_FixedConstraintSettings_GetSpace(JPH_FixedConstraintSettings* settings);
JPH_CAPI void JPH_FixedConstraintSettings_SetSpace(JPH_FixedConstraintSettings* settings, JPH_ConstraintSpace space);
JPH_CAPI JPH_Bool32 JPH_FixedConstraintSettings_GetAutoDetectPoint(JPH_FixedConstraintSettings* settings);
JPH_CAPI void JPH_FixedConstraintSettings_SetAutoDetectPoint(JPH_FixedConstraintSettings* settings, JPH_Bool32 value);
JPH_CAPI void JPH_FixedConstraintSettings_GetPoint1(JPH_FixedConstraintSettings* settings, JPH_RVec3* result);
JPH_CAPI void JPH_FixedConstraintSettings_SetPoint1(JPH_FixedConstraintSettings* settings, const JPH_RVec3* value);
JPH_CAPI void JPH_FixedConstraintSettings_GetAxisX1(JPH_FixedConstraintSettings* settings, JPH_Vec3* result);
JPH_CAPI void JPH_FixedConstraintSettings_SetAxisX1(JPH_FixedConstraintSettings* settings, const JPH_Vec3* value);
JPH_CAPI void JPH_FixedConstraintSettings_GetAxisY1(JPH_FixedConstraintSettings* settings, JPH_Vec3* result);
JPH_CAPI void JPH_FixedConstraintSettings_SetAxisY1(JPH_FixedConstraintSettings* settings, const JPH_Vec3* value);
JPH_CAPI void JPH_FixedConstraintSettings_GetPoint2(JPH_FixedConstraintSettings* settings, JPH_RVec3* result);
JPH_CAPI void JPH_FixedConstraintSettings_SetPoint2(JPH_FixedConstraintSettings* settings, const JPH_RVec3* value);
JPH_CAPI void JPH_FixedConstraintSettings_GetAxisX2(JPH_FixedConstraintSettings* settings, JPH_Vec3* result);
JPH_CAPI void JPH_FixedConstraintSettings_SetAxisX2(JPH_FixedConstraintSettings* settings, const JPH_Vec3* value);
JPH_CAPI void JPH_FixedConstraintSettings_GetAxisY2(JPH_FixedConstraintSettings* settings, JPH_Vec3* result);
JPH_CAPI void JPH_FixedConstraintSettings_SetAxisY2(JPH_FixedConstraintSettings* settings, const JPH_Vec3* value);
JPH_CAPI JPH_FixedConstraint* JPH_FixedConstraintSettings_CreateConstraint(JPH_FixedConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2);

/* JPH_FixedConstraint */
JPH_CAPI void JPH_FixedConstraint_GetTotalLambdaPosition(const JPH_FixedConstraint* constraint, JPH_Vec3* result);
JPH_CAPI void JPH_FixedConstraint_GetTotalLambdaRotation(const JPH_FixedConstraint* constraint, JPH_Vec3* result);

/* JPH_DistanceConstraintSettings */
JPH_CAPI JPH_DistanceConstraintSettings* JPH_DistanceConstraintSettings_Create(void);
JPH_CAPI JPH_ConstraintSpace JPH_DistanceConstraintSettings_GetSpace(JPH_DistanceConstraintSettings* settings);
JPH_CAPI void JPH_DistanceConstraintSettings_SetSpace(JPH_DistanceConstraintSettings* settings, JPH_ConstraintSpace space);
JPH_CAPI void JPH_DistanceConstraintSettings_GetPoint1(JPH_DistanceConstraintSettings* settings, JPH_RVec3* result);
JPH_CAPI void JPH_DistanceConstraintSettings_SetPoint1(JPH_DistanceConstraintSettings* settings, const JPH_RVec3* value);
JPH_CAPI void JPH_DistanceConstraintSettings_GetPoint2(JPH_DistanceConstraintSettings* settings, JPH_RVec3* result);
JPH_CAPI void JPH_DistanceConstraintSettings_SetPoint2(JPH_DistanceConstraintSettings* settings, const JPH_RVec3* value);
JPH_CAPI JPH_DistanceConstraint* JPH_DistanceConstraintSettings_CreateConstraint(JPH_DistanceConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2); // binding for DistanceConstraintSettings::Create()

/* JPH_DistanceConstraint */
JPH_CAPI void JPH_DistanceConstraint_SetDistance(JPH_DistanceConstraint* constraint, float minDistance, float maxDistance);
JPH_CAPI float JPH_DistanceConstraint_GetMinDistance(JPH_DistanceConstraint* constraint);
JPH_CAPI float JPH_DistanceConstraint_GetMaxDistance(JPH_DistanceConstraint* constraint);
JPH_CAPI void JPH_DistanceConstraint_GetLimitsSpringSettings(JPH_DistanceConstraint* constraint, JPH_SpringSettings* result);
JPH_CAPI void JPH_DistanceConstraint_SetLimitsSpringSettings(JPH_DistanceConstraint* constraint, JPH_SpringSettings* settings);
JPH_CAPI float JPH_DistanceConstraint_GetTotalLambdaPosition(const JPH_DistanceConstraint* constraint);

/* JPH_PointConstraintSettings */
JPH_CAPI JPH_PointConstraintSettings* JPH_PointConstraintSettings_Create(void);
JPH_CAPI JPH_ConstraintSpace JPH_PointConstraintSettings_GetSpace(JPH_PointConstraintSettings* settings);
JPH_CAPI void JPH_PointConstraintSettings_SetSpace(JPH_PointConstraintSettings* settings, JPH_ConstraintSpace space);
JPH_CAPI void JPH_PointConstraintSettings_GetPoint1(JPH_PointConstraintSettings* settings, JPH_RVec3* result);
JPH_CAPI void JPH_PointConstraintSettings_SetPoint1(JPH_PointConstraintSettings* settings, const JPH_RVec3* value);
JPH_CAPI void JPH_PointConstraintSettings_GetPoint2(JPH_PointConstraintSettings* settings, JPH_RVec3* result);
JPH_CAPI void JPH_PointConstraintSettings_SetPoint2(JPH_PointConstraintSettings* settings, const JPH_RVec3* value);
JPH_CAPI JPH_PointConstraint* JPH_PointConstraintSettings_CreateConstraint(JPH_PointConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2); // binding for PointConstraintSettings::Create()

/* JPH_PointConstraint */
JPH_CAPI void JPH_PointConstraint_SetPoint1(JPH_PointConstraint* constraint, JPH_ConstraintSpace space, JPH_RVec3* value);
JPH_CAPI void JPH_PointConstraint_SetPoint2(JPH_PointConstraint* constraint, JPH_ConstraintSpace space, JPH_RVec3* value);
JPH_CAPI void JPH_PointConstraint_GetTotalLambdaPosition(const JPH_PointConstraint* constraint, JPH_Vec3* result);

/* JPH_HingeConstraintSettings */
JPH_CAPI JPH_HingeConstraintSettings* JPH_HingeConstraintSettings_Create(void);
JPH_CAPI void JPH_HingeConstraintSettings_GetPoint1(JPH_HingeConstraintSettings* settings, JPH_RVec3* result);
JPH_CAPI void JPH_HingeConstraintSettings_SetPoint1(JPH_HingeConstraintSettings* settings, const JPH_RVec3* value);
JPH_CAPI void JPH_HingeConstraintSettings_GetPoint2(JPH_HingeConstraintSettings* settings, JPH_RVec3* result);
JPH_CAPI void JPH_HingeConstraintSettings_SetPoint2(JPH_HingeConstraintSettings* settings, const JPH_RVec3* value);
JPH_CAPI void JPH_HingeConstraintSettings_SetHingeAxis1(JPH_HingeConstraintSettings* settings, const JPH_Vec3* value);
JPH_CAPI void JPH_HingeConstraintSettings_GetHingeAxis1(JPH_HingeConstraintSettings* settings, JPH_Vec3* result);
JPH_CAPI void JPH_HingeConstraintSettings_SetNormalAxis1(JPH_HingeConstraintSettings* settings, const JPH_Vec3* value);
JPH_CAPI void JPH_HingeConstraintSettings_GetNormalAxis1(JPH_HingeConstraintSettings* settings, JPH_Vec3* result);
JPH_CAPI void JPH_HingeConstraintSettings_SetHingeAxis2(JPH_HingeConstraintSettings* settings, const JPH_Vec3* value);
JPH_CAPI void JPH_HingeConstraintSettings_GetHingeAxis2(JPH_HingeConstraintSettings* settings, JPH_Vec3* result);
JPH_CAPI void JPH_HingeConstraintSettings_SetNormalAxis2(JPH_HingeConstraintSettings* settings, const JPH_Vec3* value);
JPH_CAPI void JPH_HingeConstraintSettings_GetNormalAxis2(JPH_HingeConstraintSettings* settings, JPH_Vec3* result);
JPH_CAPI JPH_HingeConstraint* JPH_HingeConstraintSettings_CreateConstraint(JPH_HingeConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2); // binding for HingeConstraintSettings::Create()

/* JPH_HingeConstraint */
JPH_CAPI JPH_HingeConstraintSettings* JPH_HingeConstraint_GetSettings(JPH_HingeConstraint* constraint);
JPH_CAPI float JPH_HingeConstraint_GetCurrentAngle(JPH_HingeConstraint* constraint);
JPH_CAPI void JPH_HingeConstraint_SetMaxFrictionTorque(JPH_HingeConstraint* constraint, float frictionTorque);
JPH_CAPI float JPH_HingeConstraint_GetMaxFrictionTorque(JPH_HingeConstraint* constraint);
JPH_CAPI void JPH_HingeConstraint_SetMotorSettings(JPH_HingeConstraint* constraint, JPH_MotorSettings* settings);
JPH_CAPI void JPH_HingeConstraint_GetMotorSettings(JPH_HingeConstraint* constraint, JPH_MotorSettings* result);
JPH_CAPI void JPH_HingeConstraint_SetMotorState(JPH_HingeConstraint* constraint, JPH_MotorState state);
JPH_CAPI JPH_MotorState JPH_HingeConstraint_GetMotorState(JPH_HingeConstraint* constraint);
JPH_CAPI void JPH_HingeConstraint_SetTargetAngularVelocity(JPH_HingeConstraint* constraint, float angularVelocity);
JPH_CAPI float JPH_HingeConstraint_GetTargetAngularVelocity(JPH_HingeConstraint* constraint);
JPH_CAPI void JPH_HingeConstraint_SetTargetAngle(JPH_HingeConstraint* constraint, float angle);
JPH_CAPI float JPH_HingeConstraint_GetTargetAngle(JPH_HingeConstraint* constraint);
JPH_CAPI void JPH_HingeConstraint_SetLimits(JPH_HingeConstraint* constraint, float inLimitsMin, float inLimitsMax);
JPH_CAPI float JPH_HingeConstraint_GetLimitsMin(JPH_HingeConstraint* constraint);
JPH_CAPI float JPH_HingeConstraint_GetLimitsMax(JPH_HingeConstraint* constraint);
JPH_CAPI JPH_Bool32 JPH_HingeConstraint_HasLimits(JPH_HingeConstraint* constraint);
JPH_CAPI void JPH_HingeConstraint_GetLimitsSpringSettings(JPH_HingeConstraint* constraint, JPH_SpringSettings* result);
JPH_CAPI void JPH_HingeConstraint_SetLimitsSpringSettings(JPH_HingeConstraint* constraint, JPH_SpringSettings* settings);
JPH_CAPI void JPH_HingeConstraint_GetTotalLambdaPosition(const JPH_HingeConstraint* constraint, JPH_Vec3* result);
JPH_CAPI void JPH_HingeConstraint_GetTotalLambdaRotation(const JPH_HingeConstraint* constraint, float* x, float* y);
JPH_CAPI float JPH_HingeConstraint_GetTotalLambdaRotationLimits(const JPH_HingeConstraint* constraint);
JPH_CAPI float JPH_HingeConstraint_GetTotalLambdaMotor(const JPH_HingeConstraint* constraint);

/* JPH_SliderConstraintSettings */
JPH_CAPI JPH_SliderConstraintSettings* JPH_SliderConstraintSettings_Create(void);
JPH_CAPI void JPH_SliderConstraintSettings_SetSliderAxis(JPH_SliderConstraintSettings* settings, const JPH_Vec3* axis);
JPH_CAPI JPH_Bool32 JPH_SliderConstraintSettings_GetAutoDetectPoint(JPH_SliderConstraintSettings* settings);
JPH_CAPI void JPH_SliderConstraintSettings_SetAutoDetectPoint(JPH_SliderConstraintSettings* settings, JPH_Bool32 value);
JPH_CAPI void JPH_SliderConstraintSettings_GetPoint1(JPH_SliderConstraintSettings* settings, JPH_RVec3* result);
JPH_CAPI void JPH_SliderConstraintSettings_SetPoint1(JPH_SliderConstraintSettings* settings, const JPH_RVec3* value);
JPH_CAPI void JPH_SliderConstraintSettings_GetPoint2(JPH_SliderConstraintSettings* settings, JPH_RVec3* result);
JPH_CAPI void JPH_SliderConstraintSettings_SetPoint2(JPH_SliderConstraintSettings* settings, const JPH_RVec3* value);
JPH_CAPI void JPH_SliderConstraintSettings_SetSliderAxis1(JPH_SliderConstraintSettings* settings, const JPH_Vec3* value);
JPH_CAPI void JPH_SliderConstraintSettings_GetSliderAxis1(JPH_SliderConstraintSettings* settings, JPH_Vec3* result);
JPH_CAPI void JPH_SliderConstraintSettings_SetNormalAxis1(JPH_SliderConstraintSettings* settings, const JPH_Vec3* value);
JPH_CAPI void JPH_SliderConstraintSettings_GetNormalAxis1(JPH_SliderConstraintSettings* settings, JPH_Vec3* result);
JPH_CAPI void JPH_SliderConstraintSettings_SetSliderAxis2(JPH_SliderConstraintSettings* settings, const JPH_Vec3* value);
JPH_CAPI void JPH_SliderConstraintSettings_GetSliderAxis2(JPH_SliderConstraintSettings* settings, JPH_Vec3* result);
JPH_CAPI void JPH_SliderConstraintSettings_SetNormalAxis2(JPH_SliderConstraintSettings* settings, const JPH_Vec3* value);
JPH_CAPI void JPH_SliderConstraintSettings_GetNormalAxis2(JPH_SliderConstraintSettings* settings, JPH_Vec3* result);
JPH_CAPI JPH_SliderConstraint* JPH_SliderConstraintSettings_CreateConstraint(JPH_SliderConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2); // binding for SliderConstraintSettings::Create()

/* JPH_SliderConstraint */
JPH_CAPI JPH_SliderConstraintSettings* JPH_SliderConstraint_GetSettings(JPH_SliderConstraint* constraint);
JPH_CAPI float JPH_SliderConstraint_GetCurrentPosition(JPH_SliderConstraint* constraint);
JPH_CAPI void JPH_SliderConstraint_SetMaxFrictionForce(JPH_SliderConstraint* constraint, float frictionForce);
JPH_CAPI float JPH_SliderConstraint_GetMaxFrictionForce(JPH_SliderConstraint* constraint);
JPH_CAPI void JPH_SliderConstraint_SetMotorSettings(JPH_SliderConstraint* constraint, JPH_MotorSettings* settings);
JPH_CAPI void JPH_SliderConstraint_GetMotorSettings(JPH_SliderConstraint* constraint, JPH_MotorSettings* result);
JPH_CAPI void JPH_SliderConstraint_SetMotorState(JPH_SliderConstraint* constraint, JPH_MotorState state);
JPH_CAPI JPH_MotorState JPH_SliderConstraint_GetMotorState(JPH_SliderConstraint* constraint);
JPH_CAPI void JPH_SliderConstraint_SetTargetVelocity(JPH_SliderConstraint* constraint, float velocity);
JPH_CAPI float JPH_SliderConstraint_GetTargetVelocity(JPH_SliderConstraint* constraint);
JPH_CAPI void JPH_SliderConstraint_SetTargetPosition(JPH_SliderConstraint* constraint, float position);
JPH_CAPI float JPH_SliderConstraint_GetTargetPosition(JPH_SliderConstraint* constraint);
JPH_CAPI void JPH_SliderConstraint_SetLimits(JPH_SliderConstraint* constraint, float inLimitsMin, float inLimitsMax);
JPH_CAPI float JPH_SliderConstraint_GetLimitsMin(JPH_SliderConstraint* constraint);
JPH_CAPI float JPH_SliderConstraint_GetLimitsMax(JPH_SliderConstraint* constraint);
JPH_CAPI JPH_Bool32 JPH_SliderConstraint_HasLimits(JPH_SliderConstraint* constraint);
JPH_CAPI void JPH_SliderConstraint_GetLimitsSpringSettings(JPH_SliderConstraint* constraint, JPH_SpringSettings* result);
JPH_CAPI void JPH_SliderConstraint_SetLimitsSpringSettings(JPH_SliderConstraint* constraint, JPH_SpringSettings* settings);
JPH_CAPI void JPH_SliderConstraint_GetTotalLambdaPosition(const JPH_SliderConstraint* constraint, float* x, float* y);
JPH_CAPI float JPH_SliderConstraint_GetTotalLambdaPositionLimits(const JPH_SliderConstraint* constraint);
JPH_CAPI void JPH_SliderConstraint_GetTotalLambdaRotation(const JPH_SliderConstraint* constraint, JPH_Vec3* result);
JPH_CAPI float JPH_SliderConstraint_GetTotalLambdaMotor(const JPH_SliderConstraint* constraint);

/* JPH_ConeConstraintSettings */
JPH_CAPI JPH_ConeConstraintSettings* JPH_ConeConstraintSettings_Create(void);
JPH_CAPI void JPH_ConeConstraintSettings_GetPoint1(JPH_ConeConstraintSettings* settings, JPH_RVec3* result);
JPH_CAPI void JPH_ConeConstraintSettings_SetPoint1(JPH_ConeConstraintSettings* settings, const JPH_RVec3* value);
JPH_CAPI void JPH_ConeConstraintSettings_GetPoint2(JPH_ConeConstraintSettings* settings, JPH_RVec3* result);
JPH_CAPI void JPH_ConeConstraintSettings_SetPoint2(JPH_ConeConstraintSettings* settings, const JPH_RVec3* value);
JPH_CAPI void JPH_ConeConstraintSettings_SetTwistAxis1(JPH_ConeConstraintSettings* settings, const JPH_Vec3* value);
JPH_CAPI void JPH_ConeConstraintSettings_GetTwistAxis1(JPH_ConeConstraintSettings* settings, JPH_Vec3* result);
JPH_CAPI void JPH_ConeConstraintSettings_SetTwistAxis2(JPH_ConeConstraintSettings* settings, const JPH_Vec3* value);
JPH_CAPI void JPH_ConeConstraintSettings_GetTwistAxis2(JPH_ConeConstraintSettings* settings, JPH_Vec3* result);
JPH_CAPI void JPH_ConeConstraintSettings_SetHalfConeAngle(JPH_ConeConstraintSettings* settings, float halfConeAngle);
JPH_CAPI float JPH_ConeConstraintSettings_GetHalfConeAngle(JPH_ConeConstraintSettings* settings);
JPH_CAPI JPH_ConeConstraint* JPH_ConeConstraintSettings_CreateConstraint(JPH_ConeConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2);

/* JPH_ConeConstraint */
JPH_CAPI void JPH_ConeConstraint_SetHalfConeAngle(JPH_ConeConstraint* constraint, float halfConeAngle);
JPH_CAPI float JPH_ConeConstraint_GetCosHalfConeAngle(const JPH_ConeConstraint* constraint);
JPH_CAPI void JPH_ConeConstraint_GetTotalLambdaPosition(const JPH_ConeConstraint* constraint, JPH_Vec3* result);
JPH_CAPI float JPH_ConeConstraint_GetTotalLambdaRotation(const JPH_ConeConstraint* constraint);

/* JPH_SwingTwistConstraintSettings */
JPH_CAPI JPH_SwingTwistConstraintSettings* JPH_SwingTwistConstraintSettings_Create(void);
JPH_CAPI JPH_SwingTwistConstraint* JPH_SwingTwistConstraintSettings_CreateConstraint(JPH_SwingTwistConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2);

/* JPH_SwingTwistConstraint */
JPH_CAPI float JPH_SwingTwistConstraint_GetNormalHalfConeAngle(JPH_SwingTwistConstraint* constraint);
JPH_CAPI void JPH_SwingTwistConstraint_GetTotalLambdaPosition(const JPH_SwingTwistConstraint* constraint, JPH_Vec3* result);
JPH_CAPI float JPH_SwingTwistConstraint_GetTotalLambdaTwist(const JPH_SwingTwistConstraint* constraint);
JPH_CAPI float JPH_SwingTwistConstraint_GetTotalLambdaSwingY(const JPH_SwingTwistConstraint* constraint);
JPH_CAPI float JPH_SwingTwistConstraint_GetTotalLambdaSwingZ(const JPH_SwingTwistConstraint* constraint);
JPH_CAPI void JPH_SwingTwistConstraint_GetTotalLambdaMotor(const JPH_SwingTwistConstraint* constraint, JPH_Vec3* result);

/* JPH_SixDOFConstraintSettings */
JPH_CAPI JPH_SixDOFConstraintSettings* JPH_SixDOFConstraintSettings_Create(void);
JPH_CAPI JPH_SixDOFConstraint* JPH_SixDOFConstraintSettings_CreateConstraint(JPH_SixDOFConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2);

/* JPH_SixDOFConstraint */
JPH_CAPI float JPH_SixDOFConstraint_GetLimitsMin(JPH_SixDOFConstraint* constraint, JPH_SixDOFConstraintAxis axis);
JPH_CAPI float JPH_SixDOFConstraint_GetLimitsMax(JPH_SixDOFConstraint* constraint, JPH_SixDOFConstraintAxis axis);
JPH_CAPI void JPH_SixDOFConstraint_GetTotalLambdaPosition(const JPH_SixDOFConstraint* constraint, JPH_Vec3* result);
JPH_CAPI void JPH_SixDOFConstraint_GetTotalLambdaRotation(const JPH_SixDOFConstraint* constraint, JPH_Vec3* result);
JPH_CAPI void JPH_SixDOFConstraint_GetTotalLambdaMotorTranslation(const JPH_SixDOFConstraint* constraint, JPH_Vec3* result);
JPH_CAPI void JPH_SixDOFConstraint_GetTotalLambdaMotorRotation(const JPH_SixDOFConstraint* constraint, JPH_Vec3* result);

/* JPH_TwoBodyConstraint */
JPH_CAPI JPH_Body* JPH_TwoBodyConstraint_GetBody1(JPH_TwoBodyConstraint* constraint);
JPH_CAPI JPH_Body* JPH_TwoBodyConstraint_GetBody2(JPH_TwoBodyConstraint* constraint);
JPH_CAPI void JPH_TwoBodyConstraint_GetConstraintToBody1Matrix(JPH_TwoBodyConstraint* constraint, JPH_Matrix4x4* result);
JPH_CAPI void JPH_TwoBodyConstraint_GetConstraintToBody2Matrix(JPH_TwoBodyConstraint* constraint, JPH_Matrix4x4* result);

/* BodyInterface */
JPH_CAPI void JPH_BodyInterface_DestroyBody(JPH_BodyInterface* interface, JPH_BodyID bodyID);
JPH_CAPI JPH_BodyID JPH_BodyInterface_CreateAndAddBody(JPH_BodyInterface* interface, JPH_BodyCreationSettings* settings, JPH_Activation activationMode);
JPH_CAPI JPH_Body* JPH_BodyInterface_CreateBody(JPH_BodyInterface* interface, JPH_BodyCreationSettings* settings);
JPH_CAPI JPH_Body* JPH_BodyInterface_CreateBodyWithID(JPH_BodyInterface* interface, JPH_BodyID bodyID, JPH_BodyCreationSettings* settings);
JPH_CAPI JPH_Body* JPH_BodyInterface_CreateBodyWithoutID(JPH_BodyInterface* interface, JPH_BodyCreationSettings* settings);
JPH_CAPI void JPH_BodyInterface_DestroyBodyWithoutID(JPH_BodyInterface* interface, JPH_Body* body);
JPH_CAPI JPH_Bool32 JPH_BodyInterface_AssignBodyID(JPH_BodyInterface* interface, JPH_Body* body);
JPH_CAPI JPH_Bool32 JPH_BodyInterface_AssignBodyID2(JPH_BodyInterface* interface, JPH_Body* body, JPH_BodyID bodyID);
JPH_CAPI JPH_Body* JPH_BodyInterface_UnassignBodyID(JPH_BodyInterface* interface, JPH_BodyID bodyID);

JPH_CAPI JPH_Body* JPH_BodyInterface_CreateSoftBody(JPH_BodyInterface* interface, const JPH_SoftBodyCreationSettings* settings);
JPH_CAPI JPH_Body* JPH_BodyInterface_CreateSoftBodyWithID(JPH_BodyInterface* interface, JPH_BodyID bodyID, const JPH_SoftBodyCreationSettings* settings);
JPH_CAPI JPH_Body* JPH_BodyInterface_CreateSoftBodyWithoutID(JPH_BodyInterface* interface, const JPH_SoftBodyCreationSettings* settings);
JPH_CAPI JPH_BodyID JPH_BodyInterface_CreateAndAddSoftBody(JPH_BodyInterface* interface, const JPH_SoftBodyCreationSettings* settings, JPH_Activation activationMode);

JPH_CAPI void JPH_BodyInterface_AddBody(JPH_BodyInterface* interface, JPH_BodyID bodyID, JPH_Activation activationMode);
JPH_CAPI void JPH_BodyInterface_RemoveBody(JPH_BodyInterface* interface, JPH_BodyID bodyID);
JPH_CAPI JPH_Bool32 JPH_BodyInterface_IsActive(JPH_BodyInterface* interface, JPH_BodyID bodyID);
JPH_CAPI JPH_Bool32 JPH_BodyInterface_IsAdded(JPH_BodyInterface* interface, JPH_BodyID bodyID);
JPH_CAPI JPH_BodyType JPH_BodyInterface_GetBodyType(JPH_BodyInterface* interface, JPH_BodyID bodyID);

JPH_CAPI void JPH_BodyInterface_SetLinearVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyID, const JPH_Vec3* velocity);
JPH_CAPI void JPH_BodyInterface_GetLinearVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyID, JPH_Vec3* velocity);
JPH_CAPI void JPH_BodyInterface_GetCenterOfMassPosition(JPH_BodyInterface* interface, JPH_BodyID bodyID, JPH_RVec3* position);

JPH_CAPI JPH_MotionType JPH_BodyInterface_GetMotionType(JPH_BodyInterface* interface, JPH_BodyID bodyID);
JPH_CAPI void JPH_BodyInterface_SetMotionType(JPH_BodyInterface* interface, JPH_BodyID bodyID, JPH_MotionType motionType, JPH_Activation activationMode);

JPH_CAPI float JPH_BodyInterface_GetRestitution(const JPH_BodyInterface* interface, JPH_BodyID bodyID);
JPH_CAPI void JPH_BodyInterface_SetRestitution(JPH_BodyInterface* interface, JPH_BodyID bodyID, float restitution);

JPH_CAPI float JPH_BodyInterface_GetFriction(const JPH_BodyInterface* interface, JPH_BodyID bodyID);
JPH_CAPI void JPH_BodyInterface_SetFriction(JPH_BodyInterface* interface, JPH_BodyID bodyID, float friction);

JPH_CAPI void JPH_BodyInterface_SetPosition(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* position, JPH_Activation activationMode);
JPH_CAPI void JPH_BodyInterface_GetPosition(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* result);

JPH_CAPI void JPH_BodyInterface_SetRotation(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Quat* rotation, JPH_Activation activationMode);
JPH_CAPI void JPH_BodyInterface_GetRotation(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Quat* result);

JPH_CAPI void JPH_BodyInterface_SetPositionAndRotation(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* position, JPH_Quat* rotation, JPH_Activation activationMode);
JPH_CAPI void JPH_BodyInterface_SetPositionAndRotationWhenChanged(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* position, JPH_Quat* rotation, JPH_Activation activationMode);
JPH_CAPI void JPH_BodyInterface_GetPositionAndRotation(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* position, JPH_Quat* rotation);
JPH_CAPI void JPH_BodyInterface_SetPositionRotationAndVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* position, JPH_Quat* rotation, JPH_Vec3* linearVelocity, JPH_Vec3* angularVelocity);

JPH_CAPI const JPH_Shape* JPH_BodyInterface_GetShape(JPH_BodyInterface* interface, JPH_BodyID bodyId);
JPH_CAPI void JPH_BodyInterface_SetShape(JPH_BodyInterface* interface, JPH_BodyID bodyId, const JPH_Shape* shape, JPH_Bool32 updateMassProperties, JPH_Activation activationMode);
JPH_CAPI void JPH_BodyInterface_NotifyShapeChanged(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* previousCenterOfMass, JPH_Bool32 updateMassProperties, JPH_Activation activationMode);

JPH_CAPI void JPH_BodyInterface_ActivateBody(JPH_BodyInterface* interface, JPH_BodyID bodyId);
JPH_CAPI void JPH_BodyInterface_DeactivateBody(JPH_BodyInterface* interface, JPH_BodyID bodyId);

JPH_CAPI void JPH_BodyInterface_SetObjectLayer(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_ObjectLayer layer);
JPH_CAPI JPH_ObjectLayer JPH_BodyInterface_GetObjectLayer(JPH_BodyInterface* interface, JPH_BodyID bodyId);

JPH_CAPI void JPH_BodyInterface_GetWorldTransform(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RMatrix4x4* result);
JPH_CAPI void JPH_BodyInterface_GetCenterOfMassTransform(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RMatrix4x4* resutlt);

JPH_CAPI void JPH_BodyInterface_MoveKinematic(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* targetPosition, JPH_Quat* targetRotation, float deltaTime);

JPH_CAPI void JPH_BodyInterface_SetLinearAndAngularVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* linearVelocity, JPH_Vec3* angularVelocity);
JPH_CAPI void JPH_BodyInterface_GetLinearAndAngularVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* linearVelocity, JPH_Vec3* angularVelocity);

JPH_CAPI void JPH_BodyInterface_AddLinearVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* linearVelocity);
JPH_CAPI void JPH_BodyInterface_AddLinearAndAngularVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* linearVelocity, JPH_Vec3* angularVelocity);

JPH_CAPI void JPH_BodyInterface_SetAngularVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* angularVelocity);
JPH_CAPI void JPH_BodyInterface_GetAngularVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* angularVelocity);

JPH_CAPI void JPH_BodyInterface_GetPointVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* point, JPH_Vec3* velocity);

JPH_CAPI void JPH_BodyInterface_AddForce(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* force);
JPH_CAPI void JPH_BodyInterface_AddForce2(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* force, JPH_RVec3* point);
JPH_CAPI void JPH_BodyInterface_AddTorque(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* torque);
JPH_CAPI void JPH_BodyInterface_AddForceAndTorque(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* force, JPH_Vec3* torque);

JPH_CAPI void JPH_BodyInterface_AddImpulse(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* impulse);
JPH_CAPI void JPH_BodyInterface_AddImpulse2(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* impulse, JPH_RVec3* point);
JPH_CAPI void JPH_BodyInterface_AddAngularImpulse(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Vec3* angularImpulse);

JPH_CAPI void JPH_BodyInterface_SetMotionQuality(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_MotionQuality quality);
JPH_CAPI JPH_MotionQuality JPH_BodyInterface_GetMotionQuality(JPH_BodyInterface* interface, JPH_BodyID bodyId);

JPH_CAPI void JPH_BodyInterface_GetInverseInertia(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_Matrix4x4* result);

JPH_CAPI void JPH_BodyInterface_SetGravityFactor(JPH_BodyInterface* interface, JPH_BodyID bodyId, float gravityFactor);
JPH_CAPI float JPH_BodyInterface_GetGravityFactor(JPH_BodyInterface* interface, JPH_BodyID bodyId);

JPH_CAPI void JPH_BodyInterface_InvalidateContactCache(JPH_BodyInterface* interface, JPH_BodyID bodyId);
JPH_CAPI void JPH_BodyInterface_SetUserData(JPH_BodyInterface* interface, JPH_BodyID bodyId, uint64_t inUserData);
JPH_CAPI uint64_t JPH_BodyInterface_GetUserData(JPH_BodyInterface* interface, JPH_BodyID bodyId);
//--------------------------------------------------------------------------------------------------
// JPH_BodyLockInterface
//--------------------------------------------------------------------------------------------------
JPH_CAPI void JPH_BodyLockInterface_LockRead(const JPH_BodyLockInterface* lockInterface, JPH_BodyID bodyID, JPH_BodyLockRead* outLock);
JPH_CAPI void JPH_BodyLockInterface_UnlockRead(const JPH_BodyLockInterface* lockInterface, JPH_BodyLockRead* ioLock);

JPH_CAPI void JPH_BodyLockInterface_LockWrite(const JPH_BodyLockInterface* lockInterface, JPH_BodyID bodyID, JPH_BodyLockWrite* outLock);
JPH_CAPI void JPH_BodyLockInterface_UnlockWrite(const JPH_BodyLockInterface* lockInterface, JPH_BodyLockWrite* ioLock);

//--------------------------------------------------------------------------------------------------
// JPH_MotionProperties
//--------------------------------------------------------------------------------------------------
JPH_CAPI JPH_AllowedDOFs JPH_MotionProperties_GetAllowedDOFs(const JPH_MotionProperties* properties);
JPH_CAPI void JPH_MotionProperties_SetLinearDamping(JPH_MotionProperties* properties, float damping);
JPH_CAPI float JPH_MotionProperties_GetLinearDamping(const JPH_MotionProperties* properties);
JPH_CAPI void JPH_MotionProperties_SetAngularDamping(JPH_MotionProperties* properties, float damping);
JPH_CAPI float JPH_MotionProperties_GetAngularDamping(const JPH_MotionProperties* properties);
JPH_CAPI void JPH_MotionProperties_SetMassProperties(JPH_MotionProperties* properties, JPH_AllowedDOFs allowedDOFs, const JPH_MassProperties* massProperties);
JPH_CAPI float JPH_MotionProperties_GetInverseMassUnchecked(JPH_MotionProperties* properties);
JPH_CAPI void JPH_MotionProperties_SetInverseMass(JPH_MotionProperties* properties, float inverseMass);
JPH_CAPI void JPH_MotionProperties_GetInverseInertiaDiagonal(JPH_MotionProperties* properties, JPH_Vec3* result);
JPH_CAPI void JPH_MotionProperties_GetInertiaRotation(JPH_MotionProperties* properties, JPH_Quat* result);
JPH_CAPI void JPH_MotionProperties_SetInverseInertia(JPH_MotionProperties* properties, JPH_Vec3* diagonal, JPH_Quat* rot);

//--------------------------------------------------------------------------------------------------
// JPH_MassProperties
//--------------------------------------------------------------------------------------------------
JPH_CAPI void JPH_MassProperties_DecomposePrincipalMomentsOfInertia(JPH_MassProperties* properties, JPH_Matrix4x4* rotation, JPH_Vec3* diagonal);
JPH_CAPI void JPH_MassProperties_ScaleToMass(JPH_MassProperties* properties, float mass);

//--------------------------------------------------------------------------------------------------
// JPH_BroadPhaseQuery
//--------------------------------------------------------------------------------------------------
JPH_CAPI JPH_Bool32 JPH_BroadPhaseQuery_CastRay(const JPH_BroadPhaseQuery* query,
    const JPH_Vec3* origin, const JPH_Vec3* direction,
    JPH_RayCastBodyCollector* callback, void* userData,
    JPH_BroadPhaseLayerFilter* broadPhaseLayerFilter,
    JPH_ObjectLayerFilter* objectLayerFilter);

JPH_CAPI JPH_Bool32 JPH_BroadPhaseQuery_CollideAABox(const JPH_BroadPhaseQuery* query,
    const JPH_AABox* box, JPH_CollideShapeBodyCollector* callback, void* userData,
    JPH_BroadPhaseLayerFilter* broadPhaseLayerFilter,
    JPH_ObjectLayerFilter* objectLayerFilter);

JPH_CAPI JPH_Bool32 JPH_BroadPhaseQuery_CollideSphere(const JPH_BroadPhaseQuery* query,
    const JPH_Vec3* center, float radius, JPH_CollideShapeBodyCollector* callback, void* userData,
    JPH_BroadPhaseLayerFilter* broadPhaseLayerFilter,
    JPH_ObjectLayerFilter* objectLayerFilter);

JPH_CAPI JPH_Bool32 JPH_BroadPhaseQuery_CollidePoint(const JPH_BroadPhaseQuery* query,
    const JPH_Vec3* point, JPH_CollideShapeBodyCollector* callback, void* userData,
    JPH_BroadPhaseLayerFilter* broadPhaseLayerFilter,
    JPH_ObjectLayerFilter* objectLayerFilter);

//--------------------------------------------------------------------------------------------------
// JPH_NarrowPhaseQuery
//--------------------------------------------------------------------------------------------------
JPH_CAPI JPH_Bool32 JPH_NarrowPhaseQuery_CastRay(const JPH_NarrowPhaseQuery* query,
    const JPH_RVec3* origin, const JPH_Vec3* direction,
    JPH_RayCastResult* hit,
	JPH_BroadPhaseLayerFilter* broadPhaseLayerFilter,
	JPH_ObjectLayerFilter* objectLayerFilter,
	JPH_BodyFilter* bodyFilter);

JPH_CAPI JPH_Bool32 JPH_NarrowPhaseQuery_CastRay2(const JPH_NarrowPhaseQuery* query,
	const JPH_RVec3* origin, const JPH_Vec3* direction,
	JPH_CastRayCollector* callback, void* userData,
	JPH_BroadPhaseLayerFilter* broadPhaseLayerFilter,
	JPH_ObjectLayerFilter* objectLayerFilter,
	JPH_BodyFilter* bodyFilter);

JPH_CAPI JPH_Bool32 JPH_NarrowPhaseQuery_CollidePoint(const JPH_NarrowPhaseQuery* query,
	const JPH_RVec3* point,
	JPH_CollidePointCollector* callback, void* userData,
	JPH_BroadPhaseLayerFilter* broadPhaseLayerFilter,
	JPH_ObjectLayerFilter* objectLayerFilter,
	JPH_BodyFilter* bodyFilter);

JPH_CAPI JPH_Bool32 JPH_NarrowPhaseQuery_CollideShape(const JPH_NarrowPhaseQuery* query,
	const JPH_Shape* shape, const JPH_Vec3* scale, const JPH_RMatrix4x4* centerOfMassTransform,
	JPH_RVec3* baseOffset,
	JPH_CollideShapeCollector* callback, void* userData,
	JPH_BroadPhaseLayerFilter* broadPhaseLayerFilter,
	JPH_ObjectLayerFilter* objectLayerFilter,
	JPH_BodyFilter* bodyFilter);

JPH_CAPI JPH_Bool32 JPH_NarrowPhaseQuery_CastShape(const JPH_NarrowPhaseQuery* query,
    const JPH_Shape* shape,
	const JPH_RMatrix4x4* centerOfMassTransform, const JPH_Vec3* direction, JPH_RVec3* baseOffset,
	JPH_CastShapeCollector* callback, void* userData,
	JPH_BroadPhaseLayerFilter* broadPhaseLayerFilter,
	JPH_ObjectLayerFilter* objectLayerFilter,
	JPH_BodyFilter* bodyFilter);

/* Body */
JPH_CAPI JPH_BodyID JPH_Body_GetID(const JPH_Body* body);
JPH_CAPI JPH_BodyType JPH_Body_GetBodyType(const JPH_Body* body);
JPH_CAPI void JPH_Body_GetWorldSpaceBounds(const JPH_Body* body, JPH_AABox* result);
JPH_CAPI void JPH_Body_GetWorldSpaceSurfaceNormal(const JPH_Body* body, JPH_SubShapeID subShapeID, const JPH_RVec3* position, JPH_Vec3* normal);

JPH_CAPI JPH_Bool32 JPH_Body_IsActive(const JPH_Body* body);
JPH_CAPI JPH_Bool32 JPH_Body_IsStatic(const JPH_Body* body);
JPH_CAPI JPH_Bool32 JPH_Body_IsKinematic(const JPH_Body* body);
JPH_CAPI JPH_Bool32 JPH_Body_IsDynamic(const JPH_Body* body);
JPH_CAPI JPH_Bool32 JPH_Body_IsSensor(const JPH_Body* body);
JPH_CAPI void JPH_Body_SetIsSensor(JPH_Body* body, JPH_Bool32 value);

JPH_CAPI void JPH_Body_SetCollideKinematicVsNonDynamic(JPH_Body* body, JPH_Bool32 value);
JPH_CAPI JPH_Bool32 JPH_Body_GetCollideKinematicVsNonDynamic(const JPH_Body* body);

JPH_CAPI void JPH_Body_SetUseManifoldReduction(JPH_Body* body, JPH_Bool32 value);
JPH_CAPI JPH_Bool32 JPH_Body_GetUseManifoldReduction(const JPH_Body* body);
JPH_CAPI JPH_Bool32 JPH_Body_GetUseManifoldReductionWithBody(const JPH_Body* body, const JPH_Body* other);

JPH_CAPI void JPH_Body_SetApplyGyroscopicForce(JPH_Body* body, JPH_Bool32 value);
JPH_CAPI JPH_Bool32 JPH_Body_GetApplyGyroscopicForce(const JPH_Body* body);

JPH_CAPI JPH_MotionProperties* JPH_Body_GetMotionProperties(JPH_Body* body);
JPH_CAPI JPH_MotionType JPH_Body_GetMotionType(const JPH_Body* body);
JPH_CAPI void JPH_Body_SetMotionType(JPH_Body* body, JPH_MotionType motionType);
JPH_CAPI JPH_Bool32 JPH_Body_GetAllowSleeping(JPH_Body* body);
JPH_CAPI void JPH_Body_SetAllowSleeping(JPH_Body* body, JPH_Bool32 allowSleeping);
JPH_CAPI void JPH_Body_ResetSleepTimer(JPH_Body* body);

JPH_CAPI float JPH_Body_GetFriction(const JPH_Body* body);
JPH_CAPI void JPH_Body_SetFriction(JPH_Body* body, float friction);
JPH_CAPI float JPH_Body_GetRestitution(const JPH_Body* body);
JPH_CAPI void JPH_Body_SetRestitution(JPH_Body* body, float restitution);
JPH_CAPI void JPH_Body_GetLinearVelocity(JPH_Body* body, JPH_Vec3* velocity);
JPH_CAPI void JPH_Body_SetLinearVelocity(JPH_Body* body, const JPH_Vec3* velocity);
JPH_CAPI void JPH_Body_GetAngularVelocity(JPH_Body* body, JPH_Vec3* velocity);
JPH_CAPI void JPH_Body_SetAngularVelocity(JPH_Body* body, const JPH_Vec3* velocity);
JPH_CAPI void JPH_Body_AddForce(JPH_Body* body, const JPH_Vec3* force);
JPH_CAPI void JPH_Body_AddForceAtPosition(JPH_Body* body, const JPH_Vec3* force, const JPH_RVec3* position);
JPH_CAPI void JPH_Body_AddTorque(JPH_Body* body, const JPH_Vec3* force);
JPH_CAPI void JPH_Body_GetAccumulatedForce(JPH_Body* body, JPH_Vec3* force);
JPH_CAPI void JPH_Body_GetAccumulatedTorque(JPH_Body* body, JPH_Vec3* force);
JPH_CAPI void JPH_Body_AddImpulse(JPH_Body* body, const JPH_Vec3* impulse);
JPH_CAPI void JPH_Body_AddImpulseAtPosition(JPH_Body* body, const JPH_Vec3* impulse, const JPH_RVec3* position);
JPH_CAPI void JPH_Body_AddAngularImpulse(JPH_Body* body, const JPH_Vec3* angularImpulse);

JPH_CAPI void JPH_Body_GetPosition(const JPH_Body* body, JPH_RVec3* result);
JPH_CAPI void JPH_Body_GetRotation(const JPH_Body* body, JPH_Quat* result);
JPH_CAPI void JPH_Body_GetCenterOfMassPosition(const JPH_Body* body, JPH_RVec3* result);
JPH_CAPI void JPH_Body_GetWorldTransform(const JPH_Body* body, JPH_RMatrix4x4* result);
JPH_CAPI void JPH_Body_GetCenterOfMassTransform(const JPH_Body* body, JPH_RMatrix4x4* result);

JPH_CAPI void JPH_Body_SetUserData(JPH_Body* body, uint64_t userData);
JPH_CAPI uint64_t JPH_Body_GetUserData(JPH_Body* body);

JPH_CAPI JPH_Body* JPH_Body_GetFixedToWorldBody(void);

/* JPH_BroadPhaseLayerFilter_Procs */
typedef struct JPH_BroadPhaseLayerFilter_Procs {
    JPH_Bool32(JPH_API_CALL* ShouldCollide)(void* userData, JPH_BroadPhaseLayer layer);
} JPH_BroadPhaseLayerFilter_Procs;

JPH_CAPI void JPH_BroadPhaseLayerFilter_SetProcs(JPH_BroadPhaseLayerFilter* filter, JPH_BroadPhaseLayerFilter_Procs procs, void* userData);
JPH_CAPI JPH_BroadPhaseLayerFilter* JPH_BroadPhaseLayerFilter_Create(void);
JPH_CAPI void JPH_BroadPhaseLayerFilter_Destroy(JPH_BroadPhaseLayerFilter* filter);

/* JPH_ObjectLayerFilter */
typedef struct JPH_ObjectLayerFilter_Procs {
    JPH_Bool32(JPH_API_CALL* ShouldCollide)(void* userData, JPH_ObjectLayer layer);
} JPH_ObjectLayerFilter_Procs;

JPH_CAPI void JPH_ObjectLayerFilter_SetProcs(JPH_ObjectLayerFilter* filter, JPH_ObjectLayerFilter_Procs procs, void* userData);
JPH_CAPI JPH_ObjectLayerFilter* JPH_ObjectLayerFilter_Create(void);
JPH_CAPI void JPH_ObjectLayerFilter_Destroy(JPH_ObjectLayerFilter* filter);

/* JPH_BodyFilter */
typedef struct JPH_BodyFilter_Procs {
    JPH_Bool32(JPH_API_CALL* ShouldCollide)(void* userData, JPH_BodyID bodyID);
    JPH_Bool32(JPH_API_CALL* ShouldCollideLocked)(void* userData, const JPH_Body *bodyID);
} JPH_BodyFilter_Procs;

JPH_CAPI void JPH_BodyFilter_SetProcs(JPH_BodyFilter* filter, JPH_BodyFilter_Procs procs, void* userData);
JPH_CAPI JPH_BodyFilter* JPH_BodyFilter_Create(void);
JPH_CAPI void JPH_BodyFilter_Destroy(JPH_BodyFilter* filter);

/* Contact listener */
typedef struct JPH_ContactListener_Procs {
    JPH_ValidateResult (JPH_API_CALL *OnContactValidate)(void* userData,
        const JPH_Body* body1,
        const JPH_Body* body2,
        const JPH_RVec3* baseOffset,
        const JPH_CollideShapeResult* collisionResult);

    void(JPH_API_CALL* OnContactAdded)(void* userData,
        const JPH_Body* body1,
        const JPH_Body* body2,
        const JPH_ContactManifold* manifold,
        JPH_ContactSettings* settings);

    void(JPH_API_CALL* OnContactPersisted)(void* userData,
        const JPH_Body* body1,
        const JPH_Body* body2,
        const JPH_ContactManifold* manifold,
        JPH_ContactSettings* settings);

    void(JPH_API_CALL* OnContactRemoved)(void* userData,
        const JPH_SubShapeIDPair* subShapePair
        );
} JPH_ContactListener_Procs;

JPH_CAPI void JPH_ContactListener_SetProcs(JPH_ContactListener* listener, JPH_ContactListener_Procs procs, void* userData);
JPH_CAPI JPH_ContactListener* JPH_ContactListener_Create(void);
JPH_CAPI void JPH_ContactListener_Destroy(JPH_ContactListener* listener);

/* BodyActivationListener */
typedef struct JPH_BodyActivationListener_Procs {
    void(JPH_API_CALL* OnBodyActivated)(void* userData, JPH_BodyID bodyID, uint64_t bodyUserData);
    void(JPH_API_CALL* OnBodyDeactivated)(void* userData, JPH_BodyID bodyID, uint64_t bodyUserData);
} JPH_BodyActivationListener_Procs;

JPH_CAPI void JPH_BodyActivationListener_SetProcs(JPH_BodyActivationListener* listener, JPH_BodyActivationListener_Procs procs, void* userData);
JPH_CAPI JPH_BodyActivationListener* JPH_BodyActivationListener_Create(void);
JPH_CAPI void JPH_BodyActivationListener_Destroy(JPH_BodyActivationListener* listener);

/* ContactManifold */
JPH_CAPI void JPH_ContactManifold_GetWorldSpaceNormal(const JPH_ContactManifold* manifold, JPH_Vec3* result);
JPH_CAPI float JPH_ContactManifold_GetPenetrationDepth(const JPH_ContactManifold* manifold);
JPH_CAPI JPH_SubShapeID JPH_ContactManifold_GetSubShapeID1(const JPH_ContactManifold* manifold);
JPH_CAPI JPH_SubShapeID JPH_ContactManifold_GetSubShapeID2(const JPH_ContactManifold* manifold);
JPH_CAPI uint32_t JPH_ContactManifold_GetPointCount(const JPH_ContactManifold* manifold);
JPH_CAPI void JPH_ContactManifold_GetWorldSpaceContactPointOn1(const JPH_ContactManifold* manifold, uint32_t index, JPH_RVec3* result);
JPH_CAPI void JPH_ContactManifold_GetWorldSpaceContactPointOn2(const JPH_ContactManifold* manifold, uint32_t index, JPH_RVec3* result);

/* ContactSettings */
JPH_CAPI float JPH_ContactSettings_GetFriction(JPH_ContactSettings* settings);
JPH_CAPI void JPH_ContactSettings_SetFriction(JPH_ContactSettings* settings, float friction);
JPH_CAPI float JPH_ContactSettings_GetRestitution(JPH_ContactSettings* settings);
JPH_CAPI void JPH_ContactSettings_SetRestitution(JPH_ContactSettings* settings, float restitution);
JPH_CAPI float JPH_ContactSettings_GetInvMassScale1(JPH_ContactSettings* settings);
JPH_CAPI void JPH_ContactSettings_SetInvMassScale1(JPH_ContactSettings* settings, float scale);
JPH_CAPI float JPH_ContactSettings_GetInvInertiaScale1(JPH_ContactSettings* settings);
JPH_CAPI void JPH_ContactSettings_SetInvInertiaScale1(JPH_ContactSettings* settings, float scale);
JPH_CAPI float JPH_ContactSettings_GetInvMassScale2(JPH_ContactSettings* settings);
JPH_CAPI void JPH_ContactSettings_SetInvMassScale2(JPH_ContactSettings* settings, float scale);
JPH_CAPI float JPH_ContactSettings_GetInvInertiaScale2(JPH_ContactSettings* settings);
JPH_CAPI void JPH_ContactSettings_SetInvInertiaScale2(JPH_ContactSettings* settings, float scale);
JPH_CAPI JPH_Bool32 JPH_ContactSettings_GetIsSensor(JPH_ContactSettings* settings);
JPH_CAPI void JPH_ContactSettings_SetIsSensor(JPH_ContactSettings* settings, JPH_Bool32 sensor);
JPH_CAPI void JPH_ContactSettings_GetRelativeLinearSurfaceVelocity(JPH_ContactSettings* settings, JPH_Vec3* result);
JPH_CAPI void JPH_ContactSettings_SetRelativeLinearSurfaceVelocity(JPH_ContactSettings* settings, JPH_Vec3* velocity);
JPH_CAPI void JPH_ContactSettings_GetRelativeAngularSurfaceVelocity(JPH_ContactSettings* settings, JPH_Vec3* result);
JPH_CAPI void JPH_ContactSettings_SetRelativeAngularSurfaceVelocity(JPH_ContactSettings* settings, JPH_Vec3* velocity);

/* CharacterBaseSettings */
JPH_CAPI void JPH_CharacterBaseSettings_Destroy(JPH_CharacterBaseSettings* settings);
JPH_CAPI void JPH_CharacterBaseSettings_GetUp(JPH_CharacterBaseSettings* settings, JPH_Vec3* result);
JPH_CAPI void JPH_CharacterBaseSettings_SetUp(JPH_CharacterBaseSettings* settings, const JPH_Vec3* value);
JPH_CAPI void JPH_CharacterBaseSettings_GetSupportingVolume(JPH_CharacterBaseSettings* settings, JPH_Plane* result);
JPH_CAPI void JPH_CharacterBaseSettings_SetSupportingVolume(JPH_CharacterBaseSettings* settings, const JPH_Plane* value);
JPH_CAPI float JPH_CharacterBaseSettings_GetMaxSlopeAngle(JPH_CharacterBaseSettings* settings);
JPH_CAPI void JPH_CharacterBaseSettings_SetMaxSlopeAngle(JPH_CharacterBaseSettings* settings, float maxSlopeAngle);
JPH_CAPI JPH_Bool32 JPH_CharacterBaseSettings_GetEnhancedInternalEdgeRemoval(JPH_CharacterBaseSettings* settings);
JPH_CAPI void JPH_CharacterBaseSettings_SetEnhancedInternalEdgeRemoval(JPH_CharacterBaseSettings* settings, JPH_Bool32 value);
JPH_CAPI const JPH_Shape* JPH_CharacterBaseSettings_GetShape(JPH_CharacterBaseSettings* settings);
JPH_CAPI void JPH_CharacterBaseSettings_SetShape(JPH_CharacterBaseSettings* settings, const JPH_Shape* shape);

/* CharacterBase */
JPH_CAPI void JPH_CharacterBase_Destroy(JPH_CharacterBase* character);
JPH_CAPI float JPH_CharacterBase_GetCosMaxSlopeAngle(JPH_CharacterBase* character);
JPH_CAPI void JPH_CharacterBase_SetMaxSlopeAngle(JPH_CharacterBase* character, float maxSlopeAngle);
JPH_CAPI void JPH_CharacterBase_GetUp(JPH_CharacterBase* character, JPH_Vec3* result);
JPH_CAPI void JPH_CharacterBase_SetUp(JPH_CharacterBase* character, const JPH_Vec3* value);
JPH_CAPI JPH_Bool32 JPH_CharacterBase_IsSlopeTooSteep(JPH_CharacterBase* character, const JPH_Vec3* value);
JPH_CAPI const JPH_Shape* JPH_CharacterBase_GetShape(JPH_CharacterBase* character);

JPH_CAPI JPH_GroundState JPH_CharacterBase_GetGroundState(JPH_CharacterBase* character);
JPH_CAPI JPH_Bool32 JPH_CharacterBase_IsSupported(JPH_CharacterBase* character);
JPH_CAPI void JPH_CharacterBase_GetGroundPosition(JPH_CharacterBase* character, JPH_RVec3* position);
JPH_CAPI void JPH_CharacterBase_GetGroundNormal(JPH_CharacterBase* character, JPH_Vec3* normal);
JPH_CAPI void JPH_CharacterBase_GetGroundVelocity(JPH_CharacterBase* character, JPH_Vec3* velocity);
JPH_CAPI const JPH_PhysicsMaterial* JPH_CharacterBase_GetGroundMaterial(JPH_CharacterBase* character);
JPH_CAPI JPH_BodyID JPH_CharacterBase_GetGroundBodyId(JPH_CharacterBase* character);
JPH_CAPI JPH_SubShapeID JPH_CharacterBase_GetGroundSubShapeId(JPH_CharacterBase* character);
JPH_CAPI uint64_t JPH_CharacterBase_GetGroundUserData(JPH_CharacterBase* character);

/* CharacterVirtualSettings */
JPH_CAPI JPH_CharacterVirtualSettings* JPH_CharacterVirtualSettings_Create(void);
JPH_CAPI float JPH_CharacterVirtualSettings_GetMass(JPH_CharacterVirtualSettings* settings);
JPH_CAPI void JPH_CharacterVirtualSettings_SetMass(JPH_CharacterVirtualSettings* settings, float value);
JPH_CAPI float JPH_CharacterVirtualSettings_GetMaxStrength(JPH_CharacterVirtualSettings* settings);
JPH_CAPI void JPH_CharacterVirtualSettings_SetMaxStrength(JPH_CharacterVirtualSettings* settings, float value);
JPH_CAPI void JPH_CharacterVirtualSettings_GetShapeOffset(JPH_CharacterVirtualSettings* settings, JPH_Vec3* result);
JPH_CAPI void JPH_CharacterVirtualSettings_SetShapeOffset(JPH_CharacterVirtualSettings* settings, const JPH_Vec3* value);
JPH_CAPI JPH_BackFaceMode JPH_CharacterVirtualSettings_GetBackFaceMode(JPH_CharacterVirtualSettings* settings);
JPH_CAPI void JPH_CharacterVirtualSettings_SetBackFaceMode(JPH_CharacterVirtualSettings* settings, JPH_BackFaceMode value);
JPH_CAPI float JPH_CharacterVirtualSettings_GetPredictiveContactDistance(JPH_CharacterVirtualSettings* settings);
JPH_CAPI void JPH_CharacterVirtualSettings_SetPredictiveContactDistance(JPH_CharacterVirtualSettings* settings, float value);
JPH_CAPI uint32_t JPH_CharacterVirtualSettings_GetMaxCollisionIterations(JPH_CharacterVirtualSettings* settings);
JPH_CAPI void JPH_CharacterVirtualSettings_SetMaxCollisionIterations(JPH_CharacterVirtualSettings* settings, uint32_t value);
JPH_CAPI uint32_t JPH_CharacterVirtualSettings_GetMaxConstraintIterations(JPH_CharacterVirtualSettings* settings);
JPH_CAPI void JPH_CharacterVirtualSettings_SetMaxConstraintIterations(JPH_CharacterVirtualSettings* settings, uint32_t value);
JPH_CAPI float JPH_CharacterVirtualSettings_GetMinTimeRemaining(JPH_CharacterVirtualSettings* settings);
JPH_CAPI void JPH_CharacterVirtualSettings_SetMinTimeRemaining(JPH_CharacterVirtualSettings* settings, float value);
JPH_CAPI float JPH_CharacterVirtualSettings_GetCollisionTolerance(JPH_CharacterVirtualSettings* settings);
JPH_CAPI void JPH_CharacterVirtualSettings_SetCollisionTolerance(JPH_CharacterVirtualSettings* settings, float value);
JPH_CAPI float JPH_CharacterVirtualSettings_GetCharacterPadding(JPH_CharacterVirtualSettings* settings);
JPH_CAPI void JPH_CharacterVirtualSettings_SetCharacterPadding(JPH_CharacterVirtualSettings* settings, float value);
JPH_CAPI uint32_t JPH_CharacterVirtualSettings_GetMaxNumHits(JPH_CharacterVirtualSettings* settings);
JPH_CAPI void JPH_CharacterVirtualSettings_SetMaxNumHits(JPH_CharacterVirtualSettings* settings, uint32_t value);
JPH_CAPI float JPH_CharacterVirtualSettings_GetHitReductionCosMaxAngle(JPH_CharacterVirtualSettings* settings);
JPH_CAPI void JPH_CharacterVirtualSettings_SetHitReductionCosMaxAngle(JPH_CharacterVirtualSettings* settings, float value);
JPH_CAPI float JPH_CharacterVirtualSettings_GetPenetrationRecoverySpeed(JPH_CharacterVirtualSettings* settings);
JPH_CAPI void JPH_CharacterVirtualSettings_SetPenetrationRecoverySpeed(JPH_CharacterVirtualSettings* settings, float value);

/* CharacterVirtual */
JPH_CAPI JPH_CharacterVirtual* JPH_CharacterVirtual_Create(const JPH_CharacterVirtualSettings* settings,
    const JPH_RVec3* position,
    const JPH_Quat* rotation,
	uint64_t userData,
    JPH_PhysicsSystem* system);

JPH_CAPI void JPH_CharacterVirtual_SetListener(JPH_CharacterVirtual* character, JPH_CharacterContactListener* listener);

JPH_CAPI void JPH_CharacterVirtual_GetLinearVelocity(JPH_CharacterVirtual* character, JPH_Vec3* velocity);
JPH_CAPI void JPH_CharacterVirtual_SetLinearVelocity(JPH_CharacterVirtual* character, const JPH_Vec3* velocity);
JPH_CAPI void JPH_CharacterVirtual_GetPosition(JPH_CharacterVirtual* character, JPH_RVec3* position);
JPH_CAPI void JPH_CharacterVirtual_SetPosition(JPH_CharacterVirtual* character, const JPH_RVec3* position);
JPH_CAPI void JPH_CharacterVirtual_GetRotation(JPH_CharacterVirtual* character, JPH_Quat* rotation);
JPH_CAPI void JPH_CharacterVirtual_SetRotation(JPH_CharacterVirtual* character, const JPH_Quat* rotation);
JPH_CAPI void JPH_CharacterVirtual_GetWorldTransform(JPH_CharacterVirtual* character, JPH_RMatrix4x4* result);
JPH_CAPI void JPH_CharacterVirtual_GetCenterOfMassTransform(JPH_CharacterVirtual* character, JPH_RMatrix4x4* result);
JPH_CAPI float JPH_CharacterVirtual_GetMass(JPH_CharacterVirtual* character);
JPH_CAPI void JPH_CharacterVirtual_SetMass(JPH_CharacterVirtual* character, float value);
JPH_CAPI float JPH_CharacterVirtual_GetMaxStrength(JPH_CharacterVirtual* character);
JPH_CAPI void JPH_CharacterVirtual_SetMaxStrength(JPH_CharacterVirtual* character, float value);

JPH_CAPI float JPH_CharacterVirtual_GetPenetrationRecoverySpeed(JPH_CharacterVirtual* character);
JPH_CAPI void JPH_CharacterVirtual_SetPenetrationRecoverySpeed(JPH_CharacterVirtual* character, float value);
JPH_CAPI JPH_Bool32	JPH_CharacterVirtual_GetEnhancedInternalEdgeRemoval(JPH_CharacterVirtual* character);
JPH_CAPI void JPH_CharacterVirtual_SetEnhancedInternalEdgeRemoval(JPH_CharacterVirtual* character, JPH_Bool32 value);
JPH_CAPI float JPH_CharacterVirtual_GetCharacterPadding(JPH_CharacterVirtual* character);
JPH_CAPI uint32_t JPH_CharacterVirtual_GetMaxNumHits(JPH_CharacterVirtual* character);
JPH_CAPI void JPH_CharacterVirtual_SetMaxNumHits(JPH_CharacterVirtual* character, uint32_t value);
JPH_CAPI float JPH_CharacterVirtual_GetHitReductionCosMaxAngle(JPH_CharacterVirtual* character);
JPH_CAPI void JPH_CharacterVirtual_SetHitReductionCosMaxAngle(JPH_CharacterVirtual* character, float value);
JPH_CAPI JPH_Bool32 JPH_CharacterVirtual_GetMaxHitsExceeded(JPH_CharacterVirtual* character);
JPH_CAPI uint64_t JPH_CharacterVirtual_GetUserData(JPH_CharacterVirtual* character);
JPH_CAPI void JPH_CharacterVirtual_SetUserData(JPH_CharacterVirtual* character, uint64_t value);

JPH_CAPI void JPH_CharacterVirtual_CancelVelocityTowardsSteepSlopes(JPH_CharacterVirtual* character, const JPH_Vec3* desiredVelocity, JPH_Vec3* velocity);
JPH_CAPI void JPH_CharacterVirtual_Update(JPH_CharacterVirtual* character, float deltaTime, JPH_ObjectLayer layer, JPH_PhysicsSystem* system);
JPH_CAPI void JPH_CharacterVirtual_ExtendedUpdate(JPH_CharacterVirtual* character, float deltaTime,
	const JPH_ExtendedUpdateSettings* settings, JPH_ObjectLayer layer, JPH_PhysicsSystem* system);
JPH_CAPI void JPH_CharacterVirtual_RefreshContacts(JPH_CharacterVirtual* character, JPH_ObjectLayer layer, JPH_PhysicsSystem* system);

/* CharacterContactListener */
typedef struct JPH_CharacterContactListener_Procs {
    void (JPH_API_CALL *OnAdjustBodyVelocity)(void* userData,
		const JPH_CharacterVirtual* character,
        const JPH_Body* body2,
        const JPH_Vec3* linearVelocity,
		const JPH_Vec3* angularVelocity);

    JPH_Bool32(JPH_API_CALL* OnContactValidate)(void* userData,
		const JPH_CharacterVirtual* character,
        const JPH_BodyID bodyID2,
        const JPH_SubShapeID subShapeID2);

    void(JPH_API_CALL* OnContactAdded)(void* userData,
		const JPH_CharacterVirtual* character,
        const JPH_BodyID bodyID2,
		const JPH_SubShapeID subShapeID2,
        const JPH_RVec3* contactPosition,
        const JPH_Vec3* contactNormal,
		JPH_CharacterContactSettings* ioSettings);

    void(JPH_API_CALL* OnContactSolve)(void* userData,
		const JPH_CharacterVirtual* character,
        const JPH_BodyID bodyID2,
		const JPH_SubShapeID subShapeID2,
        const JPH_RVec3* contactPosition,
        const JPH_Vec3* contactNormal,
		const JPH_Vec3* contactVelocity,
		const JPH_PhysicsMaterial* contactMaterial,
		const JPH_Vec3* characterVelocity,
		JPH_Vec3* newCharacterVelocity
		);
} JPH_CharacterContactListener_Procs;

JPH_CAPI JPH_CharacterContactListener* JPH_CharacterContactListener_Create(JPH_CharacterContactListener_Procs procs, void* userData);
JPH_CAPI void JPH_CharacterContactListener_Destroy(JPH_CharacterContactListener* listener);

#endif /* JOLT_C_H_ */
