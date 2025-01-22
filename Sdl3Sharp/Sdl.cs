using static Sdl3Sharp.Imports;

namespace Sdl3Sharp;

public static class Sdl
{
	public delegate bool EventFilter(nint userdata, in SdlEvent sdlEvent);

	private const uint _scancodeMask = 1 << 30;

	private static bool _init = false;

	public static void Init(SdlInitFlags flags)
	{
		SdlErrorException.ThrowIf(!SDL_Init(flags));

		if (_init)
			return;

		AppDomain.CurrentDomain.ProcessExit += (_, _) => SDL_Quit();

		_init = true;
	}

	public static bool PollEvent(out SdlEvent sdlEvent) => SDL_PollEvent(out sdlEvent);
	public static void AddEventWatch(EventFilter filter, nint userdata)
	{
		SDL_AddEventWatch(filter, userdata);
	}

	public static SdlKeycode ToKeycode(this SdlScancode scancode) => (SdlKeycode)((uint)scancode | _scancodeMask);
}
