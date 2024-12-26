using System.Runtime.InteropServices;
using static Sdl3Sharp.Internal.Imports;

namespace Sdl3Sharp;

[StructLayout(LayoutKind.Sequential)]
public struct SdlSurfaceData
{
	public readonly SdlSurfaceFlags Flags; // The flags of the surface
	public readonly SdlPixelFormat Format; // The format of the surface
	public readonly int W; // The width of the surface.
	public readonly int H; // The height of the surface.
	public readonly int Pitch; // The distance in bytes between rows of pixels
	public unsafe void* Pixels; // A pointer to the pixels of the surface, the pixels are writeable if non-NULL

	private int _refcount; // Application reference count, used when freeing surface
	private nint _reserved; // Reserved for internal use
}

public class SdlSurface
{
	private readonly nint _handle;

	internal SdlSurface(nint handle)
	{
		if (handle == 0)
			throw new SdlErrorException();

		_handle = handle;
	}

	public static SdlSurface Create(int width, int height, SdlPixelFormat format) => new(SDL_CreateSurface(width, height, format));
	public void Destroy() => SDL_DestroySurface(_handle);
	public unsafe SdlSurfaceData* GetData() => (SdlSurfaceData*)_handle;
}
