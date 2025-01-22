namespace Sdl3Sharp;

using static Imports;

public sealed class SdlWindow : IDisposable
{
	private SdlWindowPtr _ptr;

	public SdlRenderer? Renderer { get; internal set; }

	private SdlWindow(SdlWindowPtr ptr, SdlRenderer? renderer = null)
	{
		SdlErrorException.ThrowIf(ptr == SdlWindowPtr.Null);

		_ptr = ptr;
	}

	public SdlWindow(SdlWindow parent, int offsetX, int offsetY, int width, int height, SdlWindowFlags flags) :
		this(SDL_CreatePopupWindow(parent._ptr, offsetX, offsetY, width, height, flags)) { }

	public SdlWindow(SdlProperties props) :
		this(SDL_CreateWindowWithProperties(props.Id)) { }

	public SdlWindow(string title, int width, int height, SdlWindowFlags flags = 0, bool withRenderer = false)
	{
		if (withRenderer)
		{
			SdlErrorException.ThrowIf(!SDL_CreateWindowAndRenderer(title, width, height, flags, out var window, out var renderer));

			_ptr = window;
			Renderer = new(renderer, this);
		}
		else
		{
			_ptr = SDL_CreateWindow(title, width, height, flags);
			if (_ptr == SdlWindowPtr.Null)
				throw new();
		}
	}

	public SdlRenderer CreateRenderer(string? name = null)
	{
		var handle = name != null ? SDL_CreateRenderer(_ptr, name) : SDL_CreateRenderer(_ptr, 0);
		SdlErrorException.ThrowIf(handle == SdlRendererPtr.Null);

		Renderer = new(handle, this);
		return Renderer;
	}

	public SdlWindowId GetId() => SDL_GetWindowID(_ptr);

	public SdlProperties GetProperties() => new(SDL_GetWindowProperties(_ptr));

	public void Show() => SdlErrorException.ThrowIf(!SDL_ShowWindow(_ptr));

	public void Hide() => SdlErrorException.ThrowIf(!SDL_ShowWindow(_ptr));

	public float GetOpacity() => SDL_GetWindowOpacity(_ptr);

	public bool SetOpacity(float opacity) => SDL_SetWindowOpacity(_ptr, opacity);

	public void SetSize(int width, int height)
	{
		SdlErrorException.ThrowIf(!SDL_SetWindowSize(_ptr, width, height));
	}

	private void ReleaseUnmanagedResources()
	{
		if (_ptr == SdlWindowPtr.Null)
			return;

		Renderer?.Dispose();
		SDL_DestroyWindow(_ptr);
		_ptr = SdlWindowPtr.Null;
	}

	public void Dispose()
	{
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~SdlWindow()
	{
		ReleaseUnmanagedResources();
	}
}
