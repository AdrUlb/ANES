using static Sdl3Sharp.Internal.Imports;

namespace Sdl3Sharp;

public static class Sdl
{
	private const uint _scancodeMask = 1 << 30;

	public static SdlKeycode ToKeycode(this SdlScancode scancode) => (SdlKeycode)((uint)scancode | _scancodeMask);

	public static void Init(SdlInitFlags flags)
	{
		if (!SDL_Init(flags))
			throw new SdlErrorException();
	}

	public static bool PollEvent(out SdlEvent sdlEvent) => SDL_PollEvent(out sdlEvent);
}
