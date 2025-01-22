using System.Drawing;
using static Sdl3Sharp.Imports;

namespace Sdl3Sharp;

public sealed class SdlTexture : IDisposable
{
	private readonly SdlRenderer _renderer;
	private SdlTexturePtr _handle;

	private SdlTexture(SdlRenderer renderer, SdlTexturePtr handle)
	{
		_renderer = renderer;
		SdlErrorException.ThrowIf(handle == SdlTexturePtr.Null);
		_handle = handle;
	}

	public SdlTexture(SdlRenderer renderer, SdlProperties props) :
		this(renderer, SDL_CreateTextureWithProperties(renderer.Ptr, props.Id)) { }

	public unsafe void Render(RectangleF sourceRect, RectangleF targetRect)
	{
		var srcrect = SdlFRect.FromRectangleF(sourceRect);
		var dstrect = SdlFRect.FromRectangleF(targetRect);

		var srcrectPtr = sourceRect != RectangleF.Empty ? &srcrect : null;
		var dstrectPtr = targetRect != RectangleF.Empty ? &dstrect : null;

		SdlErrorException.ThrowIf(!SDL_RenderTexture(_renderer.Ptr, _handle, srcrectPtr, dstrectPtr));
	}

	public void Render() => Render(RectangleF.Empty, RectangleF.Empty);

	public unsafe SdlSurface LockToSurface(Rectangle rect)
	{
		var sdlrect = new SdlRect { X = rect.X, Y = rect.Y, W = rect.Width, H = rect.Height };
		var rectPtr = rect != RectangleF.Empty ? &sdlrect : null;

		SdlErrorException.ThrowIf(!SDL_LockTextureToSurface(_handle, rectPtr, out var surfaceHandle));

		return new(surfaceHandle);
	}

	public unsafe SdlSurface LockToSurface()
	{
		SdlErrorException.ThrowIf(!SDL_LockTextureToSurface(_handle, null, out var surfacePtr));
		return new(surfacePtr);
	}

	public void Unlock() => SDL_UnlockTexture(_handle);

	public void SetScaleMode(SdlScaleMode scaleMode) => SdlErrorException.ThrowIf(!SDL_SetTextureScaleMode(_handle, scaleMode));

	private void ReleaseUnmanagedResources()
	{
		if (_handle == SdlTexturePtr.Null)
			return;

		SDL_DestroyTexture(_handle);
		_handle = SdlTexturePtr.Null;
	}

	private void Dispose(bool disposing)
	{
		ReleaseUnmanagedResources();
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	~SdlTexture()
	{
		Dispose(false);
	}
}
