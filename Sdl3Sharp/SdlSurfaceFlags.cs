namespace Sdl3Sharp;

public enum SdlSurfaceFlags : uint
{
	Preallocated = 0x00000001u, // Surface uses preallocated pixel memory
	LockNeeded = 0x00000002u, // Surface needs to be locked to access pixels
	Locked = 0x00000004u, // Surface is currently locked
	SimdAligned = 0x00000008u // Surface uses pixel memory allocated with SDL_aligned_alloc()
}
