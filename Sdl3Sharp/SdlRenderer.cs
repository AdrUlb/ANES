using System.Drawing;
using static Sdl3Sharp.Imports;

namespace Sdl3Sharp;

public sealed class SdlRenderer : IDisposable
{
	internal SdlRendererPtr Ptr;
	public readonly SdlWindow Window;

	internal SdlRenderer(SdlRendererPtr ptr, SdlWindow window)
	{
		Ptr = ptr;
		Window = window;
	}

	public void SetDrawColor(Color color) => SdlErrorException.ThrowIf(!SDL_SetRenderDrawColor(Ptr, color.R, color.G, color.B, color.A));

	public void SetScale(float scaleX, float scaleY) => SdlErrorException.ThrowIf(!SDL_SetRenderScale(Ptr, scaleX, scaleY));

	public void RenderDebugText(float x, float y, string str) => SdlErrorException.ThrowIf(!SDL_RenderDebugText(Ptr, x, y, str));

	public void Clear() => SdlErrorException.ThrowIf(!SDL_RenderClear(Ptr));
	public void Present() => SdlErrorException.ThrowIf(!SDL_RenderPresent(Ptr));

	public unsafe void FillRect(RectangleF rectangle)
	{
		var sdlRect = SdlFRect.FromRectangleF(rectangle);
		SdlErrorException.ThrowIf(!SDL_RenderFillRect(Ptr, &sdlRect));
	}

	public unsafe void SetClipRect(Rectangle rectangle)
	{
		var sdlRect = SdlRect.FromRectangle(rectangle);
		SdlErrorException.ThrowIf(!SDL_SetRenderClipRect(Ptr, &sdlRect));
	}

	public unsafe void SetClipRect()
	{
		SdlErrorException.ThrowIf(!SDL_SetRenderClipRect(Ptr, null));
	}

	public int GetVsync()
	{
		SdlErrorException.ThrowIf(!SDL_GetRenderVSync(Ptr, out var vsync));

		return vsync;
	}

	private void ReleaseUnmanagedResources()
	{
		if (Ptr == SdlRendererPtr.Null)
			return;

		Window.Renderer = null;
		SDL_DestroyRenderer(Ptr);
		Ptr = SdlRendererPtr.Null;
	}

	public void Dispose()
	{
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~SdlRenderer()
	{
		ReleaseUnmanagedResources();
	}
}
