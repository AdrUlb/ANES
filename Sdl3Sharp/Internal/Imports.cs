using System.Runtime.InteropServices;
using SDL_PropertiesID = uint;

namespace Sdl3Sharp.Internal;

internal static partial class Imports
{
	internal delegate SdlAppResult SdlAppInitFunc(ref nint appstate, int argc, string[] argv);
	internal delegate SdlAppResult SdlAppIterateFunc(nint appstate);
	internal delegate SdlAppResult SdlAppEventFunc(nint appstate, SdlEvent sdlEvent);
	internal delegate void SdlAppQuitFunc(nint appstate, SdlAppResult result);

	private const string _libraryName = "SDL3";

	[LibraryImport(_libraryName, StringMarshalling = StringMarshalling.Utf8)]
	internal static partial int SDL_EnterAppMainCallbacks(int argc, [In] string[] argv, SdlAppInitFunc appInit, SdlAppIterateFunc appIter, SdlAppEventFunc appEvent, SdlAppQuitFunc appQuit);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_Init(SdlInitFlags flags);

	[LibraryImport(_libraryName)]
	internal static partial void SDL_Quit();

	[LibraryImport(_libraryName, StringMarshalling = StringMarshalling.Utf8)]
	internal static partial string SDL_GetError();

	[LibraryImport(_libraryName, StringMarshalling = StringMarshalling.Utf8)]
	internal static partial nint SDL_CreateWindow(string title, int w, int h, SdlWindowFlags flags);

	[LibraryImport(_libraryName)]
	internal static partial nint SDL_CreateWindowWithProperties(SDL_PropertiesID props);

	[LibraryImport(_libraryName)]
	internal static partial SDL_PropertiesID SDL_GetWindowProperties(nint window);

	[LibraryImport(_libraryName, StringMarshalling = StringMarshalling.Utf8)]
	internal static partial nint SDL_CreateRenderer(nint window, string name);

	[LibraryImport(_libraryName)]
	internal static partial nint SDL_CreateRenderer(nint window, nint name);

	[LibraryImport(_libraryName, StringMarshalling = StringMarshalling.Utf8)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_CreateWindowAndRenderer(string title, int width, int height, SdlWindowFlags windowFlags, out nint window, out nint renderer);

	[LibraryImport(_libraryName)]
	internal static partial void SDL_DestroyRenderer(nint renderer);

	[LibraryImport(_libraryName)]
	internal static partial void SDL_DestroyWindow(nint window);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_SetWindowSize(nint window, int w, int h);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_GetRenderVSync(nint renderer, out int vsync);
	
	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_SetRenderDrawColor(nint renderer, byte r, byte g, byte b, byte a);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_RenderClear(nint renderer);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_RenderPresent(nint renderer);

	[LibraryImport(_libraryName)]
	internal static partial SDL_PropertiesID SDL_CreateProperties();

	[LibraryImport(_libraryName)]
	internal static partial void SDL_DestroyProperties(SDL_PropertiesID props);

	[LibraryImport(_libraryName, StringMarshalling = StringMarshalling.Utf8)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_SetNumberProperty(SDL_PropertiesID props, string name, long value);

	[LibraryImport(_libraryName, StringMarshalling = StringMarshalling.Utf8)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_SetPointerProperty(SDL_PropertiesID props, string name, nint value);

	[LibraryImport(_libraryName, StringMarshalling = StringMarshalling.Utf8)]
	internal static partial nint SDL_GetPointerProperty(SDL_PropertiesID props, string name, nint default_value);

	[LibraryImport(_libraryName)]
	internal static partial nint SDL_CreateTextureWithProperties(nint renderer, SDL_PropertiesID props);

	[LibraryImport(_libraryName)]
	internal static partial void SDL_DestroyTexture(nint texture);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static unsafe partial bool SDL_RenderTexture(nint renderer, nint texture, SdlFRect* srcrect, SdlFRect* dstrect);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_SetRenderScale(nint renderer, float scaleX, float scaleY);

	[LibraryImport(_libraryName, StringMarshalling = StringMarshalling.Utf8)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_RenderDebugText(nint renderer, float x, float y, string str);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static unsafe partial bool SDL_LockTextureToSurface(nint texture, SdlRect* rect, out nint surface);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static unsafe partial bool SDL_SetTextureScaleMode(nint texture, SdlScaleMode scaleMode);

	[LibraryImport(_libraryName)]
	internal static partial void SDL_UnlockTexture(nint texture);

	[LibraryImport(_libraryName)]
	internal static partial nint SDL_CreateSurface(int width, int height, SdlPixelFormat format);

	[LibraryImport(_libraryName)]
	internal static partial void SDL_DestroySurface(nint surface);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static unsafe partial bool SDL_BlitSurface(nint src, SdlRect* srcrect, nint dst, SdlRect* dstrect);

	[LibraryImport(_libraryName)]
	internal static partial SdlWindowId SDL_GetWindowID(nint window);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_ShowWindow(nint window);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_HideWindow(nint window);
}
