using System.Drawing;
using static Sdl3Sharp.Internal.Imports;

namespace Sdl3Sharp;

public sealed class SdlRenderer
{
	internal readonly nint Handle;

	internal SdlRenderer(nint handle)
	{
		if (handle == 0)
			throw new SdlErrorException();

		Handle = handle;
	}

	public static SdlRenderer Create(SdlWindow window, string name) => new(SDL_CreateRenderer(window.Handle, name));
	public static SdlRenderer Create(SdlWindow window) => new(SDL_CreateRenderer(window.Handle, 0));

	public void Destroy() => SDL_DestroyRenderer(Handle);

	public void SetDrawColor(Color color) => SDL_SetRenderDrawColor(Handle, color.R, color.G, color.B, color.A);

	public void SetScale(float scaleX, float scaleY)
	{
		if (!SDL_SetRenderScale(Handle, scaleX, scaleY))
			throw new SdlErrorException();
	}

	public void RenderDebugText(float x, float y, string str) => SDL_RenderDebugText(Handle, x, y, str);

	public void Clear() => SDL_RenderClear(Handle);
	public void Present() => SDL_RenderPresent(Handle);
}
