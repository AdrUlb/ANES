using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace Sdl3Sharp;

public static unsafe partial class Sdl
{
	private const string libraryName = "SDL3";

	[LibraryImport(libraryName)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SDL_Init(SdlInitFlags flags);

	[LibraryImport(libraryName)]
	private static partial void SDL_Quit();

	[LibraryImport(libraryName)]
	[return: MarshalAs(UnmanagedType.LPUTF8Str)]
	private static partial string SDL_GetError();

	[LibraryImport(libraryName)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SDL_CreateWindowAndRenderer([MarshalAs(UnmanagedType.LPUTF8Str)] string title, int width, int height, SdlWindowFlags windowFlags, out SdlWindowPtr window, out SdlRendererPtr renderer);
	
	public static bool Init(SdlInitFlags flags) => SDL_Init(flags);
	public static void Quit() => SDL_Quit();
	public static string GetError() => SDL_GetError();

	public static bool CreateWindowAndRenderer(string title, int width, int height, SdlWindowFlags windowFlags, out SdlWindowPtr window, out SdlRendererPtr renderer) =>
		SDL_CreateWindowAndRenderer(title, width, height, windowFlags, out window, out renderer);
}
