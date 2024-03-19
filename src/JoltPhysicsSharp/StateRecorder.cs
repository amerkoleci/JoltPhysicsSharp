// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public enum StateRecorderState
{
    None = 0,
    Global = 1,
	Bodies = 2,
	Contacts = 4,
	Constraints = 8,
	All = None | Global | Bodies | Contacts | Constraints
}

public sealed class StateRecorder : NativeObject
{
	public StateRecorder()
        : base(JPH_StateRecorder_Create())
    {

    }

	~StateRecorder() => Dispose(isDisposing: false);

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            JPH_StateRecorder_Destroy(Handle);
        }
    }

	public bool Validating
    {
        get => JPH_StateRecorder_IsValidating(Handle);
        set => JPH_StateRecorder_SetValidating(Handle, value);
    }

	public void Rewind()
	{
		JPH_StateRecorder_Rewind(Handle);
	}

	public void Clear()
	{
		JPH_StateRecorder_Clear(Handle);
	}

	public bool IsEOF()
	{
		return JPH_StateRecorder_IsEOF(Handle);
	}

	public bool IsFailed()
	{
		return JPH_StateRecorder_IsFailed(Handle);
	}

	public bool IsEqual(StateRecorder other)
	{
		return JPH_StateRecorder_IsEqual(Handle, other.Handle);
	}

	public void WriteBytes(nint data, ulong size)
	{
		JPH_StateRecorder_WriteBytes(Handle, data, size);
	}

	public void ReadBytes(nint data, ulong size)
	{
		JPH_StateRecorder_ReadBytes(Handle, data, size);
	}

	public ulong GetSize()
	{
		return JPH_StateRecorder_GetSize(Handle);
	}
}
