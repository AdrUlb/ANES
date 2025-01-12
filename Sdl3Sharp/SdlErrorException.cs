using static Sdl3Sharp.Internal.Imports;

namespace Sdl3Sharp;

public class SdlErrorException() : Exception("SDL Error: " + SDL_GetError());
