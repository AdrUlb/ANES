using System.Drawing;
using System.Runtime.InteropServices;

namespace Sdl3Sharp;

[StructLayout(LayoutKind.Sequential)]
internal struct SdlFRect
{
	public float X, Y, W, H;

	public static SdlFRect FromRectangleF(RectangleF rect) => new() { X = rect.X, Y = rect.Y, W = rect.Width, H = rect.Height };
}
