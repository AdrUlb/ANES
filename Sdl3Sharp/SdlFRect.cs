using System.Runtime.InteropServices;

namespace Sdl3Sharp;

[StructLayout(LayoutKind.Sequential)]
internal struct SdlFRect
{
	public float X, Y, W, H;
}
