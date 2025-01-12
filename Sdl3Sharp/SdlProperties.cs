namespace Sdl3Sharp;

using static Sdl3Sharp.Internal.Imports;

public readonly struct SdlProperties
{
	public readonly struct NumberProp
	{
		internal readonly string Name;

		internal NumberProp(string name) => Name = name;
	}

	public readonly struct PointerProp
	{
		internal readonly string Name;

		internal PointerProp(string name) => Name = name;
	}

	public static readonly NumberProp TextureCreateWidth = new("SDL.texture.create.width");
	public static readonly NumberProp TextureCreateHeight = new("SDL.texture.create.height");
	public static readonly NumberProp TextureCreateAccess = new("SDL.texture.create.access");
	public static readonly NumberProp TextureCreateFormat = new("SDL.texture.create.format");

	public static readonly PointerProp WindowCreateWin32Hwnd = new("SDL.window.create.win32.hwnd");
	public static readonly PointerProp WindowCreateWidth = new("SDL.window.create.width");
	public static readonly PointerProp WindowCreateHeight = new("SDL.window.create.height");
	public static readonly PointerProp WindowWin32Hwnd = new("SDL.window.win32.hwnd");

	internal readonly uint Id;

	public SdlProperties()
	{
		Id = SDL_CreateProperties();
	}

	internal SdlProperties(uint id)
	{
		Id = id;
	}

	public static SdlProperties Create() => new(SDL_CreateProperties());
	public void Destroy() => SDL_DestroyProperties(Id);
	public void Set(NumberProp prop, long value) => SDL_SetNumberProperty(Id, prop.Name, value);
	public void Set(PointerProp prop, nint value) => SDL_SetPointerProperty(Id, prop.Name, value);
	public nint Get(PointerProp prop, nint defaultValue) => SDL_GetPointerProperty(Id, prop.Name, defaultValue);
}
