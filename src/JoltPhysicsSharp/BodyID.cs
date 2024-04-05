// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

public readonly struct BodyID(uint id) : IEquatable<BodyID>
{
    /// <summary>
    /// The value for an invalid body ID
    /// </summary>
    public const uint InvalidBodyID = 0xffffffff;

    /// <summary>
    /// This bit is used by the broadphase
    /// </summary>
	public const uint BroadPhaseBit = 0x00800000;

    /// <summary>
    /// Maximum value for body index (also the maximum amount of bodies supported - 1)
    /// </summary>
	public const uint MaxBodyIndex = 0x7fffff;

    /// <summary>
    /// Maximum value for the sequence number
    /// </summary>
	public const byte MaxSequenceNumber = 0xff;

    public uint ID { get; } = id;
    public bool IsValid => ID != InvalidBodyID;
    public bool IsInvalid => ID == InvalidBodyID;

    /// <summary>
    /// Get index in body array
    /// </summary>
	public readonly uint Index => ID & MaxBodyIndex;

    /// <summary>
    /// Get sequence number of body.
    /// </summary>
	public readonly byte SequenceNumber => (byte)(ID >> 24);

    /// <summary>
    /// Returns the index and sequence number combined in an uint
    /// </summary>
    /// <returns></returns>
    public readonly uint IndexAndSequenceNumber => ID;

    public static BodyID Invalid => new(0xffffffff);
    public static implicit operator BodyID(uint id) => new(id);
    public static implicit operator uint(in BodyID id) => id.IndexAndSequenceNumber;

    public static bool operator ==(BodyID left, BodyID right) => left.ID == right.ID;
    public static bool operator !=(BodyID left, BodyID right) => left.ID != right.ID;
    public static bool operator ==(BodyID left, uint right) => left.ID == right;
    public static bool operator !=(BodyID left, uint right) => left.ID != right;
    public bool Equals(BodyID other) => ID == other.ID;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is BodyID handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => ID.GetHashCode();

    public override string ToString() => ID.ToString();
}
