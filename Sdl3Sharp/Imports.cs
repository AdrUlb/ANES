using System.Runtime.InteropServices;

namespace Sdl3Sharp;

internal static partial class Imports
{
	private const string _libraryName = "SDL3";

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_Init(SdlInitFlags flags);

	[LibraryImport(_libraryName)]
	internal static partial void SDL_Quit();

	[LibraryImport(_libraryName, StringMarshalling = StringMarshalling.Utf8)]
	internal static partial string SDL_GetError();

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_PollEvent(out SdlEvent sdlEvent);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static unsafe partial bool SDL_AddEventWatch(Sdl.EventFilter filter, nint userdata);

	[LibraryImport(_libraryName, StringMarshalling = StringMarshalling.Utf8)]
	internal static partial SdlWindowPtr SDL_CreateWindow(string title, int w, int h, SdlWindowFlags flags);

	[LibraryImport(_libraryName)]
	internal static partial float SDL_GetWindowOpacity(SdlWindowPtr window);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_SetWindowOpacity(SdlWindowPtr window, float opacity);

	[LibraryImport(_libraryName, StringMarshalling = StringMarshalling.Utf8)]
	internal static partial SdlWindowPtr SDL_CreatePopupWindow(SdlWindowPtr parent, int offsetX, int offsetY, int w, int h, SdlWindowFlags flags);

	[LibraryImport(_libraryName)]
	internal static partial SdlWindowPtr SDL_CreateWindowWithProperties(SdlPropertiesId props);

	[LibraryImport(_libraryName)]
	internal static partial SdlPropertiesId SDL_GetWindowProperties(SdlWindowPtr window);

	[LibraryImport(_libraryName, StringMarshalling = StringMarshalling.Utf8)]
	internal static partial SdlRendererPtr SDL_CreateRenderer(SdlWindowPtr window, string name);

	[LibraryImport(_libraryName)]
	internal static partial SdlRendererPtr SDL_CreateRenderer(SdlWindowPtr window, nint name);

	[LibraryImport(_libraryName, StringMarshalling = StringMarshalling.Utf8)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_CreateWindowAndRenderer(string title, int width, int height, SdlWindowFlags windowFlags, out SdlWindowPtr window, out SdlRendererPtr renderer);

	[LibraryImport(_libraryName)]
	internal static partial void SDL_DestroyRenderer(SdlRendererPtr renderer);

	[LibraryImport(_libraryName)]
	internal static partial void SDL_DestroyWindow(SdlWindowPtr window);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_SetWindowSize(SdlWindowPtr window, int w, int h);

	[LibraryImport(_libraryName)]
	internal static partial SdlWindowId SDL_GetWindowID(SdlWindowPtr window);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_ShowWindow(SdlWindowPtr window);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_HideWindow(SdlWindowPtr window);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_GetRenderVSync(SdlRendererPtr renderer, out int vsync);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_SetRenderDrawColor(SdlRendererPtr renderer, byte r, byte g, byte b, byte a);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_RenderClear(SdlRendererPtr renderer);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_RenderPresent(SdlRendererPtr renderer);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static unsafe partial bool SDL_RenderFillRect(SdlRendererPtr renderer, SdlFRect* rect);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static unsafe partial bool SDL_SetRenderClipRect(SdlRendererPtr renderer, SdlRect* rect);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_GetRenderClipRect(SdlRendererPtr renderer, out SdlRect rect);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_RenderClipEnabled(SdlRendererPtr renderer);

	[LibraryImport(_libraryName)]
	internal static partial SdlPropertiesId SDL_CreateProperties();

	[LibraryImport(_libraryName)]
	internal static partial void SDL_DestroyProperties(SdlPropertiesId props);

	[LibraryImport(_libraryName, StringMarshalling = StringMarshalling.Utf8)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_SetNumberProperty(SdlPropertiesId props, string name, long value);

	[LibraryImport(_libraryName, StringMarshalling = StringMarshalling.Utf8)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_SetPointerProperty(SdlPropertiesId props, string name, nint value);

	[LibraryImport(_libraryName, StringMarshalling = StringMarshalling.Utf8)]
	internal static partial nint SDL_GetPointerProperty(SdlPropertiesId props, string name, nint defaultValue);

	[LibraryImport(_libraryName)]
	internal static partial SdlTexturePtr SDL_CreateTextureWithProperties(SdlRendererPtr renderer, SdlPropertiesId props);

	[LibraryImport(_libraryName)]
	internal static partial void SDL_DestroyTexture(SdlTexturePtr texture);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static unsafe partial bool SDL_RenderTexture(SdlRendererPtr renderer, SdlTexturePtr texture, SdlFRect* srcrect, SdlFRect* dstrect);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_SetRenderScale(SdlRendererPtr renderer, float scaleX, float scaleY);

	[LibraryImport(_libraryName, StringMarshalling = StringMarshalling.Utf8)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool SDL_RenderDebugText(SdlRendererPtr renderer, float x, float y, string str);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static unsafe partial bool SDL_LockTextureToSurface(SdlTexturePtr texture, SdlRect* rect, out SdlSurfacePtr surface);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static unsafe partial bool SDL_SetTextureScaleMode(SdlTexturePtr texture, SdlScaleMode scaleMode);

	[LibraryImport(_libraryName)]
	internal static partial void SDL_UnlockTexture(SdlTexturePtr texture);

	[LibraryImport(_libraryName)]
	internal static partial SdlSurfacePtr SDL_CreateSurface(int width, int height, SdlPixelFormat format);

	[LibraryImport(_libraryName)]
	internal static partial void SDL_DestroySurface(SdlSurfacePtr surface);

	[LibraryImport(_libraryName)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static unsafe partial bool SDL_BlitSurface(SdlSurfacePtr src, SdlRect* srcrect, SdlSurfacePtr dst, SdlRect* dstrect);
}
