namespace Sdl3Sharp;

using static Sdl3Sharp.Internal.Imports;

public sealed class SdlWindow
{
	internal readonly nint Handle;

	private SdlWindow(nint handle)
	{
		if (handle == 0)
			throw new SdlErrorException();

		Handle = handle;
	}

	public static SdlWindow Create(string title, int width, int height, SdlWindowFlags flags = 0) => new(SDL_CreateWindow(title, width, height, flags));

	public static SdlWindow CreatePopup(SdlWindow parent, int offsetX, int offsetY, int width, int height, SdlWindowFlags flags = 0) =>
		new(SDL_CreatePopupWindow(parent.Handle, offsetX, offsetY, width, height, flags));

	public static SdlWindow CreateWithProperties(SdlProperties props) => new(SDL_CreateWindowWithProperties(props.Id));

	public static (SdlWindow window, SdlRenderer renderer) CreateWithRenderer(string title, int width, int height, SdlWindowFlags flags = 0)
	{
		if (!SDL_CreateWindowAndRenderer(title, width, height, flags, out var window, out var renderer))
			throw new SdlErrorException();

		return (new(window), new(renderer));
	}

	public void Destroy() => SDL_DestroyWindow(Handle);
	public SdlWindowId GetId() => SDL_GetWindowID(Handle);

	public SdlProperties GetProperties()
	{
		var props = SDL_GetWindowProperties(Handle);
		if (props == 0)
			throw new SdlErrorException();

		return new(props);
	}

	public void SetSize(int width, int height)
	{
		if (!SDL_SetWindowSize(Handle, width, height))
			throw new SdlErrorException();
	}

	public void Show()
	{
		if (!SDL_ShowWindow(Handle))
			throw new SdlErrorException();
	}

	public void Hide()
	{
		if (!SDL_ShowWindow(Handle))
			throw new SdlErrorException();
	}
}
