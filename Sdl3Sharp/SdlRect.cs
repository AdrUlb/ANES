using System.Drawing;
using System.Runtime.InteropServices;

namespace Sdl3Sharp;

[StructLayout(LayoutKind.Sequential)]
internal struct SdlRect
{
	public int X, Y, W, H;

	public static SdlRect FromRectangle(Rectangle rect) => new() { X = rect.X, Y = rect.Y, W = rect.Width, H = rect.Height };
}
