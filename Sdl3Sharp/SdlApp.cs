namespace Sdl3Sharp;

using static Internal.Imports;

public abstract class SdlApp(SdlInitFlags initFlags)
{
	protected abstract SdlAppResult Init();
	protected abstract SdlAppResult Iterate();
	protected abstract SdlAppResult Event(SdlEvent sdlEvent);
	protected abstract void Quit(SdlAppResult result);

	private SdlAppResult InternalInit(ref nint appstate, int argc, string[] argv)
	{
		SDL_Init(initFlags);
		return Init();
	}

	private SdlAppResult InternalIterate(nint appstate) => Iterate();

	private SdlAppResult InternalEvent(nint appstate, SdlEvent sdlEvent) => Event(sdlEvent);

	private void InternalQuit(nint appstate, SdlAppResult result)
	{
		Quit(result);
		SDL_Quit();
	}

	public int Run()
	{
		var args = Environment.GetCommandLineArgs();
		return SDL_EnterAppMainCallbacks(args.Length, args, InternalInit, InternalIterate, InternalEvent, InternalQuit);
	}
}
