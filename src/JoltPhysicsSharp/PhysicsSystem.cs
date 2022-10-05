// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public delegate ValidateResult OnContactValidateHandler(PhysicsSystem system, in Body body1, in Body body2, IntPtr collisionResult);
public delegate void OnContactAddedHandler(PhysicsSystem system, in Body body1, in Body body2);
public delegate void OnContactPersistedHandler(PhysicsSystem system, in Body body1, in Body body2);
public delegate void OnContactRemovedHandler(PhysicsSystem system, ref SubShapeIDPair subShapePair);

public sealed class PhysicsSystem : NativeObject
{
    private static readonly Dictionary<IntPtr, PhysicsSystem> s_listeners = new();
    private static readonly JPH_ContactListener_Procs s_contactListener_Procs;
    private readonly IntPtr contactListenerHandle;

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
    }

    public PhysicsSystem()
        : base(JPH_PhysicsSystem_Create())
    {
        contactListenerHandle = JPH_ContactListener_Create();
        s_listeners.Add(contactListenerHandle, this);

        JPH_PhysicsSystem_SetContactListener(Handle, contactListenerHandle);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="PhysicsSystem" /> class.
    /// </summary>
    ~PhysicsSystem() => Dispose(isDisposing: false);

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            s_listeners.Remove(contactListenerHandle);
            JPH_ContactListener_Destroy(contactListenerHandle);
            JPH_PhysicsSystem_Destroy(Handle);
        }
    }

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
    public event OnContactValidateHandler? OnContactValidate;

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
    public event OnContactAddedHandler? OnContactAdded;

    /// <summary>
    /// Called whenever a contact is detected that was also detected last update.
	/// Note that this callback is called when all bodies are locked, so don't use any locking functions!
	/// Body 1 and 2 will be sorted such that body 1 ID less than body 2 ID, so body 1 may not be dynamic.
    /// </summary>
    public event OnContactPersistedHandler? OnContactPersisted;

    /// <summary>
    /// Called whenever a contact was detected last update but is not detected anymore.
    /// Note that this callback is called when all bodies are locked, so don't use any locking functions!
    /// Note that we're using BodyID's since the bodies may have been removed at the time of callback.
    /// Body 1 and 2 will be sorted such that body 1 ID less than body 2 ID, so body 1 may not be dynamic.
    /// </summary>
    public event OnContactRemovedHandler? OnContactRemoved;
    #endregion Events

    public BodyInterface BodyInterface => new(JPH_PhysicsSystem_GetBodyInterface(Handle));

    public void Init(
        uint maxBodies, uint numBodyMutexes,
        uint maxBodyPairs,
        uint maxContactConstraints,
        BroadPhaseLayer layer,
        ObjectVsBroadPhaseLayerFilter objectVsBroadPhaseLayerFilter,
        ObjectLayerPairFilter objectLayerPairFilter)
    {
        JPH_PhysicsSystem_Init(Handle,
            maxBodies, numBodyMutexes, maxBodyPairs, maxContactConstraints,
            layer.Handle,
            objectVsBroadPhaseLayerFilter,
            objectLayerPairFilter);
    }

    public void OptimizeBroadPhase()
    {
        JPH_PhysicsSystem_OptimizeBroadPhase(Handle);
    }

    public void Update(float deltaTime, int collisionSteps, int integrationSubSteps,
        in TempAllocator tempAlocator, in JobSystemThreadPool jobSystem)
    {
        JPH_PhysicsSystem_Update(Handle, deltaTime, collisionSteps, integrationSubSteps, tempAlocator.Handle, jobSystem.Handle);
    }

    #region ContactListener
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static ValidateResult OnContactValidateCallback(IntPtr listenerPtr, IntPtr body1, IntPtr body2, IntPtr collisionResult)
    {
        PhysicsSystem listener = s_listeners[listenerPtr];

        if (listener.OnContactValidate != null)
            return listener.OnContactValidate(listener, new Body(body1), new Body(body2), collisionResult);

        return ValidateResult.AcceptAllContactsForThisBodyPair;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void OnContactAddedCallback(IntPtr listenerPtr, IntPtr body1, IntPtr body2)
    {
        PhysicsSystem listener = s_listeners[listenerPtr];
        listener.OnContactAdded?.Invoke(listener, new Body(body1), new Body(body2));
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void OnContactPersistedCallback(IntPtr listenerPtr, IntPtr body1, IntPtr body2)
    {
        PhysicsSystem listener = s_listeners[listenerPtr];
        listener.OnContactPersisted?.Invoke(listener, new Body(body1), new Body(body2));
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private unsafe static void OnContactRemovedCallback(IntPtr listenerPtr, SubShapeIDPair* subShapePair)
    {
        PhysicsSystem listener = s_listeners[listenerPtr];
        listener.OnContactRemoved?.Invoke(listener, ref Unsafe.AsRef<SubShapeIDPair>(subShapePair));
    }
    #endregion
}
