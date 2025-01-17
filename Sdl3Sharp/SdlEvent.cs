using System.Runtime.InteropServices;
using SdlKeyboardId = uint;

namespace Sdl3Sharp;

[StructLayout(LayoutKind.Explicit)]
public struct SdlEvent
{
	[FieldOffset(0)] public readonly SdlEventType Type;

	[FieldOffset(0)] public readonly SdlWindowEvent Window;

	[FieldOffset(0)] public readonly SdlKeyboardEvent Key;

	[FieldOffset(0)] private unsafe fixed byte _padding[128];
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct SdlWindowEvent
{
	private readonly SdlEventType Type;
	private readonly uint _reserved;
	public readonly ulong Timestamp; // In nanoseconds, populated using SDL_GetTicksNS()
	public readonly SdlWindowId WindowID; // The associated window
	public readonly int Data1; // event dependent data
	public readonly int Data2; // event dependent data
}

[StructLayout(LayoutKind.Sequential)]
public struct SdlKeyboardEvent
{
	private SdlEventType _type; // SDL_EVENT_KEY_DOWN or SDL_EVENT_KEY_UP 
	private uint _reserved;
	public ulong Timestamp; // In nanoseconds, populated using SDL_GetTicksNS()
	public SdlWindowId WindowID; // The window with keyboard focus, if any
	public SdlKeyboardId Which; // The keyboard instance id, or 0 if unknown or virtual
	public SdlScancode Scancode; // SDL physical key code
	public SdlKeycode Key; // SDL virtual key code
	public SdlKeymod Mod; // current key modifiers
	public ushort Raw; // The platform dependent scancode for this event
	[MarshalAs(UnmanagedType.U1)] public byte Down; // true if the key is pressed
	[MarshalAs(UnmanagedType.U1)] public byte Repeat; // true if this is a key repeat
}
