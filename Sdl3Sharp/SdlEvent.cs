using System.Runtime.InteropServices;

namespace Sdl3Sharp;

[StructLayout(LayoutKind.Explicit)]
public struct SdlEvent
{
	[FieldOffset(0)]
	public readonly SdlEventType Type;

	[FieldOffset(0)]
	public readonly SdlWindowEvent Window;

	[FieldOffset(0)]
	private unsafe fixed byte _padding[128];
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct SdlWindowEvent
{
	public readonly SdlEventType Type;
	private readonly uint _reserved;
	public readonly ulong timestamp; // In nanoseconds, populated using SDL_GetTicksNS()
	public readonly SdlWindowId WindowID; // The associated window
	public readonly int Data1; // event dependent data
	public readonly int Data2; // event dependent data
}
