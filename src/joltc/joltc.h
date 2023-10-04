// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

#ifndef _JOLT_C_H
#define _JOLT_C_H

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

typedef struct JPH_Matrix4x4 {
    float m11, m12, m13, m14;
    float m21, m22, m23, m24;
    float m31, m32, m33, m34;
    float m41, m42, m43, m44;
} JPH_Matrix4x4;

typedef struct JPH_RVec3 {
    double x;
    double y;
    double z;
} JPH_RVec3;

typedef struct JPH_Quat {
    float x;
    float y;
    float z;
    float w;
} JPH_Quat;

typedef struct JPH_RMatrix4x4 {
    float m11, m12, m13, m14;
    float m21, m22, m23, m24;
    float m31, m32, m33, m34;
    double m41, m42, m43, m44;
} JPH_RMatrix4x4;

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
} JPH_IndexedTriangle;

typedef struct JPH_MassProperties {
    float mass;
    JPH_Matrix4x4 inertia;
} JPH_MassProperties;

typedef struct JPH_SubShapeIDPair
{
    JPH_BodyID     Body1ID;
    JPH_SubShapeID subShapeID1;
    JPH_BodyID     Body2ID;
    JPH_SubShapeID subShapeID2;
} JPH_SubShapeIDPair;

// NOTE: Needs to be kept in sync with JPH::RayCastResult
typedef struct JPH_RayCastResult
{
    JPH_BodyID     bodyID; // JPC_BODY_ID_INVALID
    float          fraction; // 1.0 + JPC_FLT_EPSILON
    JPH_SubShapeID subShapeID2;
} JPH_RayCastResult;

typedef struct JPH_TempAllocator                    JPH_TempAllocator;
typedef struct JPH_JobSystem                        JPH_JobSystem;
typedef struct JPH_BroadPhaseLayerInterface         JPH_BroadPhaseLayerInterface;
typedef struct JPH_ObjectVsBroadPhaseLayerFilter    JPH_ObjectVsBroadPhaseLayerFilter;
typedef struct JPH_ObjectLayerPairFilter            JPH_ObjectLayerPairFilter;
typedef struct JPH_BroadPhaseLayerFilter            JPH_BroadPhaseLayerFilter;
typedef struct JPH_ObjectLayerFilter                JPH_ObjectLayerFilter;
typedef struct JPH_BodyFilter                       JPH_BodyFilter;

typedef struct JPH_PhysicsSystem                    JPH_PhysicsSystem;

typedef struct JPH_ShapeSettings                JPH_ShapeSettings;
typedef struct JPH_BoxShapeSettings             JPH_BoxShapeSettings;
typedef struct JPH_SphereShapeSettings          JPH_SphereShapeSettings;
typedef struct JPH_TriangleShapeSettings        JPH_TriangleShapeSettings;
typedef struct JPH_CapsuleShapeSettings         JPH_CapsuleShapeSettings;
typedef struct JPH_CylinderShapeSettings        JPH_CylinderShapeSettings;
typedef struct JPH_ConvexShapeSettings			JPH_ConvexShapeSettings;
typedef struct JPH_ConvexHullShapeSettings      JPH_ConvexHullShapeSettings;
typedef struct JPH_MeshShapeSettings            JPH_MeshShapeSettings;
typedef struct JPH_HeightFieldShapeSettings     JPH_HeightFieldShapeSettings;
typedef struct JPH_TaperedCapsuleShapeSettings  JPH_TaperedCapsuleShapeSettings;
typedef struct JPH_CompoundShapeSettings        JPH_CompoundShapeSettings;
typedef struct JPH_StaticCompoundShapeSettings  JPH_StaticCompoundShapeSettings;
typedef struct JPH_MutableCompoundShapeSettings JPH_MutableCompoundShapeSettings;

typedef struct JPH_Shape                        JPH_Shape;
typedef struct JPH_ConvexShape                  JPH_ConvexShape;
typedef struct JPH_BoxShape                     JPH_BoxShape;
typedef struct JPH_SphereShape                  JPH_SphereShape;
typedef struct JPH_CylinderShape                JPH_CylinderShape;
typedef struct JPH_CapsuleShape                 JPH_CapsuleShape;
typedef struct JPH_StaticCompoundShape          JPH_StaticCompoundShape;
typedef struct JPH_MeshShape                    JPH_MeshShape;
typedef struct JPH_MutableCompoundShape         JPH_MutableCompoundShape;
typedef struct JPH_ConvexHullShape              JPH_ConvexHullShape;

typedef struct JPH_BodyCreationSettings         JPH_BodyCreationSettings;
typedef struct JPH_SoftBodyCreationSettings     JPH_SoftBodyCreationSettings;
typedef struct JPH_BodyInterface                JPH_BodyInterface;
typedef struct JPH_BodyLockInterface            JPH_BodyLockInterface;
typedef struct JPH_NarrowPhaseQuery             JPH_NarrowPhaseQuery;
typedef struct JPH_MotionProperties             JPH_MotionProperties;
typedef struct JPH_MassProperties               JPH_MassProperties;
typedef struct JPH_Body                         JPH_Body;

typedef struct JPH_SpringSettings               JPH_SpringSettings;

typedef struct JPH_ConstraintSettings           JPH_ConstraintSettings;
typedef struct JPH_TwoBodyConstraintSettings    JPH_TwoBodyConstraintSettings;
typedef struct JPH_DistanceConstraintSettings   JPH_DistanceConstraintSettings;
typedef struct JPH_HingeConstraintSettings      JPH_HingeConstraintSettings;
typedef struct JPH_SliderConstraintSettings     JPH_SliderConstraintSettings;
typedef struct JPH_PointConstraintSettings      JPH_PointConstraintSettings;

typedef struct JPH_Constraint                   JPH_Constraint;
typedef struct JPH_TwoBodyConstraint            JPH_TwoBodyConstraint;
typedef struct JPH_DistanceConstraint           JPH_DistanceConstraint;
typedef struct JPH_PointConstraint              JPH_PointConstraint;
typedef struct JPH_HingeConstraint              JPH_HingeConstraint;
typedef struct JPH_SliderConstraint             JPH_SliderConstraint;

typedef struct JPH_CollideShapeResult           JPH_CollideShapeResult;
typedef struct JPH_ContactListener              JPH_ContactListener;

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

typedef JPH_Bool32(JPH_API_CALL* JPH_AssertFailureFunc)(const char* expression, const char* mssage, const char* file, uint32_t line);

JPH_CAPI JPH_Bool32 JPH_Init(void);
JPH_CAPI void JPH_Shutdown(void);
JPH_CAPI void JPH_SetAssertFailureHandler(JPH_AssertFailureFunc handler);

/* JPH_TempAllocator */
JPH_CAPI JPH_TempAllocator* JPH_TempAllocatorMalloc_Create();
JPH_CAPI JPH_TempAllocator* JPH_TempAllocator_Create(uint32_t size);
JPH_CAPI void JPH_TempAllocator_Destroy(JPH_TempAllocator* allocator);

/* JPH_JobSystemThreadPool */
JPH_CAPI JPH_JobSystem* JPH_JobSystemThreadPool_Create(uint32_t maxJobs, uint32_t maxBarriers, int inNumThreads);
JPH_CAPI JPH_JobSystem* JPH_JobSystemSingleThreaded_Create(uint32_t maxJobs);
JPH_CAPI void JPH_JobSystem_Destroy(JPH_JobSystem* system);

/* JPH_ShapeSettings */
JPH_CAPI void JPH_ShapeSettings_Destroy(JPH_ShapeSettings* settings);

/* JPH_ConvexShape */
JPH_CAPI float JPH_ConvexShape_GetDensity(const JPH_ConvexShape* shape);
JPH_CAPI void JPH_ConvexShape_SetDensity(JPH_ConvexShape* shape, float inDensity);

/* BoxShape */
JPH_CAPI JPH_BoxShapeSettings* JPH_BoxShapeSettings_Create(const JPH_Vec3* halfExtent, float convexRadius);
JPH_CAPI JPH_BoxShape* JPH_BoxShapeSettings_CreateShape(const JPH_BoxShapeSettings* settings); 
JPH_CAPI JPH_BoxShape* JPH_BoxShape_Create(const JPH_Vec3* halfExtent, float convexRadius);
JPH_CAPI void JPH_BoxShape_GetHalfExtent(const JPH_BoxShape* shape, JPH_Vec3* halfExtent);
JPH_CAPI float JPH_BoxShape_GetVolume(const JPH_BoxShape* shape);
JPH_CAPI float JPH_BoxShape_GetConvexRadius(const JPH_BoxShape* shape);

/* SphereShapeSettings */
JPH_CAPI JPH_SphereShapeSettings* JPH_SphereShapeSettings_Create(float radius);
JPH_CAPI JPH_SphereShape* JPH_SphereShapeSettings_CreateShape(const JPH_SphereShapeSettings* settings); 
JPH_CAPI float JPH_SphereShapeSettings_GetRadius(const JPH_SphereShapeSettings* settings);
JPH_CAPI void JPH_SphereShapeSettings_SetRadius(JPH_SphereShapeSettings* settings, float radius);
JPH_CAPI JPH_SphereShape* JPH_SphereShape_Create(float radius);
JPH_CAPI float JPH_SphereShape_GetRadius(const JPH_SphereShape* shape);

/* TriangleShape */
JPH_CAPI JPH_TriangleShapeSettings* JPH_TriangleShapeSettings_Create(const JPH_Vec3* v1, const JPH_Vec3* v2, const JPH_Vec3* v3, float convexRadius);

/* CapsuleShape */
JPH_CAPI JPH_CapsuleShapeSettings* JPH_CapsuleShapeSettings_Create(float halfHeightOfCylinder, float radius);
JPH_CAPI JPH_CapsuleShape* JPH_CapsuleShape_Create(float halfHeightOfCylinder, float radius);
JPH_CAPI float JPH_CapsuleShape_GetRadius(const JPH_CapsuleShape* shape);
JPH_CAPI float JPH_CapsuleShape_GetHalfHeightOfCylinder(const JPH_CapsuleShape* shape);

/* CylinderShape */
JPH_CAPI JPH_CylinderShapeSettings* JPH_CylinderShapeSettings_Create(float halfHeight, float radius, float convexRadius);
JPH_CAPI JPH_CylinderShape* JPH_CylinderShape_Create(float halfHeight, float radius);
JPH_CAPI float JPH_CylinderShape_GetRadius(const JPH_CylinderShape* shape);
JPH_CAPI float JPH_CylinderShape_GetHalfHeight(const JPH_CylinderShape* shape);

/* ConvexHullShape */
JPH_CAPI float JPH_ConvexShapeSettings_GetDensity(const JPH_ConvexShapeSettings* shape);
JPH_CAPI void JPH_ConvexShapeSettings_SetDensity(JPH_ConvexShapeSettings* shape, float value);

JPH_CAPI JPH_ConvexHullShapeSettings* JPH_ConvexHullShapeSettings_Create(const JPH_Vec3* points, uint32_t pointsCount, float maxConvexRadius);
JPH_CAPI JPH_ConvexHullShape* JPH_ConvexHullShapeSettings_CreateShape(const JPH_ConvexHullShapeSettings* settings);

/* MeshShape */
JPH_CAPI JPH_MeshShapeSettings* JPH_MeshShapeSettings_Create(const JPH_Triangle* triangles, uint32_t triangleCount);
JPH_CAPI JPH_MeshShapeSettings* JPH_MeshShapeSettings_Create2(const JPH_Vec3* vertices, uint32_t verticesCount, const JPH_IndexedTriangle* triangles, uint32_t triangleCount);
JPH_CAPI void JPH_MeshShapeSettings_Sanitize(JPH_MeshShapeSettings* settings);
JPH_CAPI JPH_MeshShape* JPH_MeshShapeSettings_CreateShape(const JPH_MeshShapeSettings* settings); 

/* HeightFieldShape */
JPH_CAPI JPH_HeightFieldShapeSettings* JPH_HeightFieldShapeSettings_Create(const float* samples, const JPH_Vec3* offset, const JPH_Vec3* scale, uint32_t sampleCount);
JPH_CAPI void JPH_MeshShapeSettings_DetermineMinAndMaxSample(const JPH_HeightFieldShapeSettings* settings, float* pOutMinValue, float* pOutMaxValue, float* pOutQuantizationScale);
JPH_CAPI uint32_t JPH_MeshShapeSettings_CalculateBitsPerSampleForError(const JPH_HeightFieldShapeSettings* settings, float maxError);

/* TaperedCapsuleShape */
JPH_CAPI JPH_TaperedCapsuleShapeSettings* JPH_TaperedCapsuleShapeSettings_Create(float halfHeightOfTaperedCylinder, float topRadius, float bottomRadius);

/* CompoundShape */
JPH_CAPI void JPH_CompoundShapeSettings_AddShape(JPH_CompoundShapeSettings* settings, const JPH_Vec3* position, const JPH_Quat* rotation, const JPH_ShapeSettings* shape, uint32_t userData);
JPH_CAPI void JPH_CompoundShapeSettings_AddShape2(JPH_CompoundShapeSettings* settings, const JPH_Vec3* position, const JPH_Quat* rotation, const JPH_Shape* shape, uint32_t userData);

JPH_CAPI JPH_StaticCompoundShapeSettings* JPH_StaticCompoundShapeSettings_Create();

/* MutableCompoundShape */
JPH_CAPI JPH_MutableCompoundShapeSettings* JPH_MutableCompoundShapeSettings_Create();
JPH_CAPI JPH_MutableCompoundShape* JPH_MutableCompoundShape_Create(const JPH_MutableCompoundShapeSettings* settings);

/* Shape */
JPH_CAPI void JPH_Shape_Destroy(JPH_Shape* shape);
JPH_CAPI void JPH_Shape_GetLocalBounds(JPH_Shape* shape, JPH_AABox* result);
JPH_CAPI void JPH_Shape_GetMassProperties(const JPH_Shape* shape, JPH_MassProperties* result);
JPH_CAPI void JPH_Shape_GetCenterOfMass(JPH_Shape* shape, JPH_Vec3* result);
JPH_CAPI float JPH_Shape_GetInnerRadius(JPH_Shape* shape);

/* JPH_BodyCreationSettings */
JPH_CAPI JPH_BodyCreationSettings* JPH_BodyCreationSettings_Create();
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
JPH_CAPI JPH_SoftBodyCreationSettings* JPH_SoftBodyCreationSettings_Create();
//JPH_CAPI void JPH_SoftBodyCreationSettings_Destroy(JPH_SoftBodyCreationSettings* settings);

/* JPH_SpringSettings */
JPH_CAPI JPH_SpringSettings* JPH_SpringSettings_Create(float frequency, float damping);
JPH_CAPI float JPH_SpringSettings_GetFrequency(JPH_SpringSettings* settings);

/* JPH_ConstraintSettings */
JPH_CAPI void JPH_ConstraintSettings_Destroy(JPH_ConstraintSettings* settings);

/* JPH_Constraint */
JPH_CAPI JPH_ConstraintSettings* JPH_Constraint_GetConstraintSettings(JPH_Constraint* constraint);
JPH_CAPI JPH_Bool32 JPH_Constraint_GetEnabled(JPH_Constraint* constraint);
JPH_CAPI void JPH_Constraint_SetEnabled(JPH_Constraint* constraint, JPH_Bool32 enabled);
JPH_CAPI void JPH_Constraint_Destroy(JPH_Constraint* constraint);

/* JPH_DistanceConstraintSettings */
JPH_CAPI JPH_DistanceConstraintSettings* JPH_DistanceConstraintSettings_Create(void);
JPH_CAPI void JPH_DistanceConstraintSettings_GetPoint1(JPH_DistanceConstraintSettings* settings, JPH_RVec3* result);
JPH_CAPI void JPH_DistanceConstraintSettings_SetPoint1(JPH_DistanceConstraintSettings* settings, const JPH_RVec3* value);
JPH_CAPI void JPH_DistanceConstraintSettings_GetPoint2(JPH_DistanceConstraintSettings* settings, JPH_RVec3* result);
JPH_CAPI void JPH_DistanceConstraintSettings_SetPoint2(JPH_DistanceConstraintSettings* settings, const JPH_RVec3* value);
JPH_CAPI JPH_DistanceConstraint* JPH_DistanceConstraintSettings_CreateConstraint(JPH_DistanceConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2); // binding for DistanceConstraintSettings::Create()

/* JPH_DistanceConstraint */
JPH_CAPI void JPH_DistanceConstraint_SetDistance(JPH_DistanceConstraint* constraint, float minDistance, float maxDistance);
JPH_CAPI float JPH_DistanceConstraint_GetMinDistance(JPH_DistanceConstraint* constraint);
JPH_CAPI float JPH_DistanceConstraint_GetMaxDistance(JPH_DistanceConstraint* constraint);
JPH_CAPI JPH_SpringSettings* JPH_DistanceConstraint_GetLimitsSpringSettings(JPH_DistanceConstraint* constraint);
JPH_CAPI void JPH_DistanceConstraint_SetLimitsSpringSettings(JPH_DistanceConstraint* constraint, JPH_SpringSettings* settings);

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
JPH_CAPI void JPH_HingeConstraint_SetLimits(JPH_HingeConstraint* constraint, float inLimitsMin, float inLimitsMax);
JPH_CAPI float JPH_HingeConstraint_GetLimitsMin(JPH_HingeConstraint* constraint);
JPH_CAPI float JPH_HingeConstraint_GetLimitsMax(JPH_HingeConstraint* constraint);
JPH_CAPI JPH_Bool32 JPH_HingeConstraint_HasLimits(JPH_HingeConstraint* constraint);
JPH_CAPI JPH_SpringSettings* JPH_HingeConstraint_GetLimitsSpringSettings(JPH_HingeConstraint* constraint);
JPH_CAPI void JPH_HingeConstraint_SetLimitsSpringSettings(JPH_HingeConstraint* constraint, JPH_SpringSettings* settings);

/* JPH_SliderConstraintSettings */
JPH_CAPI JPH_SliderConstraintSettings* JPH_SliderConstraintSettings_Create(void);
JPH_CAPI void JPH_SliderConstraintSettings_SetSliderAxis(JPH_SliderConstraintSettings* settings, const JPH_Vec3* axis);
JPH_CAPI void JPH_SliderConstraintSettings_GetSliderAxis(JPH_SliderConstraintSettings* settings, JPH_Vec3* axis);
JPH_CAPI JPH_SliderConstraint* JPH_SliderConstraintSettings_CreateConstraint(JPH_SliderConstraintSettings* settings, JPH_Body* body1, JPH_Body* body2); // binding for SliderConstraintSettings::Create()

/* JPH_SliderConstraint */
JPH_CAPI float JPH_SliderConstraint_GetCurrentPosition(JPH_SliderConstraint* constraint);
JPH_CAPI void JPH_SliderConstraint_SetLimits(JPH_SliderConstraint* constraint, float inLimitsMin, float inLimitsMax);
JPH_CAPI float JPH_SliderConstraint_GetLimitsMin(JPH_SliderConstraint* constraint);
JPH_CAPI float JPH_SliderConstraint_GetLimitsMax(JPH_SliderConstraint* constraint);
JPH_CAPI JPH_Bool32 JPH_SliderConstraint_HasLimits(JPH_SliderConstraint* constraint);

/* JPH_TwoBodyConstraint */
JPH_CAPI JPH_Body* JPH_TwoBodyConstraint_GetBody1(JPH_TwoBodyConstraint* constraint);
JPH_CAPI JPH_Body* JPH_TwoBodyConstraint_GetBody2(JPH_TwoBodyConstraint* constraint);
JPH_CAPI void JPH_TwoBodyConstraint_GetConstraintToBody1Matrix(JPH_TwoBodyConstraint* constraint, JPH_Matrix4x4* result);
JPH_CAPI void JPH_TwoBodyConstraint_GetConstraintToBody2Matrix(JPH_TwoBodyConstraint* constraint, JPH_Matrix4x4* result);


/* JPH_PhysicsSystem */
JPH_CAPI JPH_PhysicsSystem* JPH_PhysicsSystem_Create(void);
JPH_CAPI void JPH_PhysicsSystem_Destroy(JPH_PhysicsSystem* system);
JPH_CAPI void JPH_PhysicsSystem_Init(JPH_PhysicsSystem* system,
    uint32_t maxBodies, uint32_t numBodyMutexes, uint32_t maxBodyPairs, uint32_t maxContactConstraints,
    JPH_BroadPhaseLayer* layer,
    JPH_ObjectVsBroadPhaseLayerFilter* objectVsBroadPhaseLayerFilter,
    JPH_ObjectLayerPairFilter* objectLayerPairFilter);

JPH_CAPI void JPH_PhysicsSystem_OptimizeBroadPhase(JPH_PhysicsSystem* system);
JPH_CAPI JPH_PhysicsUpdateError JPH_PhysicsSystem_Update(JPH_PhysicsSystem* system, float deltaTime, int collisionSteps, 
    JPH_TempAllocator* tempAllocator,
    JPH_JobSystem* jobSystem);

JPH_CAPI JPH_BodyInterface* JPH_PhysicsSystem_GetBodyInterface(JPH_PhysicsSystem* system);
JPH_CAPI JPH_BodyInterface* JPH_PhysicsSystem_GetBodyInterfaceNoLock(JPH_PhysicsSystem* system);

JPH_CAPI const JPH_BodyLockInterface* JPC_PhysicsSystem_GetBodyLockInterface(const JPH_PhysicsSystem* system);
JPH_CAPI const JPH_BodyLockInterface* JPC_PhysicsSystem_GetBodyLockInterfaceNoLock(const JPH_PhysicsSystem* system);

JPH_CAPI const JPH_NarrowPhaseQuery* JPC_PhysicsSystem_GetNarrowPhaseQuery(const JPH_PhysicsSystem* system);
JPH_CAPI const JPH_NarrowPhaseQuery* JPC_PhysicsSystem_GetNarrowPhaseQueryNoLock(const JPH_PhysicsSystem* system);

JPH_CAPI void JPH_PhysicsSystem_SetContactListener(JPH_PhysicsSystem* system, JPH_ContactListener* listener);
JPH_CAPI void JPH_PhysicsSystem_SetBodyActivationListener(JPH_PhysicsSystem* system, JPH_BodyActivationListener* listener);

JPH_CAPI uint32_t JPH_PhysicsSystem_GetNumBodies(const JPH_PhysicsSystem* system);
JPH_CAPI uint32_t JPH_PhysicsSystem_GetNumActiveBodies(const JPH_PhysicsSystem* system, JPH_BodyType type);
JPH_CAPI uint32_t JPH_PhysicsSystem_GetMaxBodies(const JPH_PhysicsSystem* system);

JPH_CAPI void JPH_PhysicsSystem_SetGravity(JPH_PhysicsSystem* system, const JPH_Vec3* value);
JPH_CAPI void JPH_PhysicsSystem_GetGravity(JPH_PhysicsSystem* system, JPH_Vec3* result);

JPH_CAPI void JPH_PhysicsSystem_AddConstraint(JPH_PhysicsSystem* system, JPH_Constraint* constraint);
JPH_CAPI void JPH_PhysicsSystem_RemoveConstraint(JPH_PhysicsSystem* system, JPH_Constraint* constraint);

JPH_CAPI void JPH_PhysicsSystem_AddConstraints(JPH_PhysicsSystem* system, JPH_Constraint** constraints, uint32_t count);
JPH_CAPI void JPH_PhysicsSystem_RemoveConstraints(JPH_PhysicsSystem* system, JPH_Constraint** constraints, uint32_t count);

/* BodyInterface */
JPH_CAPI void JPH_BodyInterface_DestroyBody(JPH_BodyInterface* interface, JPH_BodyID bodyID);
JPH_CAPI JPH_BodyID JPH_BodyInterface_CreateAndAddBody(JPH_BodyInterface* interface, JPH_BodyCreationSettings* settings, JPH_Activation activationMode);
JPH_CAPI JPH_Body* JPH_BodyInterface_CreateBody(JPH_BodyInterface* interface, JPH_BodyCreationSettings* settings);
JPH_CAPI JPH_Body* JPH_BodyInterface_CreateSoftBody(JPH_BodyInterface* interface, JPH_SoftBodyCreationSettings* settings);
JPH_CAPI JPH_Body* JPH_BodyInterface_CreateBodyWithID(JPH_BodyInterface* interface, JPH_BodyID bodyID, JPH_BodyCreationSettings* settings);
JPH_CAPI JPH_Body* JPH_BodyInterface_CreateBodyWithoutID(JPH_BodyInterface* interface, JPH_BodyCreationSettings* settings);
JPH_CAPI void JPH_BodyInterface_DestroyBodyWithoutID(JPH_BodyInterface* interface, JPH_Body* body);
JPH_CAPI JPH_Bool32 JPH_BodyInterface_AssignBodyID(JPH_BodyInterface* interface, JPH_Body* body);
JPH_CAPI JPH_Bool32 JPH_BodyInterface_AssignBodyID2(JPH_BodyInterface* interface, JPH_Body* body, JPH_BodyID bodyID);
JPH_CAPI JPH_Body* JPH_BodyInterface_UnassignBodyID(JPH_BodyInterface* interface, JPH_BodyID bodyID);

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
JPH_CAPI void JPH_BodyInterface_SetPositionRotationAndVelocity(JPH_BodyInterface* interface, JPH_BodyID bodyId, JPH_RVec3* position, JPH_Quat* rotation, JPH_Vec3* linearVelocity, JPH_Vec3* angularVelocity);

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
JPH_CAPI void JPH_MotionProperties_SetLinearDamping(JPH_MotionProperties* properties, float damping);
JPH_CAPI float JPH_MotionProperties_GetLinearDamping(const JPH_MotionProperties* properties);
JPH_CAPI void JPH_MotionProperties_SetAngularDamping(JPH_MotionProperties* properties, float damping);
JPH_CAPI float JPH_MotionProperties_GetAngularDamping(const JPH_MotionProperties* properties);
JPH_CAPI float JPH_MotionProperties_GetInverseMassUnchecked(JPH_MotionProperties* properties);
JPH_CAPI void JPH_MotionProperties_SetMassProperties(JPH_MotionProperties* properties, JPH_AllowedDOFs allowedDOFs, const JPH_MassProperties* massProperties);

//--------------------------------------------------------------------------------------------------
// JPH_NarrowPhaseQuery
//--------------------------------------------------------------------------------------------------
JPH_CAPI JPH_Bool32 JPH_NarrowPhaseQuery_CastRay(const JPH_NarrowPhaseQuery* query,
    const JPH_RVec3* origin, const JPH_Vec3* direction,
    JPH_RayCastResult* hit, // *Must* be default initialized (see JPH_RayCastResult)
    const void* broadPhaseLayerFilter, // Can be NULL (no filter)
    const void* objectLayerFilter, // Can be NULL (no filter)
    const void* bodyFilter); // Can be NULL (no filter)

/* Body */
JPH_CAPI JPH_BodyID JPH_Body_GetID(const JPH_Body* body);
JPH_CAPI JPH_BodyType JPH_Body_GetBodyType(const JPH_Body* body);
JPH_CAPI void JPH_Body_GetWorldSpaceBounds(const JPH_Body* body, JPH_AABox* result);

JPH_CAPI JPH_Bool32 JPH_Body_IsActive(const JPH_Body* body);
JPH_CAPI JPH_Bool32 JPH_Body_IsStatic(const JPH_Body* body);
JPH_CAPI JPH_Bool32 JPH_Body_IsKinematic(const JPH_Body* body);
JPH_CAPI JPH_Bool32 JPH_Body_IsDynamic(const JPH_Body* body);
JPH_CAPI JPH_Bool32 JPH_Body_IsSensor(const JPH_Body* body);
JPH_CAPI void JPH_Body_SetIsSensor(JPH_Body* body, JPH_Bool32 value);
JPH_CAPI JPH_MotionProperties* JPH_Body_GetMotionProperties(JPH_Body* body);
JPH_CAPI JPH_MotionType JPH_Body_GetMotionType(const JPH_Body* body);
JPH_CAPI void JPH_Body_SetMotionType(JPH_Body* body, JPH_MotionType motionType);
JPH_CAPI JPH_Bool32 JPH_Body_GetAllowSleeping(JPH_Body* body);
JPH_CAPI void JPH_Body_SetAllowSleeping(JPH_Body* body, JPH_Bool32 allowSleeping);
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

/* JPH_BroadPhaseLayer */
typedef struct JPH_BroadPhaseLayerInterface_Procs {
    uint32_t(JPH_API_CALL* GetNumBroadPhaseLayers)(const JPH_BroadPhaseLayerInterface* interface);
    JPH_BroadPhaseLayer(JPH_API_CALL* GetBroadPhaseLayer)(const JPH_BroadPhaseLayerInterface* interface, JPH_ObjectLayer layer);

    const char* (*GetBroadPhaseLayerName)(const JPH_BroadPhaseLayerInterface* interface, JPH_BroadPhaseLayer layer);
} JPH_BroadPhaseLayerInterface_Procs;

JPH_CAPI void JPH_BroadPhaseLayerInterface_SetProcs(JPH_BroadPhaseLayerInterface_Procs procs);
JPH_CAPI JPH_BroadPhaseLayerInterface* JPH_BroadPhaseLayerInterface_Create();
JPH_CAPI void JPH_BroadPhaseLayerInterface_Destroy(JPH_BroadPhaseLayerInterface* layer);

/* JPH_ObjectVsBroadPhaseLayerFilter */
typedef struct JPH_ObjectVsBroadPhaseLayerFilter_Procs {
    JPH_Bool32(JPH_API_CALL* ShouldCollide)(const JPH_ObjectVsBroadPhaseLayerFilter* filter, JPH_ObjectLayer layer1, JPH_BroadPhaseLayer layer2);
} JPH_ObjectVsBroadPhaseLayerFilter_Procs;

JPH_CAPI void JPH_ObjectVsBroadPhaseLayerFilter_SetProcs(JPH_ObjectVsBroadPhaseLayerFilter_Procs procs);
JPH_CAPI JPH_ObjectVsBroadPhaseLayerFilter* JPH_ObjectVsBroadPhaseLayerFilter_Create();
JPH_CAPI void JPH_ObjectVsBroadPhaseLayerFilter_Destroy(JPH_ObjectVsBroadPhaseLayerFilter* filter);

/* JPH_ObjectLayerPairFilter */
typedef struct JPH_ObjectLayerPairFilter_Procs {
    JPH_Bool32(JPH_API_CALL* ShouldCollide)(const JPH_ObjectLayerPairFilter* filter, JPH_ObjectLayer object1, JPH_ObjectLayer object2);
} JPH_ObjectLayerPairFilter_Procs;

JPH_CAPI void JPH_ObjectLayerPairFilter_SetProcs(JPH_ObjectLayerPairFilter_Procs procs);
JPH_CAPI JPH_ObjectLayerPairFilter* JPH_ObjectLayerPairFilter_Create();
JPH_CAPI void JPH_ObjectLayerPairFilter_Destroy(JPH_ObjectLayerPairFilter* filter);

/* JPH_BroadPhaseLayerFilter_Procs */
typedef struct JPH_BroadPhaseLayerFilter_Procs {
    JPH_Bool32(JPH_API_CALL* ShouldCollide)(const JPH_BroadPhaseLayerFilter* filter, JPH_BroadPhaseLayer layer);
} JPH_BroadPhaseLayerFilter_Procs;

JPH_CAPI void JPH_BroadPhaseLayerFilter_SetProcs(JPH_BroadPhaseLayerFilter_Procs procs);
JPH_CAPI JPH_BroadPhaseLayerFilter* JPH_BroadPhaseLayerFilter_Create();
JPH_CAPI void JPH_BroadPhaseLayerFilter_Destroy(JPH_BroadPhaseLayerFilter* filter);

/* JPH_ObjectLayerFilter */
typedef struct JPH_ObjectLayerFilter_Procs {
    JPH_Bool32(JPH_API_CALL* ShouldCollide)(const JPH_ObjectLayerFilter *filter, JPH_ObjectLayer layer);
} JPH_ObjectLayerFilter_Procs;

JPH_CAPI void JPH_ObjectLayerFilter_SetProcs(JPH_ObjectLayerFilter_Procs procs);
JPH_CAPI JPH_ObjectLayerFilter* JPH_ObjectLayerFilter_Create();
JPH_CAPI void JPH_ObjectLayerFilter_Destroy(JPH_ObjectLayerFilter* filter);

/* JPH_BodyFilter */
typedef struct JPH_BodyFilter_Procs {
    JPH_Bool32(JPH_API_CALL* ShouldCollide)(const JPH_BodyFilter* filter, JPH_BodyID bodyID);
    JPH_Bool32(JPH_API_CALL* ShouldCollideLocked)(const JPH_BodyFilter* filter, const JPH_Body *bodyID);
} JPH_BodyFilter_Procs;

JPH_CAPI void JPH_BodyFilter_SetProcs(JPH_BodyFilter_Procs procs);
JPH_CAPI JPH_BodyFilter* JPH_BodyFilter_Create();
JPH_CAPI void JPH_BodyFilter_Destroy(JPH_BodyFilter* filter);

/* Contact listener */
typedef struct JPH_ContactListener_Procs {
    JPH_ValidateResult (JPH_API_CALL *OnContactValidate)(JPH_ContactListener* listener,
        const JPH_Body* body1,
        const JPH_Body* body2,
        const JPH_RVec3* baseOffset,
        const JPH_CollideShapeResult* collisionResult);

    void(JPH_API_CALL* OnContactAdded)(JPH_ContactListener* listener,
        const JPH_Body* body1,
        const JPH_Body* body2);

    void(JPH_API_CALL* OnContactPersisted)(JPH_ContactListener* listener,
        const JPH_Body* body1,
        const JPH_Body* body2);

    void(JPH_API_CALL* OnContactRemoved)(JPH_ContactListener* listener,
        const JPH_SubShapeIDPair* subShapePair
        );
} JPH_ContactListener_Procs;

JPH_CAPI void JPH_ContactListener_SetProcs(JPH_ContactListener_Procs procs);
JPH_CAPI JPH_ContactListener* JPH_ContactListener_Create();
JPH_CAPI void JPH_ContactListener_Destroy(JPH_ContactListener* listener);

/* BodyActivationListener */
typedef struct JPH_BodyActivationListener_Procs {
    void(JPH_API_CALL* OnBodyActivated)(JPH_BodyActivationListener* listener, JPH_BodyID bodyID, uint64_t bodyUserData);
    void(JPH_API_CALL* OnBodyDeactivated)(JPH_BodyActivationListener* listener, JPH_BodyID bodyID, uint64_t bodyUserData);
} JPH_BodyActivationListener_Procs;

JPH_CAPI void JPH_BodyActivationListener_SetProcs(JPH_BodyActivationListener_Procs procs);
JPH_CAPI JPH_BodyActivationListener* JPH_BodyActivationListener_Create();
JPH_CAPI void JPH_BodyActivationListener_Destroy(JPH_BodyActivationListener* listener);

#endif /* _JOLT_C_H */
