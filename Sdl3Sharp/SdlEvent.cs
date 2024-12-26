using System.Runtime.InteropServices;

namespace Sdl3Sharp;

[StructLayout(LayoutKind.Explicit)]
public struct SdlEvent
{
	[FieldOffset(0)]
	public readonly SdlEventType Type;

	[FieldOffset(0)]
	private unsafe fixed byte _padding[128];
}
