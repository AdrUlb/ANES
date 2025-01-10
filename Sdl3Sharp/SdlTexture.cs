using System.Diagnostics;
using System.Drawing;
using static Sdl3Sharp.Internal.Imports;

namespace Sdl3Sharp;

public class SdlTexture
{
	private readonly SdlRenderer _renderer;
	private readonly nint _handle;

	private SdlTexture(SdlRenderer renderer, nint handle)
	{
		_renderer = renderer;

		if (handle == 0)
			throw new SdlErrorException();

		_handle = handle;
	}

	public static SdlTexture CreateWithProperties(SdlRenderer renderer, SdlProperties props) => new(renderer, SDL_CreateTextureWithProperties(renderer.Handle, props.Id));
	public void Destroy() => SDL_DestroyTexture(_handle);

	public unsafe void Render(RectangleF sourceRect, RectangleF targetRect)
	{
		var srcrect = SdlFRect.FromRectangleF(sourceRect);
		var dstrect = SdlFRect.FromRectangleF(targetRect);

		var srcrectPtr = sourceRect != RectangleF.Empty ? &srcrect : null;
		var dstrectPtr = targetRect != RectangleF.Empty ? &dstrect : null;

		SDL_RenderTexture(_renderer.Handle, _handle, srcrectPtr, dstrectPtr);
	}

	public void Render() => Render(RectangleF.Empty, RectangleF.Empty);

	public unsafe SdlSurface LockToSurface(Rectangle rect)
	{
		var sdlrect = new SdlRect { X = rect.X, Y = rect.Y, W = rect.Width, H = rect.Height };
		var rectPtr = rect != RectangleF.Empty ? &sdlrect : null;

		if (!SDL_LockTextureToSurface(_handle, rectPtr, out var surfaceHandle))
			throw new SdlErrorException();

		return new(surfaceHandle);
	}

	public SdlSurface LockToSurface() => LockToSurface(Rectangle.Empty);

	public void Unlock() => SDL_UnlockTexture(_handle);

	public void SetScaleMode(SdlScaleMode scaleMode)
	{
		if (!SDL_SetTextureScaleMode(_handle, scaleMode))
			throw new SdlErrorException();
	}
}
