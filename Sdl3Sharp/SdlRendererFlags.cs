namespace Sdl3Sharp;

public enum SdlRendererFlags : uint
{
	Software = 0x00000001, // The renderer is a software fallback
	Accelerated = 0x00000002, // The renderer uses hardware acceleration
	PresentVsync = 0x00000004, // Present is synchronized with the refresh rate
	TargetTexture = 0x00000008 //The renderer supports rendering to texture
}
