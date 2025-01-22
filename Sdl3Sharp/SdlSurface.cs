using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Sdl3Sharp.Imports;

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

public sealed class SdlSurface : IDisposable
{
	private readonly SdlSurfacePtr _handle;

	private unsafe SdlSurfaceData* Data
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _handle;
	}

	public unsafe int Width => Data->W;

	public unsafe int Height => Data->H;

	public unsafe int Pitch => Data->Pitch;

	public unsafe SdlPixelFormat Format => Data->Format;

	internal unsafe SdlSurface(SdlSurfacePtr handle)
	{
		SdlErrorException.ThrowIf(handle == SdlSurfacePtr.Null);

		_handle = handle;
	}

	public SdlSurface(int width, int height, SdlPixelFormat format) : this(SDL_CreateSurface(width, height, format)) { }

	public unsafe Span<T> GetPixelsRowSpan<T>(int row) where T : unmanaged => new(Data->Pixels + (row * Pitch), Pitch / sizeof(T));

	public unsafe void Blit(Rectangle sourceRect, SdlSurface target, Rectangle targetRect)
	{
		var srcrect = SdlRect.FromRectangle(sourceRect);
		var dstrect = SdlRect.FromRectangle(targetRect);

		var srcrectPtr = sourceRect != Rectangle.Empty ? &srcrect : null;
		var dstrectPtr = targetRect != Rectangle.Empty ? &dstrect : null;

		SdlErrorException.ThrowIf(!SDL_BlitSurface(_handle, srcrectPtr, target._handle, dstrectPtr));
	}

	public void Blit(SdlSurface target) => Blit(Rectangle.Empty, target, Rectangle.Empty);

	private void ReleaseUnmanagedResources()
	{
		SDL_DestroySurface(_handle);
	}

	public void Dispose()
	{
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~SdlSurface()
	{
		ReleaseUnmanagedResources();
	}
}
