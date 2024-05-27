// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class ObjectLayerFilter : NativeObject
{
    private readonly JPH_ObjectLayerFilter_Procs _objectLayerFilter_Procs;

    public ObjectLayerFilter()
        : base(JPH_ObjectLayerFilter_Create())
    {
        nint context = DelegateProxies.CreateUserData(this, true);
        _objectLayerFilter_Procs = new JPH_ObjectLayerFilter_Procs
        {
            ShouldCollide = &ShouldCollideCallback,
        };
        JPH_ObjectLayerFilter_SetProcs(Handle, _objectLayerFilter_Procs, context);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="BodyFilter" /> class.
    /// </summary>
    ~ObjectLayerFilter() => Dispose(disposing: false);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            JPH_ObjectLayerFilter_Destroy(Handle);
        }
    }

    protected abstract bool ShouldCollide(ObjectLayer layer);

    [UnmanagedCallersOnly]
    private static Bool32 ShouldCollideCallback(IntPtr context, ObjectLayer layer)
    {
        ObjectLayerFilter listener = DelegateProxies.GetUserData<ObjectLayerFilter>(context, out _);
        return listener.ShouldCollide(layer);
    }
}
