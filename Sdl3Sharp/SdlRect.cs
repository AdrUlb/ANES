using System.Runtime.InteropServices;

namespace Sdl3Sharp;

[StructLayout(LayoutKind.Sequential)]
internal struct SdlRect
{
	public int X, Y, W, H;
}
