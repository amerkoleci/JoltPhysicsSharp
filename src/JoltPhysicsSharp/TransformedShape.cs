// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly unsafe struct TransformedShape : IEquatable<TransformedShape>
{
    public TransformedShape(nint handle) { Handle = handle; }
    public nint Handle { get; }
    public bool IsNull => Handle == 0;
    public static TransformedShape Null => new(0);
    public static implicit operator TransformedShape(nint handle) => new(handle);
    public static bool operator ==(TransformedShape left, TransformedShape right) => left.Handle == right.Handle;
    public static bool operator !=(TransformedShape left, TransformedShape right) => left.Handle != right.Handle;
    public static bool operator ==(TransformedShape left, nint right) => left.Handle == right;
    public static bool operator !=(TransformedShape left, nint right) => left.Handle != right;
    public bool Equals(TransformedShape other) => Handle == other.Handle;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is TransformedShape handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => Handle.GetHashCode();

    public BodyID ID => JPH_TransformedShape_GetBodyID(Handle);

    public Vector3 ShapeScale
    {
        get
        {
            JPH_TransformedShape_GetShapeScale(Handle, out Vector3 result);
            return result;
        }
        set => JPH_TransformedShape_SetShapeScale(Handle, in value);
    }

    #region GetCenterOfMassTransform
    public Matrix4x4 GetCenterOfMassTransform()
    {
        if (DoublePrecision)
        {
            JPH_TransformedShape_GetCenterOfMassTransform(Handle, out RMatrix4x4 dResult);
            return (Matrix4x4)dResult;
        }

        JPH_TransformedShape_GetCenterOfMassTransform(Handle, out Matrix4x4 result);
        return result;
    }

    public void GetCenterOfMassTransform(out Matrix4x4 result)
    {
        if (DoublePrecision)
        {
            JPH_TransformedShape_GetCenterOfMassTransform(Handle, out RMatrix4x4 dResult);
            result = (Matrix4x4)dResult;
            return;
        }

        JPH_TransformedShape_GetCenterOfMassTransform(Handle, out result);
    }

    public RMatrix4x4 GetRCenterOfMassTransform()
    {
        if (!DoublePrecision)
        {
            JPH_TransformedShape_GetCenterOfMassTransform(Handle, out Matrix4x4 sResult);
            return sResult;
        }

        JPH_TransformedShape_GetCenterOfMassTransform(Handle, out RMatrix4x4 result);
        return result;
    }

    public void GetRCenterOfMassTransform(out RMatrix4x4 result)
    {
        if (!DoublePrecision)
        {
            JPH_TransformedShape_GetCenterOfMassTransform(Handle, out Matrix4x4 sResult);
            result = sResult;
            return;
        }

        JPH_TransformedShape_GetCenterOfMassTransform(Handle, out result);
    }
    #endregion


    #region GetCenterOfMassTransform
    public Matrix4x4 GetInverseCenterOfMassTransform()
    {
        if (DoublePrecision)
        {
            JPH_TransformedShape_GetInverseCenterOfMassTransform(Handle, out RMatrix4x4 dResult);
            return (Matrix4x4)dResult;
        }

        JPH_TransformedShape_GetInverseCenterOfMassTransform(Handle, out Matrix4x4 result);
        return result;
    }

    public void GetInverseCenterOfMassTransform(out Matrix4x4 result)
    {
        if (DoublePrecision)
        {
            JPH_TransformedShape_GetInverseCenterOfMassTransform(Handle, out RMatrix4x4 dResult);
            result = (Matrix4x4)dResult;
            return;
        }

        JPH_TransformedShape_GetInverseCenterOfMassTransform(Handle, out result);
    }

    public RMatrix4x4 GetRInverseCenterOfMassTransform()
    {
        if (!DoublePrecision)
        {
            JPH_TransformedShape_GetInverseCenterOfMassTransform(Handle, out Matrix4x4 sResult);
            return sResult;
        }

        JPH_TransformedShape_GetInverseCenterOfMassTransform(Handle, out RMatrix4x4 result);
        return result;
    }

    public void GetRInverseCenterOfMassTransform(out RMatrix4x4 result)
    {
        if (!DoublePrecision)
        {
            JPH_TransformedShape_GetInverseCenterOfMassTransform(Handle, out Matrix4x4 sResult);
            result = sResult;
            return;
        }

        JPH_TransformedShape_GetInverseCenterOfMassTransform(Handle, out result);
    }
    #endregion


    #region GetWorldTransform
    public Matrix4x4 GetWorldTransform()
    {
        if (DoublePrecision)
            return (Matrix4x4)GetRWorldTransform();

        JPH_TransformedShape_GetWorldTransform(Handle, out Matrix4x4 result);
        return result;
    }

    public void GetWorldTransform(out Matrix4x4 result)
    {
        if (DoublePrecision)
        {
            JPH_TransformedShape_GetWorldTransform(Handle, out RMatrix4x4 dResult);
            result = (Matrix4x4)dResult;
            return;
        }

        JPH_TransformedShape_GetWorldTransform(Handle, out result);
    }

    public RMatrix4x4 GetRWorldTransform()
    {
        if (!DoublePrecision)
            return GetWorldTransform();

        JPH_TransformedShape_GetWorldTransform(Handle, out RMatrix4x4 result);
        return result;
    }

    public void GetRWorldTransform(out RMatrix4x4 result)
    {
        if (!DoublePrecision)
        {
            JPH_TransformedShape_GetWorldTransform(Handle, out Matrix4x4 sResult);
            result = sResult;
            return;
        }

        JPH_TransformedShape_GetWorldTransform(Handle, out result);
    }

    public void SetWorldTransform(in Matrix4x4 result)
    {
        if (DoublePrecision)
        {
            JPH_TransformedShape_SetWorldTransform(Handle, (RMatrix4x4)result);
            return;
        }

        JPH_TransformedShape_SetWorldTransform(Handle, in result);
    }

    public void SetWorldTransform(in RMatrix4x4 result)
    {
        if (!DoublePrecision)
        {
            JPH_TransformedShape_SetWorldTransform(Handle, (Matrix4x4)result);
            return;
        }

        JPH_TransformedShape_SetWorldTransform(Handle, in result);
    }

    public void SetWorldTransform(in Vector3 position, in Quaternion rotation, in Vector3 scale)
    {
        if (DoublePrecision)
        {
            JPH_TransformedShape_SetWorldTransform2(Handle, (Double3)position, rotation, scale);
            return;
        }

        JPH_TransformedShape_SetWorldTransform2(Handle, in position, in rotation, in scale);
    }

    public void SetWorldTransform(in Double3 position, in Quaternion rotation, in Vector3 scale)
    {
        if (!DoublePrecision)
        {
            JPH_TransformedShape_SetWorldTransform2(Handle, (Vector3)position, rotation, scale);
            return;
        }

        JPH_TransformedShape_SetWorldTransform2(Handle, in position, in rotation, in scale);
    }
    #endregion

    public BoundingBox WorldSpaceBounds
    {
        get
        {
            JPH_TransformedShape_GetWorldSpaceBounds(Handle, out BoundingBox result);
            return result;
        }
    }

    public void GetWorldSpaceBounds(out BoundingBox bounds)
    {
        JPH_TransformedShape_GetWorldSpaceBounds(Handle, out bounds);
    }

    public void GetWorldSpaceSurfaceNormal(in SubShapeID subShapeID, in Vector3 position, out Vector3 normal)
    {
        if (DoublePrecision)
        {
            JPH_TransformedShape_GetWorldSpaceSurfaceNormal(Handle, subShapeID.Value, new Double3(in position), out normal);
        }
        else
        {
            JPH_TransformedShape_GetWorldSpaceSurfaceNormal(Handle, subShapeID.Value, in position, out normal);
        }
    }

    public Vector3 GetWorldSpaceSurfaceNormal(in SubShapeID subShapeID, in Vector3 position)
    {
        if (DoublePrecision)
        {
            JPH_TransformedShape_GetWorldSpaceSurfaceNormal(Handle, subShapeID.Value, new Double3(in position), out Vector3 normal);
            return normal;
        }
        else
        {
            JPH_TransformedShape_GetWorldSpaceSurfaceNormal(Handle, subShapeID.Value, in position, out Vector3 normal);
            return normal;
        }
    }

    public void GetWorldSpaceSurfaceNormal(in SubShapeID subShapeID, in Double3 position, out Vector3 normal)
    {
        if (!DoublePrecision)
        {
            Vector3 sPosition = (Vector3)position;
            JPH_TransformedShape_GetWorldSpaceSurfaceNormal(Handle, subShapeID.Value, in sPosition, out normal);
        }
        else
        {
            JPH_TransformedShape_GetWorldSpaceSurfaceNormal(Handle, subShapeID.Value, in position, out normal);
        }
    }

    public Vector3 GetWorldSpaceSurfaceNormal(in SubShapeID subShapeID, in Double3 position)
    {
        if (!DoublePrecision)
        {
            Vector3 sPosition = (Vector3)position;
            JPH_TransformedShape_GetWorldSpaceSurfaceNormal(Handle, subShapeID.Value, in sPosition, out Vector3 normal);
            return normal;

        }
        else
        {
            JPH_TransformedShape_GetWorldSpaceSurfaceNormal(Handle, subShapeID.Value, in position, out Vector3 normal);
            return normal;
        }
    }

    #region CastRay
    public bool CastRay(in Ray ray, out RayCastResult hit)
    {
        if (DoublePrecision)
            return JPH_TransformedShape_CastRay(Handle, new Double3(in ray.Position), in ray.Direction, out hit);

        return JPH_TransformedShape_CastRay(Handle, in ray.Position, in ray.Direction, out hit);
    }

    public bool CastRay(
        in Ray ray,
        RayCastSettings settings,
        CollisionCollectorType collectorType,
        ICollection<RayCastResult> results,
        ShapeFilter? shapeFilter = default)
    {
        GCHandle callbackHandle = GCHandle.Alloc(results);
        bool callbackResult;

        if (DoublePrecision)
        {
            callbackResult = JPH_TransformedShape_CastRay2(Handle,
                new Double3(in ray.Position), in ray.Direction,
                &settings,
                collectorType, &OnCastRayResultCallback, GCHandle.ToIntPtr(callbackHandle),
                shapeFilter?.Handle ?? 0);
        }
        else
        {
            callbackResult = JPH_TransformedShape_CastRay2(Handle,
                in ray.Position, in ray.Direction,
                &settings,
                collectorType, &OnCastRayResultCallback, GCHandle.ToIntPtr(callbackHandle),
                shapeFilter?.Handle ?? 0);
        }

        callbackHandle.Free();
        return callbackResult;
    }

    [UnmanagedCallersOnly]
    private static unsafe void OnCastRayResultCallback(nint userData, RayCastResult* result)
    {
        ICollection<RayCastResult> collection = (ICollection<RayCastResult>)GCHandle.FromIntPtr(userData).Target!;
        collection.Add(*result);
    }
    #endregion

}
