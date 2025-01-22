using static Sdl3Sharp.Imports;

namespace Sdl3Sharp;

public sealed class SdlErrorException() : Exception("SDL Error: " + SDL_GetError())
{
	public static void ThrowIf(bool condition)
	{
		if (condition)
			throw new SdlErrorException();
	}
}
