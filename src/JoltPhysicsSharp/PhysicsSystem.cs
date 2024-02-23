// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public delegate ValidateResult ContactValidateHandler(PhysicsSystem system, in Body body1, in Body body2, Double3 baseOffset, IntPtr collisionResult);
public delegate void ContactAddedHandler(PhysicsSystem system, in Body body1, in Body body2);
public delegate void ContactPersistedHandler(PhysicsSystem system, in Body body1, in Body body2);
public delegate void ContactRemovedHandler(PhysicsSystem system, ref SubShapeIDPair subShapePair);
public delegate void BodyActivationHandler(PhysicsSystem system, in BodyID bodyID, ulong bodyUserData);

public readonly struct PhysicsSystemSettings
{
    public readonly int MaxBodies { get; init; } = 10240;
    public readonly int MaxBodyPairs { get; init; } = 65536;
    public readonly int MaxContactConstraints { get; init; } = 10240;
    public ObjectLayerPairFilter? ObjectLayerPairFilter { get; init; }
    public BroadPhaseLayerInterface? BroadPhaseLayerInterface { get; init; }
    public ObjectVsBroadPhaseLayerFilter? ObjectVsBroadPhaseLayerFilter { get; init; }

    public PhysicsSystemSettings()
    {
    }
}

public sealed class PhysicsSystem : NativeObject
{
    private static readonly Dictionary<IntPtr, PhysicsSystem> s_contactListeners = new();
    private static readonly Dictionary<IntPtr, PhysicsSystem> s_bodyActivationListenerListeners = new();
    private static readonly JPH_ContactListener_Procs s_contactListener_Procs;
    private static readonly JPH_BodyActivationListener_Procs s_BodyActivationListener_Procs;

    private readonly IntPtr contactListenerHandle;
    private readonly IntPtr bodyActivationListenerHandle;

    static unsafe PhysicsSystem()
    {
        s_contactListener_Procs = new JPH_ContactListener_Procs
        {
            OnContactValidate = &OnContactValidateCallback,
            OnContactAdded = &OnContactAddedCallback,
            OnContactPersisted = &OnContactPersistedCallback,
            OnContactRemoved = &OnContactRemovedCallback
        };
        JPH_ContactListener_SetProcs(s_contactListener_Procs);

        s_BodyActivationListener_Procs = new JPH_BodyActivationListener_Procs
        {
            OnBodyActivated = &OnBodyActivatedCallback,
            OnBodyDeactivated = &OnBodyDeactivatedCallback
        };
        JPH_BodyActivationListener_SetProcs(s_BodyActivationListener_Procs);
    }

    public unsafe PhysicsSystem(PhysicsSystemSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings.ObjectLayerPairFilter, nameof(settings.ObjectLayerPairFilter));
        ArgumentNullException.ThrowIfNull(settings.BroadPhaseLayerInterface, nameof(settings.BroadPhaseLayerInterface));
        ArgumentNullException.ThrowIfNull(settings.ObjectVsBroadPhaseLayerFilter, nameof(settings.ObjectVsBroadPhaseLayerFilter));

        NativePhysicsSystemSettings nativeSettings = new();
        nativeSettings.maxBodies = settings.MaxBodies;
        nativeSettings.maxBodyPairs = settings.MaxBodyPairs;
        nativeSettings.maxContactConstraints = settings.MaxContactConstraints;
        nativeSettings.broadPhaseLayerInterface = settings.BroadPhaseLayerInterface.Handle;
        nativeSettings.objectLayerPairFilter = settings.ObjectLayerPairFilter.Handle;
        nativeSettings.objectVsBroadPhaseLayerFilter = settings.ObjectVsBroadPhaseLayerFilter.Handle;
        Handle = JPH_PhysicsSystem_Create(&nativeSettings);

        contactListenerHandle = JPH_ContactListener_Create();
        s_contactListeners.Add(contactListenerHandle, this);

        bodyActivationListenerHandle = JPH_BodyActivationListener_Create();
        s_bodyActivationListenerListeners.Add(bodyActivationListenerHandle, this);

        JPH_PhysicsSystem_SetContactListener(Handle, contactListenerHandle);
        JPH_PhysicsSystem_SetBodyActivationListener(Handle, bodyActivationListenerHandle);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="PhysicsSystem" /> class.
    /// </summary>
    ~PhysicsSystem() => Dispose(isDisposing: false);

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            s_contactListeners.Remove(contactListenerHandle);
            JPH_ContactListener_Destroy(contactListenerHandle);

            s_bodyActivationListenerListeners.Remove(bodyActivationListenerHandle);
            JPH_BodyActivationListener_Destroy(bodyActivationListenerHandle);

            JPH_PhysicsSystem_Destroy(Handle);
        }
    }

    public uint BodiesCount => JPH_PhysicsSystem_GetNumBodies(Handle);
    public uint GetNumActiveBodies(BodyType type) => JPH_PhysicsSystem_GetNumActiveBodies(Handle, type);
    public uint MaxBodies => JPH_PhysicsSystem_GetMaxBodies(Handle);

    #region Events
    /// <summary>
    /// Called after detecting a collision between a body pair, but before calling OnContactAdded and before adding the contact constraint.
    /// If the function returns false, the contact will not be added and any other contacts between this body pair will not be processed.
	/// This function will only be called once per PhysicsSystem::Update per body pair and may not be called again the next update
	/// if a contact persists and no new contact pairs between sub shapes are found.
	/// This is a rather expensive time to reject a contact point since a lot of the collision detection has happened already, make sure you
	/// filter out the majority of undesired body pairs through the ObjectLayerPairFilter that is registered on the PhysicsSystem.
	/// Note that this callback is called when all bodies are locked, so don't use any locking functions!
	/// The order of body 1 and 2 is undefined, but when one of the two bodies is dynamic it will be body 1
    /// </summary>
    public event ContactValidateHandler? OnContactValidate;

    /// <summary>
    /// Called whenever a new contact point is detected.
	/// Note that this callback is called when all bodies are locked, so don't use any locking functions!
	/// Body 1 and 2 will be sorted such that body 1 ID less than body 2 ID, so body 1 may not be dynamic.
	/// Note that only active bodies will report contacts, as soon as a body goes to sleep the contacts between that body and all other
	/// bodies will receive an OnContactRemoved callback, if this is the case then <see cref="Body.IsActive"/> will return false during the callback.
	/// When contacts are added, the constraint solver has not run yet, so the collision impulse is unknown at that point.
	/// The velocities of inBody1 and inBody2 are the velocities before the contact has been resolved, so you can use this to
	/// estimate the collision impulse to e.g. determine the volume of the impact sound to play.
    /// </summary>
    public event ContactAddedHandler? OnContactAdded;

    /// <summary>
    /// Called whenever a contact is detected that was also detected last update.
	/// Note that this callback is called when all bodies are locked, so don't use any locking functions!
	/// Body 1 and 2 will be sorted such that body 1 ID less than body 2 ID, so body 1 may not be dynamic.
    /// </summary>
    public event ContactPersistedHandler? OnContactPersisted;

    /// <summary>
    /// Called whenever a contact was detected last update but is not detected anymore.
    /// Note that this callback is called when all bodies are locked, so don't use any locking functions!
    /// Note that we're using BodyID's since the bodies may have been removed at the time of callback.
    /// Body 1 and 2 will be sorted such that body 1 ID less than body 2 ID, so body 1 may not be dynamic.
    /// </summary>
    public event ContactRemovedHandler? OnContactRemoved;

    /// <summary>
    /// Called whenever a body activates, note this can be called from any thread so make sure your code is thread safe.
	/// At the time of the callback the body inBodyID will be locked and no bodies can be activated/deactivated from the callback.
    /// </summary>
    public event BodyActivationHandler? OnBodyActivated;

    /// <summary>
    /// Called whenever a body deactivates, note this can be called from any thread so make sure your code is thread safe.
	/// At the time of the callback the body inBodyID will be locked and no bodies can be activated/deactivated from the callback.
    /// </summary>
    public event BodyActivationHandler? OnBodyDeactivated;
    #endregion Events

    public BodyInterface BodyInterface => new(JPH_PhysicsSystem_GetBodyInterface(Handle));
    public BodyInterface BodyInterfaceNoLock => new(JPH_PhysicsSystem_GetBodyInterfaceNoLock(Handle));

    public BodyLockInterface BodyLockInterface => new(JPC_PhysicsSystem_GetBodyLockInterface(Handle));
    public BodyLockInterface BodyLockInterfaceNoLock => new(JPC_PhysicsSystem_GetBodyLockInterfaceNoLock(Handle));

    public NarrowPhaseQuery NarrowPhaseQuery => new(JPC_PhysicsSystem_GetNarrowPhaseQuery(Handle));
    public NarrowPhaseQuery NarrowPhaseQueryNoLock => new(JPC_PhysicsSystem_GetNarrowPhaseQueryNoLock(Handle));

    public void OptimizeBroadPhase()
    {
        JPH_PhysicsSystem_OptimizeBroadPhase(Handle);
    }

    public PhysicsUpdateError Step(float deltaTime, int collisionSteps)
    {
        return JPH_PhysicsSystem_Step(Handle, deltaTime, collisionSteps);
    }

    public Vector3 Gravity
    {
        get
        {
            JPH_PhysicsSystem_GetGravity(Handle, out Vector3 value);
            return value;
        }
        set => JPH_PhysicsSystem_SetGravity(Handle, value);
    }

    public void GetGravity(out Vector3 gravity)
    {
        JPH_PhysicsSystem_GetGravity(Handle, out gravity);
    }

    public void AddConstraint(Constraint constraint)
    {
        JPH_PhysicsSystem_AddConstraint(Handle, constraint.Handle);
    }

    public void RemoveConstraint(Constraint constraint)
    {
        JPH_PhysicsSystem_RemoveConstraint(Handle, constraint.Handle);
    }

    public void AddConstraints(Constraint[] constraints)
    {
        unsafe
        {
            int count = constraints.Length;
            IntPtr* constraintsPtr = stackalloc IntPtr[count];
            for (int i = 0; i < count; i++)
            {
                constraintsPtr[i] = constraints[i].Handle;
            }
            JPH_PhysicsSystem_AddConstraints(Handle, constraintsPtr, (uint)count);
        }
    }

    public void RemoveConstraints(Constraint[] constraints)
    {
        unsafe
        {
            int count = constraints.Length;
            IntPtr* constraintsPtr = stackalloc IntPtr[count];
            for (int i = 0; i < count; i++)
            {
                constraintsPtr[i] = constraints[i].Handle;
            }
            JPH_PhysicsSystem_RemoveConstraints(Handle, constraintsPtr, (uint)count);
        }
    }

    public void SaveState(StateRecorder recorder, StateRecorderState state)
    {
        JPH_PhysicsSystem_SaveState(Handle, recorder.Handle, state);
    }

    public void RestoreState(StateRecorder recorder)
    {
        JPH_PhysicsSystem_RestoreState(Handle, recorder.Handle);
    }

    #region ContactListener
    [UnmanagedCallersOnly]
    private static unsafe uint OnContactValidateCallback(IntPtr listenerPtr, IntPtr body1, IntPtr body2, Double3* baseOffset, IntPtr collisionResult)
    {
        PhysicsSystem listener = s_contactListeners[listenerPtr];

        if (listener.OnContactValidate != null)
        {
            return (uint)listener.OnContactValidate(listener, new Body(body1), new Body(body2), *baseOffset, collisionResult);
        }

        return (uint)ValidateResult.AcceptAllContactsForThisBodyPair;
    }

    [UnmanagedCallersOnly]
    private static void OnContactAddedCallback(IntPtr listenerPtr, IntPtr body1, IntPtr body2)
    {
        PhysicsSystem listener = s_contactListeners[listenerPtr];
        listener.OnContactAdded?.Invoke(listener, new Body(body1), new Body(body2));
    }

    [UnmanagedCallersOnly]
    private static void OnContactPersistedCallback(IntPtr listenerPtr, IntPtr body1, IntPtr body2)
    {
        PhysicsSystem listener = s_contactListeners[listenerPtr];
        listener.OnContactPersisted?.Invoke(listener, new Body(body1), new Body(body2));
    }

    [UnmanagedCallersOnly]
    private unsafe static void OnContactRemovedCallback(IntPtr listenerPtr, SubShapeIDPair* subShapePair)
    {
        PhysicsSystem listener = s_contactListeners[listenerPtr];
        listener.OnContactRemoved?.Invoke(listener, ref Unsafe.AsRef<SubShapeIDPair>(subShapePair));
    }
    #endregion ContactListener

    #region BodyActivationListener
    [UnmanagedCallersOnly]
    private static void OnBodyActivatedCallback(IntPtr listenerPtr, uint bodyID, ulong bodyUserData)
    {
        PhysicsSystem listener = s_bodyActivationListenerListeners[listenerPtr];
        listener.OnBodyActivated?.Invoke(listener, bodyID, bodyUserData);
    }

    [UnmanagedCallersOnly]
    private static void OnBodyDeactivatedCallback(IntPtr listenerPtr, uint bodyID, ulong bodyUserData)
    {
        PhysicsSystem listener = s_bodyActivationListenerListeners[listenerPtr];
        listener.OnBodyDeactivated?.Invoke(listener, bodyID, bodyUserData);
    }
    #endregion BodyActivationListener
}
