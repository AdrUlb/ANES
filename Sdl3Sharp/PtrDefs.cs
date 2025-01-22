namespace Sdl3Sharp;

internal readonly record struct SdlWindowPtr
{
	private readonly nint _ptr;

	public static readonly SdlWindowPtr Null = new(0);
	private SdlWindowPtr(nint ptr) => _ptr = ptr;
}

internal readonly record struct SdlRendererPtr
{
	private readonly nint _ptr;

	public static readonly SdlRendererPtr Null = new(0);

	private SdlRendererPtr(nint ptr) => _ptr = ptr;
}

internal readonly record struct SdlTexturePtr
{
	private readonly nint _ptr;

	public static readonly SdlTexturePtr Null = new(0);

	private SdlTexturePtr(nint ptr) => _ptr = ptr;
}

internal readonly record struct SdlSurfacePtr
{
	private readonly nint _ptr;

	public static readonly SdlSurfacePtr Null = new(0);

	private SdlSurfacePtr(nint ptr) => _ptr = ptr;
	public static unsafe implicit operator SdlSurfaceData*(SdlSurfacePtr value) => (SdlSurfaceData*)value._ptr;
}

internal readonly record struct SdlPropertiesId
{
	public static readonly SdlPropertiesId Zero = new(0);

	private readonly uint _value;

	private SdlPropertiesId(uint value) => _value = value;
}
