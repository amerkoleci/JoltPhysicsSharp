// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

public enum ValidateResult
{
    /// <summary>
    /// Accept this and any further contact points for this body pair
    /// </summary>
    AcceptAllContactsForThisBodyPair,
    /// <summary>
    /// Accept this contact only (and continue calling this callback for every contact manifold for the same body pair)
    /// </summary>
	AcceptContact,
    /// <summary>
    /// Reject this contact only (but process any other contact manifolds for the same body pair)
    /// </summary>
	RejectContact,
    /// <summary>
    /// Rejects this and any further contact points for this body pair
    /// </summary>
	RejectAllContactsForThisBodyPair
}
