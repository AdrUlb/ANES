using static Sdl3Sharp.Internal.Imports;

namespace Sdl3Sharp;

public sealed class SdlErrorException() : Exception("SDL Error: " + SDL_GetError());
