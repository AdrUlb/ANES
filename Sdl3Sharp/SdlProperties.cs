namespace Sdl3Sharp;

using static Sdl3Sharp.Internal.Imports;

public readonly struct SdlProperties
{
	public readonly struct NumberProp
	{
		internal readonly string Name;

		internal NumberProp(string name) => Name = name;
	}

	public static readonly NumberProp TextureCreateWidth = new("SDL.texture.create.width");
	public static readonly NumberProp TextureCreateHeight = new("SDL.texture.create.height");
	public static readonly NumberProp TextureCreateAccess = new("SDL.texture.create.access");
	public static readonly NumberProp TextureCreateFormat = new("SDL.texture.create.format");

	internal readonly uint Id;

	private SdlProperties(uint id)
	{
		Id = id;
	}

	public static SdlProperties Create() => new(SDL_CreateProperties());
	public void Destroy() => SDL_DestroyProperties(Id);
	public void Set(NumberProp prop, long value) => SDL_SetNumberProperty(Id, prop.Name, value);
}
