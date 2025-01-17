using ANES.Rendering.Sdl3;
using Sdl3Sharp;
using System.Diagnostics;

namespace ANES.Platform.Sdl3;

internal static class Program
{
	public static void Main()
	{
		if (OperatingSystem.IsWindows())
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

#if !PUBLISH
		AnesSdlRenderer.SetRuntimeImportResolver();
#endif

		Sdl.Init(SdlInitFlags.Video);
		new App().Run();
	}
}
