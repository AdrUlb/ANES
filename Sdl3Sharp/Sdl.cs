namespace Sdl3Sharp;

public static class Sdl
{
	private const uint _scancodeMask = 1 << 30;

	public static SdlKeycode ToKeycode(this SdlScancode scancode) => (SdlKeycode)((uint)scancode | _scancodeMask);
}
