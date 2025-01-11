using ANES.Rendering.Sdl3;

namespace ANES.Platform.Sdl3;

internal static class Program
{
	public static int Main()
	{
#if !PUBLISH
		AnesSdlRenderer.SetRuntimeImportResolver();
#endif

		return new App().Run();
	}
}
