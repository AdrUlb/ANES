using System.Runtime.CompilerServices;
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
	public unsafe byte* Pixels; // A pointer to the pixels of the surface, the pixels are writeable if non-NULL

	private int _refcount; // Application reference count, used when freeing surface
	private nint _reserved; // Reserved for internal use
}

public class SdlSurface
{
	private readonly nint _handle;

	private unsafe SdlSurfaceData* Data
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => (SdlSurfaceData*)_handle;
	}

	public unsafe int Width => Data->W;

	public unsafe int Height => Data->H;

	public unsafe int Pitch => Data->Pitch;

	public unsafe SdlPixelFormat Format => Data->Format;

	internal unsafe SdlSurface(nint handle)
	{
		if (handle == 0)
			throw new SdlErrorException();

		_handle = handle;
	}

	public unsafe Span<T> GetPixels<T>(int row) where T : unmanaged => new(Data->Pixels + (row * Pitch), Pitch / sizeof(T));

	public static SdlSurface Create(int width, int height, SdlPixelFormat format) => new(SDL_CreateSurface(width, height, format));
	public void Destroy() => SDL_DestroySurface(_handle);
}
