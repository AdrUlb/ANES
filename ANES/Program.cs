namespace ANES;

using Sdl3Sharp;
using System.Runtime.InteropServices;

internal static class Program
{
	public static int Main()
	{
#if MY_SET_DLLIMPORTRESOLVER
		NativeLibrary.SetDllImportResolver(typeof(SdlApp).Assembly, (name, _, _) =>
		{
			if (name != "SDL3")
				return 0;

			var (ridOs, libPrefix, libSuffix) =
				RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ("win", "", ".dll") :
				RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? ("linux", "lib", ".so") :
				throw new PlatformNotSupportedException("Operating system is not supported.");

			var ridArch = RuntimeInformation.ProcessArchitecture switch
			{
				Architecture.X64 => "x64",
				Architecture.X86 => "x86",
				_ => throw new PlatformNotSupportedException("Architecture is not supported.")
			};

			var rid = ridOs + '-' + ridArch;

			var libFilePath = Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native", $"{libPrefix}SDL3{libSuffix}");

			return File.Exists(libFilePath) ? NativeLibrary.Load(libFilePath) : 0;

		});
#endif

		return new App().Run();
	}
}
